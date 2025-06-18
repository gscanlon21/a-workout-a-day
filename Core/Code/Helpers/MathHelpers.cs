namespace Core.Code.Helpers
{
    public static class MathHelpers
    {
        public static float? Mod(float? a, float b)
        {
            return a - b * (int)Math.Floor(a.GetValueOrDefault() / b);
        }
    }
}
