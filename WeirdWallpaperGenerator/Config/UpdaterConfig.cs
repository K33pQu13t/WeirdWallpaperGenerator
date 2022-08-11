using System;
using System.Collections.Generic;
using System.Text;

namespace WeirdWallpaperGenerator.Config
{
    public class UpdaterConfig
    {
        public bool AutoCheckUpdates { get; set; }

        public bool AskBeforeUpdate { get; set; }

        public int CheckPeriodDays { get; set; }

        public DateTime LastDateUpdate { get; set; }
    }
}
