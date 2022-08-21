using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models;
using WeirdWallpaperGenerator.Services.Serialization;

namespace WeirdWallpaperGenerator.Services
{
    internal class ReleasePreparingService
    {
        const string buildFolder = @"..\..\..\..\Release build";
        const string configFileName = "config.json";
        const string hashTableFileName = "hashtable";

        const string colorsFolderName = "colors";

        string ConfigFileBuildPath => Path.Combine(buildFolder, configFileName);
        string ColorsFolderBuildPath => Path.Combine(buildFolder, colorsFolderName);
        string ColorsFolderDevPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, colorsFolderName);

        readonly string[] garbage = new string[] { "WeirdWallpaperGenerator.pdb" };

        SystemMessagePrinter _printer;
        BinarySerializationService _binarySerializationService = new BinarySerializationService();
        JsonSerializationService _jsonSerializationService = new JsonSerializationService();

        internal enum VersionStack
        {
            Major,
            Minor,
            Patch
        }

        public ReleasePreparingService()
        {
            _printer = SystemMessagePrinter.GetInstance();
        }

        internal void Prepare(VersionStack versionUpdate)
        {
            RemoveGarbage();
            //CopyColors();
            CopyConfigWithIncrementVersion(versionUpdate);
            SetReleaseDate();
            GenerateHashTable();
        }

        internal void GenerateHashTable()
        {
            var config = GetConfigFromBuildFolder();
            HashTable hashTable = new HashTable
            {
                Version = config.About.Version,
                Table = HashHelper.GetSHA1ChecksumFromFolder(buildFolder, new string[] { configFileName, hashTableFileName })
            };

            _binarySerializationService.Serialize(Path.Combine(buildFolder, hashTableFileName).ToString(), hashTable);

            _printer.PrintLog("Hash table created");
        }

        internal void CopyColors()
        {
            RemoveDuplicatedColors();
            Directory.CreateDirectory(ColorsFolderBuildPath);
            var colorsPaths = Directory.GetFiles(ColorsFolderDevPath);
            foreach (var path in colorsPaths)
            {
                var saveFilePath = Path.Combine(ColorsFolderBuildPath, Path.GetFileName(path));
                File.Copy(ColorsFolderDevPath, saveFilePath, true);
            }
            _printer.PrintLog("Colors copied");
        }

        internal void RemoveGarbage()
        {
            foreach (var fileNameToDelete in garbage)
            {
                File.Delete(Path.Combine(buildFolder, fileNameToDelete));
            }
            _printer.PrintLog("Garbage removed");
        }

        internal void RemoveDuplicatedColors()
        {
            var files = Directory.GetFiles("colors");
            foreach (var file in files)
            {
                var colors = File.ReadAllLines(file);
                colors = colors.Select(c => c.ToLower()).ToArray();
                colors = colors.Distinct().ToArray();
                using (StreamWriter sw = new StreamWriter(file, false))
                {
                    sw.Write(string.Join('\n', colors));
                }
            }

            _printer.PrintLog("Duplicated colors removed");
        }

        internal void CopyConfigWithIncrementVersion(VersionStack stack)
        {
            // get current's develop config file
            Config buildConfig = (Config)_jsonSerializationService.Deserialize(configFileName, typeof(Config));

            var stacks = buildConfig.About.Version.Split('.');
            string newStack = (Convert.ToInt32(stacks[(int)stack])
                + 1).ToString();


            List<string> newVersion = new List<string>();
            for (int i = 0; i <= (int)VersionStack.Patch; i++)
            {
                if (i == (int)stack)
                    newVersion.Add(newStack);
                else
                    newVersion.Add(stacks[i]);
            }

            buildConfig.About.Version = string.Join('.', newVersion);
            ContextConfig.Save(buildConfig, ConfigFileBuildPath);

            _printer.PrintLog("Version incremented");
        }

        private void SetReleaseDate()
        {
            var config = GetConfigFromBuildFolder();
            config.About.ReleaseDate = DateTime.Now;
            ContextConfig.Save(config, ConfigFileBuildPath);

            _printer.PrintLog("Release date setted");
        }

        private Config GetConfigFromBuildFolder()
        {
            return (Config)_jsonSerializationService.Deserialize(ConfigFileBuildPath, typeof(Config));
        }
    }
}
