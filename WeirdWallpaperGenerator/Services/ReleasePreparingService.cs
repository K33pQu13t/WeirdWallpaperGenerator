using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WeirdWallpaperGenerator.Config;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models;

namespace WeirdWallpaperGenerator.Services
{
    internal class ReleasePreparingService
    {
        const string buildFolder = @"..\..\..\..\Release build";
        const string configFileName = "config.json";
        const string hashTableFileName = "hashtable";

        readonly string[] garbage = new string[] { "WeirdWallpaperGenerator.pdb" };

        SystemMessagePrinter _printer;
        SerializationService _serializationService = new SerializationService();

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
        }

        internal void GenerateHashTable()
        {
            HashTable hashTable = new HashTable
            {
                Table = HashHelper.GetSHA1ChecksumFromFolder(buildFolder, new string[] { configFileName, hashTableFileName })
            };

            _serializationService.Serialize(Path.Combine(buildFolder, hashTableFileName).ToString(), hashTable);
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
            var about = config.GetSection("About").Get<About>();

            var stacks = about.Version.Split('.');
            string newStack = (Convert.ToInt32(stacks[(int)stack])
                +1).ToString();

            List<string> newVersion = new List<string>();
            for (int i = 0; i <= (int)VersionStack.Patch; i++)
            {
                if (i == (int)stack)
                    newVersion.Add(newStack);
                else
                    newVersion.Add(stacks[i]);
            }

            config.GetSection("About")["version"] = string.Join('.', newVersion);
        }

        private IConfiguration GetConfigFromBuildFolder()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.GetFullPath(Path.Combine(buildFolder, configFileName)), optional: false);
            return builder.Build();
        }
    }
}
