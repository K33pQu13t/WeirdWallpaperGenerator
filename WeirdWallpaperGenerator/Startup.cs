using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Services;

namespace WeirdWallpaperGenerator
{
    public class Startup
    {
        public void Run()
        {
            Configure();
        }

        private void Configure()
        {
            // get its instance to preset it for whole application on the start
            SystemMessagePrinter.GetInstance(
                "[Error]",
                "[Warning]",
                "[Success]",
                "[Log]",
                Enums.ConsoleColorNullable.DarkRed,
                Enums.ConsoleColorNullable.DarkYellow,
                Enums.ConsoleColorNullable.Green,
                Enums.ConsoleColorNullable.Cyan);

            // gets config if its exist, sets it by default if not
            ContextConfig.GetInstance();
        }
    }
}
