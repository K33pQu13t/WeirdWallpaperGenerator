using Microsoft.Extensions.Configuration;
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
        const string contentsUrlTemplate = "https://api.github.com/repos/{0}/contents"; //?ref=master";
        //const string branch = "?ref=master";
        const string branch = "?ref=feature/add_auto_updater";
        const string _repo = "K33pQu13t/WeirdWallpaperGenerator";


        const string configFileName = "config.json";
        const string hashTableFileName = "hashtable";

        public string ReleaseFolderName => "Release build";

        string ConfigPath => Path.Combine(ReleaseFolderName, configFileName);
        public string UpdatePath => Path.Combine(Path.GetTempPath(), ReleaseFolderName);
        public string UpdateConfigFilePath => Path.Combine(UpdatePath, configFileName);

        readonly string _mainUrl;

        static readonly string[] positiveAnswers = new string[] { "y", "yes" };
        static readonly string[] negativeAnswers = new string[] { "n", "no" };

        BinarySerializationService _serializationService = new BinarySerializationService();
        JsonSerializationService _jsonSerializationService = new JsonSerializationService();

        public UpdateService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Updater", "1"));

            _mainUrl = string.Format(contentsUrlTemplate, new string[] { _repo });
        }

        /// <summary>
        /// downloads only one file - config.json, to determine is newer version avaible
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ShouldUpdate()
        {
            var _contextConfig = ContextConfig.GetInstance();
            if (!_contextConfig.UpdaterSettings.AutoCheckUpdates)
                return false;

            // if its not time yet - skip
            var lastDateUpdate = _contextConfig.UpdaterSettings.LastUpdateDate;
            if (lastDateUpdate.AddDays(_contextConfig.UpdaterSettings.CheckPeriodDays) > DateTime.Now.Date)
                return false;

            DeleteTempPath();
            await GetUpdate(ConfigPath, UpdatePath);

            Config configOfUpdate = GetConfigFromUpdateFolder();
            // compare it with current
            return VersionComparer(ContextConfig.GetInstance().About.Version, configOfUpdate.About.Version) == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if all needed files was downloaded, integrity is ensured</returns>
        public bool IsUpdateReady()
        {
            // that files somewhy changes they content so hash changed anyway
            string[] excludeFileNames = new string[] 
            { 
                "WeirdWallpaperGenerator.deps.json", 
                "WeirdWallpaperGenerator.runtimeconfig.json" 
            };

            HashTable hashtable = (HashTable)_serializationService.Deserialize(Path.Combine(UpdatePath, hashTableFileName));
            Dictionary<string, string> hashesFromUpdateFolder = HashHelper.GetSHA1ChecksumFromFolder(UpdatePath, new string[] { configFileName, hashTableFileName });
            foreach(var hashPair in hashesFromUpdateFolder)
            {
                if (excludeFileNames.Contains(hashPair.Value))
                    continue;

                // if some of downloaded file's hashes didn't represent in hashtable (probably means integrity issue)
                if (!hashtable.Table.ContainsKey(hashPair.Key))
                    return false;
            }
            return true;
        }

        public async Task CheckUpdateBeforeExit()
        {
            var context = ContextConfig.GetInstance();
            // wait for update to download
            await context.UpdateLoading;

            if (context.ShouldUpdateOnExit)
            {
                if (context.UpdaterSettings.AskBeforeUpdate)
                {
                    Console.WriteLine("A new version of the programm is ready. Update it? (y/n)");
                    string answer = string.Empty;
                    while (!(positiveAnswers.Contains(answer) || negativeAnswers.Contains(answer)))
                    {
                        answer = Console.ReadLine();
                    }

                    if (negativeAnswers.Contains(answer))
                        return;
                }

                // TODO: start cmd process of cutting-pasting-deleting-running procces of updation here
                Console.WriteLine("update started");
                //CopyUpdateToWorkFolder();
                //context.UpdaterSettings.LastUpdateDate = DateTime.Now.Date;
                // TODO: need to save config.json to update information
                Environment.Exit(0);
            }
        }

        private Config GetConfigFromUpdateFolder()
        {
            return (Config)_jsonSerializationService.Deserialize(UpdateConfigFilePath, typeof(Config));
        }

        public async Task GetUpdate(string pathToUpdateGithubFolderOrFile, string pathToSave, List<string> hashesToExclude = null)
        {
            if (hashesToExclude == null)
                hashesToExclude = GetCurrentVersionFilesGithubSha1Hashes();

            Dictionary<string, string> filesToDownload = 
                await GetFilesToDownload(_mainUrl, pathToUpdateGithubFolderOrFile, hashesToExclude);
            foreach (var fileToDownload in filesToDownload)
            {
                await Download(fileToDownload.Key, Path.Combine(pathToSave, fileToDownload.Value.Replace('/', '\\')));
            }
        }

        public void CopyUpdateToWorkFolder()
        {
            Process.Start("cmd.exe", $@"move {UpdatePath} {Environment.CurrentDirectory}"); // TODO: && "what's new.txt");
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
            var path = Environment.CurrentDirectory;
            List<string> hashes = new List<string>();

            string[] filesPaths = Directory.GetFiles(path);
            foreach (string filePath in filesPaths)
            {
                hashes.Add(HashHelper.GetSHA1ChecksumGithub(filePath));
            }

            return hashes;
        }

        /// <returns>-1 if version 1 is higher. 1 if version 2 is higher. 0 if equals </returns>
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
