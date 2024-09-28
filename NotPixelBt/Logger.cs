namespace NotPixelBt
{
    public static class Logger
    {
        public static void LogInfo(string info)
        {
            Console.WriteLine($"[{DateTime.Now} INFO]-{info}");
        }

        public static void LogWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now} WARN]-{warning}");
            Console.ResetColor();
        }
        public static void LogError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now} ERROR]-{error}");
            Console.ResetColor();
        }
    }
}
