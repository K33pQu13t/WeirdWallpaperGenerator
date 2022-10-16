using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WeirdWallpaperGenerator.Configuration;
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
            path = Path.GetFullPath(path);
            string[] hexColors = File.ReadAllLines(path).Select(c => c.ToLower()).ToArray();

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

        public Color GetRandomColorFromSets(List<ColorSet> sets, out ColorSet choosenSet)
        {
            Console.WriteLine($"app directory: {ContextConfig.AppDirectory}");
            if (sets == null || sets.Count == 0)
            {
                throw ExceptionHelper.GetException(nameof(ColorService), nameof(GetRandomColorFromSets), 
                    $"Attempt to get color from empty colors' list");
            }

            choosenSet = sets[_rnd.Next(0, sets.Count)];

            var colorsFromSet = GetColorsFromFile(choosenSet.Path);

            return GetRandomColor(colorsFromSet);
        }
    }
}
