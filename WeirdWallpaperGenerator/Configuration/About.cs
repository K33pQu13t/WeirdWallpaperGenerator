using Newtonsoft.Json;
using System;

namespace WeirdWallpaperGenerator.Configuration
{
    public class About
    {
        [JsonIgnore]
        public string ProjectName => "Weird Wallpaper Generator";
        public string Version { get; set; } = "1.0.3";
        public DateTime ReleaseDate { get; set; } = new DateTime(day: 31, month: 8, year: 2022);
        [JsonIgnore]
        public string Author => "K33p_Qu13t";

        // TODO: Update it before release:
        public string Version { get; set; } = "1.0.2";
        public DateTime ReleaseDate { get; set; } = new DateTime(day: 31, month: 8, year: 2022);
    }
}
