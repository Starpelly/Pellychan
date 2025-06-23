namespace Pellychan.GUI
{
    public static class Mathf
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
