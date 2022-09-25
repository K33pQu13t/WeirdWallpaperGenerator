using System.Drawing;
using WeirdWallpaperGenerator.Enums.Drawers;

namespace WeirdWallpaperGenerator.DTO
{
    public class MathBilliardsConfigDto : DrawerConfigDto
    {
        public Color FillInsideColor { get; set; }
        public Color FillOutsideColor { get; set; }
        public int BrushSize { get; set; }
        public CornerPosition StartPosition { get; set; }
    }
}
