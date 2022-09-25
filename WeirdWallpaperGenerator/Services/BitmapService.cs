using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WeirdWallpaperGenerator.Services
{
    public class BitmapService
    {
        /// <summary>
        /// colors one brush pattern. Start X and Y is left-up corner of pattern
        /// </summary>
        public void FillBitmapArea(Bitmap bitmap, int startX, int startY, int xLength, int yLenght, Color color)
        {
            for (int i = startX; i < startX + xLength; i++)
            {
                for (int j = startY; j < startY + yLenght; j++)
                {
                    bitmap.SetPixel(i, j, color);
                }
            }
        }
    }
}
