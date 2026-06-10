using System;

namespace btr.portal.worker.Progress
{
    internal static class ConsoleColorSupport
    {
        public static bool IsEnabled =>
            !Console.IsOutputRedirected;

        public static void WriteLine(ConsoleColor color, string message)
        {
            if (!IsEnabled)
            {
                Console.Out.WriteLine(message);
                return;
            }

            var previous = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.Out.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previous;
            }
        }

        public static void Write(ConsoleColor color, string message)
        {
            if (!IsEnabled)
            {
                Console.Out.Write(message);
                return;
            }

            var previous = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.Out.Write(message);
            }
            finally
            {
                Console.ForegroundColor = previous;
            }
        }
    }
}
