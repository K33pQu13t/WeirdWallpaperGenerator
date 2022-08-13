using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace WeirdWallpaperGenerator.Models
{
    [Serializable]
    class HashTable
    {
        public Dictionary<string, string> Table { get; set; } = new Dictionary<string, string>();
    }
}
