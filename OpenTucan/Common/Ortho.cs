using OpenTK;

namespace OpenTucan.Common
{
    public static class Ortho
    {
        public static Vector2 ScreenToRect(int x, int y, int width, int height) 
        {
            Vector2 processedCoordinates;
            processedCoordinates.X = 2.0f * x / width - 1;
            processedCoordinates.Y = -(2.0f * y / height - 1);
            return processedCoordinates;
        }
        
        public static Vector2 RectToScreen(float x, float y, int width, int height)
        {
            Vector2 processedCoordinates;
            processedCoordinates.X = (x + 1) * width / 2;
            processedCoordinates.Y = (y + 1) * height / 2;
            return processedCoordinates;
        }
    }
}