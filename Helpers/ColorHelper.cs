using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WeirdWallpaperGenerator.Helpers
{
    public static class ColorHelper
    { 
        public static string ToHex(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static Color ToColor(this string hexColor)
        {
            ColorConverter convertor = new ColorConverter();
            return (Color)convertor.ConvertFromString(hexColor);
        }
    }
}
