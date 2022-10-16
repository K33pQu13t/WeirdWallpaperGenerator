using Newtonsoft.Json;
using System;

namespace WeirdWallpaperGenerator.Configuration
{
    public class About
    {
        [JsonIgnore]
        public string ProjectName => "Weird Wallpaper Generator";
        [JsonIgnore]
        public string Author => "K33p_Qu13t";

        // TODO: Update it before release:
        public string Version { get; set; } = "1.1.0";
        public DateTime ReleaseDate { get; set; } = new DateTime(day: 16, month: 10, year: 2022);
    }
}
