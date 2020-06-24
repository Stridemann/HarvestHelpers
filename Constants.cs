using SharpDX;

namespace HarvestHelpers
{
    public static class Constants
    {
        public const float GRID_STEP = 11.2f;
        private const float GRID_BORDER = GRID_STEP * 1.5f;
        public const float IMAGE_CUTOFF_LEFT = 166 - GRID_BORDER; //(16,8) = 149,2
        public const float IMAGE_CUTOFF_BOT = 166 - GRID_BORDER;
        public const float GRID_WIDTH = 448 + GRID_BORDER * 2;

        public static Color Blue = Color.SkyBlue;
        public static Color Purple = Color.Purple;
        public static Color Yellow = Color.Yellow;
        public static Color Neutral = Color.Red;
        public static Color OutOfRange = Color.Red;
        public static Color Highlighted = Color.Azure;
    }
}