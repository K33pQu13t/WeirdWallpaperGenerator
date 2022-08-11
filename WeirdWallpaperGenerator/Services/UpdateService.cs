using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Config;
using WeirdWallpaperGenerator.Helpers;

namespace WeirdWallpaperGenerator.Services
{
    public class UpdateService
    {
        HttpClient _client;
        const string contentsUrlTemplate = "https://api.github.com/repos/{0}/contents"; //?ref=master";
        const string branch = "?ref=master";
        const string _repo = "K33pQu13t/WeirdWallpaperGenerator";

        public string ReleaseFolder => "Release build";
        const string configFile = "config.json";
        string ConfigPath => Path.Combine(ReleaseFolder, configFile);
        public string TempPath => Path.Combine(Path.GetTempPath(), ReleaseFolder);

        readonly string _mainUrl;

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
            if (!ContextConfig.GetInstance().UpdaterConfig.AutoCheckUpdates)
                return false;

            DeleteTempPath();
            await GetUpdate(ConfigPath, TempPath);

            IConfiguration config = GetConfigFromUpdateFolder();
            // get about section from last loaded version in repo
            var about = config.GetSection("About").Get<About>();
            // compare it with current
            return VersionComparer(ContextConfig.GetInstance().About.Version, about.Version) == 1;
        }

        public bool IsUpdateReady()
        {
            IConfiguration config = GetConfigFromUpdateFolder();
            var about = config.GetSection("About").Get<About>();

            string updateHash = HashHelper.GetMD5ChecksumFromFolder(TempPath);

            return !string.IsNullOrWhiteSpace(updateHash) && updateHash != about.Hash;
        }

        private IConfiguration GetConfigFromUpdateFolder()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(TempPath, ConfigPath), optional: false);
            return builder.Build();
        }

        public async Task GetUpdate(string pathToUpdateGithubFolderOrFile, string pathToSave)
        {
            Dictionary<string, string> filesToDownload = await GetFilesToDownload(_mainUrl, pathToUpdateGithubFolderOrFile);
            foreach (var fileToDownload in filesToDownload)
            {
                await Download(fileToDownload.Key, Path.Combine(pathToSave, fileToDownload.Value.Replace('/', '\\')));
            }
        }

        private async Task Download(string url, string path)
        {
            var response = await _client.GetAsync(url);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(
                path,
                FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        private async Task<JArray> GetContents(string url)
        {
            string contentsJson = await _client.GetStringAsync(url);
            return (JArray)JsonConvert.DeserializeObject(contentsJson);
        }

        private async Task<Dictionary<string, string>> GetFilesToDownload(string url, string pathToGithubFileOrFolder = "")
        {
            Dictionary<string, string> filesToDownload = new Dictionary<string, string>();
            url = $"{url}{(!string.IsNullOrWhiteSpace(pathToGithubFileOrFolder) ? $"/{pathToGithubFileOrFolder}{branch}" : "")}";
            JArray contents = await GetContents(url);
            foreach (var file in contents)
            {
                var fileType = (string)file["type"];
                if (fileType == "file")
                {
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
            return filesToDownload;
        }

        private void DeleteTempPath()
        {
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath, true);
            }
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
