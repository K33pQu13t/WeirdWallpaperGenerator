using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models;
using WeirdWallpaperGenerator.Services.Serialization;

namespace WeirdWallpaperGenerator.Services
{
    public class UpdateService
    {
        HttpClient _client;
        private const string contentsUrlTemplate = "https://api.github.com/repos/{0}/contents";
        private const string branch = "?ref=master";
        private const string _repo = "K33pQu13t/WeirdWallpaperGenerator";

        private const string configFileName = "config.json";
        private const string hashTableFileName = "hashtable";
        private const string pdbFile = "WeirdWallpaperGenerator.pdb";
        private const string updateButchFile = "update.bat";
        private const string whatsNewFile = "whats new.txt";

        public string ReleaseFolderName => "Release build";

        private const int countOfDownloadsRetry = 1;

        string ConfigPath => Path.Combine(ReleaseFolderName, configFileName);
        public string UpdatePath => Path.Combine(Path.GetTempPath(), ReleaseFolderName);
        public string ConfigFileUpdatePath => Path.Combine(UpdatePath, configFileName);
        public string HashTableFileUpdatePath => Path.Combine(UpdatePath, hashTableFileName);
        private string UpdateBatchFilePath => Path.Combine(UpdatePath, updateButchFile);
        private string WhatsNewFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, whatsNewFile);

        readonly string _mainUrl;

        static readonly string[] positiveAnswers = new string[] { "y", "yes", "н" };
        static readonly string[] negativeAnswers = new string[] { "n", "no", "т" };

        BinarySerializationService _serializationService = new BinarySerializationService();
        JsonSerializationService _jsonSerializationService = new JsonSerializationService();
        MessagePrinterService _systemMessagePrinter;
        ContextConfig _contextConfig = ContextConfig.GetInstance();

        public UpdateService()
        {
            _systemMessagePrinter = MessagePrinterService.GetInstance();

            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Updater", "1"));

            _mainUrl = string.Format(contentsUrlTemplate, new string[] { _repo });
        }

        /// <summary>
        /// checks if there is a need of update and download it if needed
        /// </summary>
        /// <param name="isManual">true if updating from /update command, not automatically</param>
        /// <returns></returns>
        public async Task CheckUpdates(bool isManual = false)
        {
#if DEBUG
            _systemMessagePrinter.PrintLog("Check updates");
#endif
            if (await ShouldUpdate(isManual))
            {
#if DEBUG
                _systemMessagePrinter.PrintLog("Downloading updates");
#endif
                if (isManual)
                {
                    Config updateConfig = (Config)_jsonSerializationService.Deserialize(ConfigFileUpdatePath, typeof(Config));
                    _systemMessagePrinter.PrintLog($"Newer version ({updateConfig.About.Version}) found. Downloading...", false);
                }

                await GetUpdate(ReleaseFolderName);
                await ShouldUpdateOnExit();
            }
            else if (isManual)
                _systemMessagePrinter.PrintLog("App is up to date, no need to update", false);

            _contextConfig.UpdaterSettings.LastUpdateCheckDate = DateTime.Now.Date;
        }

        public async Task ShouldUpdateOnExit()
        {
            if (await IsUpdateReady())
            {
                ContextConfig.GetInstance().ShouldUpdateOnExit = true;
            }
        }

        /// <summary>
        /// downloads only one file - config.json, to determine is newer version available
        /// </summary>
        /// <param name="isManual">true if updating from /update command, not automatically</param>
        /// <returns></returns>
        public async Task<bool> ShouldUpdate(bool isManual = false)
        {
            // if it ran automatically then config should be checked
            if (!_contextConfig.UpdaterSettings.AutoCheckUpdates 
                && !isManual)
                return false;

            // if its not time yet - skip
            var lastDateUpdate = _contextConfig.UpdaterSettings.LastUpdateCheckDate;
            if (lastDateUpdate.AddDays(_contextConfig.UpdaterSettings.CheckPeriodDays) > DateTime.Now.Date
                && !isManual)
                return false;

            if (isManual)
                _systemMessagePrinter.PrintLog("Update started", false);

            DeleteTempPath();
            await GetUpdate(ConfigPath);

            Config configOfUpdate = GetConfigFromUpdateFolder();
            // compare it with current
            return VersionComparer(_contextConfig.About.Version, configOfUpdate.About.Version) == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if all needed files was downloaded, integrity is ensured</returns>
        public async Task<bool> IsUpdateReady()
        {
            return await IsUpdateReady(0);
        }

        private async Task<bool> IsUpdateReady(int countOfTry = 0)
        {
            // that files somehow changes they hash
            string[] excludeFileNames = new string[]
            {
                "WeirdWallpaperGenerator.deps.json",
                "WeirdWallpaperGenerator.runtimeconfig.json",
                "WeirdWallpaperGenerator.runtimeconfig.dev.json",
                "whats new.txt"
            };

            List<string> filenamesWithBadIntegrity = new List<string>() { };

            HashTable hashtable = (HashTable)_serializationService.Deserialize(HashTableFileUpdatePath);
            Dictionary<string, string> hashesFromUpdateFolder =
                HashHelper.GetSHA1ChecksumFromFolder(
                    UpdatePath,
                    new string[] { configFileName, hashTableFileName, pdbFile }
                    );
            foreach (var hashPair in hashesFromUpdateFolder)
            {
                if (excludeFileNames.Contains(hashPair.Value))
                    continue;

                // if some of downloaded file's hashes didn't represent in hashtable (probably means integrity issue)
                if (!hashtable.Table.ContainsKey(hashPair.Key))
                    filenamesWithBadIntegrity.Add(hashPair.Value);
            }

            if (filenamesWithBadIntegrity.Any() )
            {
                if (countOfTry < countOfDownloadsRetry)
                {
                    List<string> hashesToExclude = hashesFromUpdateFolder.Where(
                        h => !filenamesWithBadIntegrity.Contains(h.Value)).Select(h => h.Key).ToList();
                    await GetUpdate(ReleaseFolderName, hashesToExclude);
                    if (await IsUpdateReady(++countOfTry) == false)
                        return false;
                    else return true;
                }
                else
                {
                    // setting the flag what there was integrity issue
                    _contextConfig.UpdateCorrupted = true;
                    return false;
                }
            }

            _contextConfig.UpdateCorrupted = false;
            _contextConfig.VersionFromUpdate = hashtable.Version;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isManual">true if updating from /update command, not automatically</param>
        /// <returns></returns>
        public async Task CheckUpdateBeforeExit(bool isManual = false)
        {
            // wait for update to download
            if (_contextConfig.UpdateLoading != null && _contextConfig.UpdateLoading.Status == TaskStatus.WaitingForActivation)
                await _contextConfig.UpdateLoading;

            if (_contextConfig.ShouldUpdateOnExit)
            {
                if (_contextConfig.UpdaterSettings.AskBeforeUpdate && !isManual)
                {
                    _systemMessagePrinter.PrintWarning(
                        $"A new version {_contextConfig.VersionFromUpdate} of the programm is ready " +
                        $"(your's is {_contextConfig.About.Version}). " +
                        $"Do you want to update it? (y/n)", 
                        putPrefix: false);
                    string answer = string.Empty;
                    while (!(positiveAnswers.Contains(answer.ToLower()) 
                        || negativeAnswers.Contains(answer.ToLower())))
                    {
                        answer = Console.ReadLine();
                    }

                    if (negativeAnswers.Contains(answer))
                        return;
                }

                // if not automatically updation - show message
                if (_contextConfig.UpdaterSettings.AskBeforeUpdate || isManual)
                    _systemMessagePrinter.PrintLog("Copying updated files...", false);
                CopyUpdateToWorkFolder();
                Environment.Exit(0);
            }
            else if (_contextConfig.UpdateCorrupted)
            {
                throw ExceptionHelper.GetException(
                    nameof(UpdateService),
                    nameof(CheckUpdates),
                    "Tried to update, some of downloaded files was corrupted. " +
                    "That probably means Internet interruptions " +
                    "or an error on the server. Try to fix Internet connection or " +
                    "download update manualy: \"https://github.com/K33pQu13t/WeirdWallpaperGenerator/Release build\"");
            }
        }

        private Config GetConfigFromUpdateFolder()
        {
            return (Config)_jsonSerializationService.Deserialize(ConfigFileUpdatePath, typeof(Config));
        }

        public async Task GetUpdate(string pathToUpdateGithubFolderOrFile, List<string> hashesToExclude = null)
        {
            if (hashesToExclude == null)
                hashesToExclude = new List<string>();

            hashesToExclude.AddRange(GetCurrentVersionFilesGithubSha1Hashes());

            try
            {
                Dictionary<string, string> filesToDownload = 
                await GetFilesToDownload(_mainUrl, pathToUpdateGithubFolderOrFile, hashesToExclude);

                foreach (var fileToDownload in filesToDownload)
                {
                    await Download(fileToDownload.Key, Path.Combine(UpdatePath, fileToDownload.Value.Replace('/', '\\')));
                }
            }
            catch (HttpRequestException)
            {
                throw ExceptionHelper.GetException(nameof(UpdateService), nameof(GetUpdate),
                    "Failed attempt to get update. It's probably an Internet connection problem");
            }
        }

        public void CopyUpdateToWorkFolder()
        {
            var files = Directory.GetFiles(UpdatePath)
                .Where(fileName => !(new string[] {
                    configFileName, 
                    hashTableFileName
                }).Contains(Path.GetFileName(fileName))).ToList();

            string commands = string.Join('\n',
                GenerateDeletionCommand(files, AppDomain.CurrentDomain.BaseDirectory),
                GenerateCopyCommand(files, AppDomain.CurrentDomain.BaseDirectory),
                GenerateStartWhatsNewCommand(),
                GenerateCleanup()
                );

            using (StreamWriter sw = new StreamWriter(UpdateBatchFilePath, false))
            {
                sw.Write(GenerateFinalCommand(commands));
            }

            UpdateConfig();

            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(UpdatePath, "update.bat"))
            {
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private void UpdateConfig()
        {
            Config configOfUpdate = GetConfigFromUpdateFolder();
            _contextConfig.About.Version = configOfUpdate.About.Version;
            _contextConfig.About.ReleaseDate = configOfUpdate.About.ReleaseDate;
            ContextConfig.Save();
        }

        private string GetDeleteCommand()
        {
            return "del /f /q";
        }

        private string GetCopyCommand()
        {
            return "copy /y";
        }

        private string GenerateDeletionCommand(List<string> filesToDelete, string pathWhereDelete)
        {
            List<string> commands = new List<string>();
            foreach (var filePath in filesToDelete)
            {
                var fileName = Path.GetFileName(filePath);
                commands.Add($"{GetDeleteCommand()} " +
                    $"\"{Path.GetFullPath(Path.Combine(pathWhereDelete, fileName))}\"");
            }

            return string.Join("\n", commands);
        }

        private string GenerateCopyCommand(List<string> filesToCopy, string pathWhereCopy)
        {
            List<string> commands = new List<string>();
            foreach (var filePath in filesToCopy)
            {
                var fileName = Path.GetFileName(filePath);
                commands.Add($"{GetCopyCommand()} " +
                    $"\"{filePath}\" " +
                    $"\"{Path.GetFullPath(Path.Combine(pathWhereCopy, fileName))}\"");
            }

            return string.Join("\n", commands);
        }

        private string GenerateCleanup()
        {
            return $"rmdir /s /q \"{UpdatePath}\"";
        }

        private string GenerateStartWhatsNewCommand()
        {
            return $"\"{WhatsNewFilePath}\"";
        }
        
        private string GenerateFinalCommand(string commands)
        {
            return $"echo @off timeout /t 1\n{commands}";
        }

        private async Task Download(string url, string path)
        {
            var response = await _client.GetAsync(url);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        private async Task<object> GetContents(string url)
        {
            string contentsJson = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject(contentsJson);
        }

        private async Task<Dictionary<string, string>> GetFilesToDownload(string url, string pathToGithubFileOrFolder = "", List<string> hashesToExclude = null)
        {
            Dictionary<string, string> filesToDownload = new Dictionary<string, string>();
            url += $"{(!string.IsNullOrWhiteSpace(pathToGithubFileOrFolder) ? $"/{pathToGithubFileOrFolder.Replace('\\', '/')}{branch}" : "")}";
            object contents = await GetContents(url);
            if (contents is JArray contentsArray)
            {
                foreach (var file in contentsArray)
                {
                    var fileType = (string)file["type"];
                    if (fileType == "file")
                    {
                        var hash = (string)file["sha"];
                        if (hashesToExclude != null && hashesToExclude.Contains(hash))
                            continue;

                        var downloadUrl = (string)file["download_url"];
                        string filename = string.Join('/', ((string)file["path"]).Split('/')[1..]);
                        filesToDownload.Add(downloadUrl, filename);
                    }
                    else if (fileType == "dir")
                    {
                        var directoryContentsUrl = (string)file["url"];

                        await GetFilesToDownload(directoryContentsUrl);
                    }
                }
            }
            else if (contents is JObject contentObject)
            {
                var fileType = (string)contentObject["type"];
                if (fileType == "file")
                {
                    var downloadUrl = (string)contentObject["download_url"];
                    string filename = string.Join('/', ((string)contentObject["path"]).Split('/')[1..]);
                    filesToDownload.Add(downloadUrl, filename);
                }
                else if (fileType == "dir")
                {
                    var directoryContentsUrl = (string)contentObject["url"];

                    await GetFilesToDownload(directoryContentsUrl);
                }
            }

            return filesToDownload;
        }

        private void DeleteTempPath()
        {
            if (Directory.Exists(UpdatePath))
            {
                Directory.Delete(UpdatePath, true);
            }
        }

        public List<string> GetCurrentVersionFilesGithubSha1Hashes()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            List<string> hashes = new List<string>();

            string[] filesPaths = Directory.GetFiles(path);
            foreach (string filePath in filesPaths)
            {
                hashes.Add(HashHelper.GetSHA1ChecksumGithub(filePath));
            }

            return hashes;
        }

        /// <returns>-1 if version1 is higher. 1 if version2 is higher. 0 if equals </returns>
        private int VersionComparer(string version1, string version2)
        {
            if (string.IsNullOrEmpty(version1))
                return 1;
            if (string.IsNullOrEmpty(version2))
                return -1;
            if (version1 == version2)
                return 0;

            string[] version1Stack = version1.Split('.');
            string[] version2Stack = version2.Split('.');
            if (version1Stack.Length > version2Stack.Length)
                return -1;
            if (version1Stack.Length < version2Stack.Length)
                return 1;

            for (int i = 0; i < version1Stack.Length; i++)
            {
                if (int.Parse(version1Stack[i]) > int.Parse(version2Stack[i]))
                    return -1;
                if (int.Parse(version1Stack[i]) < int.Parse(version2Stack[i]))
                    return 1;
            }

            return 0;
        }
    }
}
