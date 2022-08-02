using System;
using System.Collections.Generic;
using System.Text;

namespace WallpaperGenerator.Config
{
    public class ContextConfig
    {
        private static ContextConfig instance;
        private ContextConfig() {}

        public static ContextConfig GetInstance()
        {
            if (instance == null)
                instance = new ContextConfig();
            return instance;
        }

        public ColorsSets ColorsSets { get; set; }
    }
}
