using System;
using System.Drawing;
using System.IO;
using System.Linq;
using WeirdWallpaperGenerator.Helpers;

namespace WeirdWallpaperGenerator.Services
{
    public class ColorService
    {
        private Random _rnd;

        public ColorService()
        {
            _rnd = new Random();
        }

        public Color[] GetColorsFromFile(string path)
        {
            string[] hexColors = File.ReadAllLines(path);

            // make sure only unique colors represented
            if (hexColors.Distinct().Count() != hexColors.Length)
            {
                hexColors = hexColors.Distinct().ToArray();
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.Write(string.Join("\n", hexColors));
                }
            }

            Color[] colors = new Color[hexColors.Length];
            for (int i = 0; i < hexColors.Length; i++)
            {
                colors[i] = hexColors[i].ToColor();
            }

            return colors;
        }

        public Color GetRandomColor(Color[] hexColors)
        {
            return hexColors[_rnd.Next(0, hexColors.Length)];
        }
    }
}
