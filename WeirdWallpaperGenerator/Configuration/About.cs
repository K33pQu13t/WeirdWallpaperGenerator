using Newtonsoft.Json;
using System;

namespace WeirdWallpaperGenerator.Configuration
{
    public class About
    {
        [JsonIgnore]
        public string ProjectName => "Weird Wallpaper Generator";
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; } = DateTime.Now;
        [JsonIgnore]
        public string Author => "K33p_Qu13t";
    }
}
