using System.Drawing;

namespace WeirdWallpaperGenerator.Helpers
{
    public static class ColorHelper
    { 
        public static string ToHex(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLower();
        }

        public static Color ToColor(this string hexColor)
        {
            ColorConverter convertor = new ColorConverter();
            return (Color)convertor.ConvertFromString(hexColor);
        }

        public static bool IsColorString(this string str)
        {
            if (!str.StartsWith('#'))
                return false;

            try
            {
                str.ToColor();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
