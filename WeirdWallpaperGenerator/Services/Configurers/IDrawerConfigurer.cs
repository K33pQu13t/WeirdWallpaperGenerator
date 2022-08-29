using WeirdWallpaperGenerator.Models.CommandLineParts;
using WeirdWallpaperGenerator.Services.Drawers;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    public interface IDrawerConfigurer<T> where T : IDrawer
    {
        /// <summary>
        /// configures instance of IDrawer according to specified commands
        /// </summary>
        /// <param name="command"><see cref="Command"/> instance contained flags for configuring</param>
        /// <returns></returns>
        T Configure(Command command);

        /// <summary>
        /// returns a help string about this method
        /// </summary>
        /// <returns></returns>
        string GetMethodHelp();
    }
}
