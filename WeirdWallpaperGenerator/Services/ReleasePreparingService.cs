using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        string configFilePath => Path.Combine(buildFolder, configFileName);
        const string hashTableFileName = "hashtable";

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
            GenerateHashTable();
            IncrementVersion(versionUpdate);
            SetReleaseDate();
        }

        internal void GenerateHashTable()
        {
            HashTable hashTable = new HashTable
            {
                Version = ContextConfig.GetInstance().About.Version,
                Table = HashHelper.GetSHA1ChecksumFromFolder(buildFolder, new string[] { configFileName, hashTableFileName })
            };

            _binarySerializationService.Serialize(Path.Combine(buildFolder, hashTableFileName).ToString(), hashTable);
            _printer.PrintLog("Hash table created");
        }

        internal void RemoveGarbage()
        {
            foreach (var fileNameToDelete in garbage)
            {
                File.Delete(Path.Combine(buildFolder, fileNameToDelete));
            }
        }

        internal void IncrementVersion(VersionStack stack)
        {
            var config = GetConfigFromBuildFolder();

            var stacks = config.About.Version.Split('.');
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

            config.About.Version = string.Join('.', newVersion);
            ContextConfig.Save(config);
        }

        private void SetReleaseDate()
        {
            var config = GetConfigFromBuildFolder();
            config.About.ReleaseDate = DateTime.Now;
            ContextConfig.Save(config, configFilePath);
        }

        private Config GetConfigFromBuildFolder()
        {
            return (Config)_jsonSerializationService.Deserialize(configFilePath, typeof(Config));
        } 
    }
}
