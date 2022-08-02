
using WallpaperGenerator.Services.BackgroundDrawers;
using System.Collections.Generic;

namespace WallpaperGenerator.Services.Configurers
{
    public interface IDrawerConfigurer<T> where T : IDrawer
    {
        /// <summary>
        /// configures instance of IDrawer according to specified commands
        /// </summary>
        /// <param name="commands">commands for configuring</param>
        /// <returns></returns>
        T Configure(List<string> commandsList);

        /// <returns>a string of command list with description and usage</returns>
        string GetHelp();
    }
}
