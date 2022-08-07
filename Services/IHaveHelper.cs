using System.Collections.Generic;

namespace WeirdWallpaperGenerator.Services
{
    public interface IHaveHelper
    {
        /// <returns>a string of command list with description and usage</returns>
        string GetHelp(List<string> commandList = null, string helpFor = "");
    }
}
