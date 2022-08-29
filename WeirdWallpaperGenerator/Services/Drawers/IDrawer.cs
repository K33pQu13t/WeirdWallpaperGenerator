using System.Drawing;

namespace WeirdWallpaperGenerator.Services.Drawers
{
    public interface IDrawer
    {
        /// <summary>
        /// draws an image, returns bitmap of got image
        /// All needed feelds must be specified in a constructor of an instance
        /// </summary>
        /// <returns>Bitmap image of fractal</returns>
        public Bitmap Draw();

        /// <returns>a string with arguments which can be used in constructor of instance
        /// to redraw same bitmap as got</returns>
        public string GetArguments();
    }
}
