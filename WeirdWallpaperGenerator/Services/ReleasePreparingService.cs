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
        const string configFile = "config.json";

        SystemMessagePrinter _printer;
        SerializationService _serializationService = new SerializationService();

        public ReleasePreparingService()
        {
            _printer = SystemMessagePrinter.GetInstance();
        }

        internal void Prepare()
        {
            GenerateHashTable();
            //SetBuildHash();
        }

        internal void GenerateHashTable()
        {
            HashTable hashTable = new HashTable();
            hashTable.Table = HashHelper.GetSHA1ChecksumFromFolder(buildFolder, new string[] { configFile });

            _serializationService.Serialize(Path.Combine(buildFolder, "hashtable").ToString(), hashTable);
            _printer.PrintLog("Hash table created");
        }

        //internal void SetBuildHash()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("config.json", optional: false);
        //    IConfiguration config = builder.Build();

        //    string buildHash = HashHelper.GetSHA1ChecksumFromFolder(buildFolder, new string[] { configFile });

        //    config.GetSection("About").Get<About>().Hash = buildHash;
        //    // about section
        //    var about = config.GetSection("About").Get<About>();
        //}
    }
}
