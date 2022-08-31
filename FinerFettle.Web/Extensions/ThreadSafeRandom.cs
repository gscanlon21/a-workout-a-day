namespace FinerFettle.Web.Extensions
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] 
        private static Random? Local;

        public static Random ThisThreadsRandom
        {
            get => Local ??= new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));
        }
    }
}
