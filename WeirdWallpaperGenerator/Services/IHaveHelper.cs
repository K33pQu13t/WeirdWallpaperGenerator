using WeirdWallpaperGenerator.Models.CommandLineParts;

namespace WeirdWallpaperGenerator.Services
{
    public interface IHaveHelper
    {
        /// <returns>a string of command list with description and usage</returns>
        string GetHelp(Command command = null);
    }
}
