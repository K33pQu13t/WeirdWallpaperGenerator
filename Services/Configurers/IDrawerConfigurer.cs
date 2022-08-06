
using WeirdWallpaperGenerator.Services.BackgroundDrawers;
using System.Collections.Generic;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    public interface IDrawerConfigurer<T> where T : IDrawer
    {
        /// <summary>
        /// configures instance of IDrawer according to specified commands
        /// </summary>
        /// <param name="commands">commands for configuring</param>
        /// <returns></returns>
        T Configure(List<string> commandsList);

        /// <summary>
        /// returns a help string about this method
        /// </summary>
        /// <returns></returns>
        string GetMethodHelp();
    }
}
