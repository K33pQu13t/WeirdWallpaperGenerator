using FractalGenerator.BackgroundDrawers;
using System;
using System.Drawing;

namespace FractalGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            PrimeFractalDrawer drawer = new PrimeFractalDrawer(1080, 1920,
                fillOutsideColor: "#acc7b4", fillInsideColor: "#331b3f", brushSize: 16);

            Bitmap bitmap = drawer.Draw();
            bitmap.Save("output.png");
        }
    }
}
