using Newtonsoft.Json;
using System;

namespace WeirdWallpaperGenerator.Configuration
{
    public class UpdaterSettings
    {
        public bool AutoCheckUpdates { get; set; } = true;

        public bool AskBeforeUpdate { get; set; } = true;

        public int CheckPeriodDays { get; set; } = 14;

        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    }
}
