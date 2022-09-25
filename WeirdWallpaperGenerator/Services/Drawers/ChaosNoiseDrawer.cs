using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Helpers;

namespace WeirdWallpaperGenerator.Services.Drawers
{
    public class ChaosNoiseDrawer : IDrawer
    {
        private int _height;
        private int _width;
        private Color _coloredColor;
        private Color _backgroundColor;
        private int _brushSizeX;
        private int _brushSizeY;

        private Bitmap _bitmap;

        private readonly SecureRandomService _randomService;
        private readonly BitmapService _bitmapService;

        public ChaosNoiseDrawer(ChaosNoiseConfigDto config)
        {
            _randomService = new SecureRandomService();
            _bitmapService = new BitmapService();

            _width = config.Width.Value;
            _height = config.Height.Value;
            _coloredColor = config.ColoredColor;
            _backgroundColor = config.BackgroundColor;
            _brushSizeX = config.BrushSizeX;
            _brushSizeY = config.BrushSizeY;
        }

        public Bitmap Draw()
        {
            _bitmap = new Bitmap(_width, _height);
            var map = GetMap();

            int iteration = 0;
            for (int x = 0; x < _width; x += _brushSizeX)
            {
                for (int y = 0; y < _height; y += _brushSizeY)
                {
                    var value = map[iteration];
                    if (value == 1)
                    {
                        _bitmapService.FillBitmapArea(_bitmap, x, y, _brushSizeX, _brushSizeY, _coloredColor);
                    }
                    else
                    {
                        _bitmapService.FillBitmapArea(_bitmap, x, y, _brushSizeX, _brushSizeY, _backgroundColor);
                    }
                    iteration++;
                }
            }

            var result = new Bitmap(_bitmap);
            _bitmap.Dispose();
            return result;
        }

        private List<short> GetMap()
        {
            List<short> map = new List<short>();
            for(int i = 0; i < (_width/ _brushSizeX ) * (_height/ _brushSizeY); i++)
            {
                map.Add((short)_randomService.Next(0, 2));
            }
            return map;
        }

        public string GetArguments()
        {
            return $"-m chaosnoise " +
                  $"-h {_height} -w {_width}" +
                  $"-c \'{_coloredColor.ToHex()} {_backgroundColor.ToHex()}\'" +
                  $"-b {_brushSizeX} {_brushSizeY}";
        }
    }
}
