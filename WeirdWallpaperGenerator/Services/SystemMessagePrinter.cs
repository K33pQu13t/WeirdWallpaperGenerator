using System;
using System.Threading;
using WeirdWallpaperGenerator.Enums;

namespace WeirdWallpaperGenerator.Services
{
    internal class SystemMessagePrinter
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        readonly string _errorTitle;
        readonly string _warningTitle;
        readonly string _successTitle;
        readonly string _logTitle;

        readonly ConsoleColorNullable _errorColor;
        readonly ConsoleColorNullable _warningColor;
        readonly ConsoleColorNullable _successColor;
        readonly ConsoleColorNullable _logColor;

        private static SystemMessagePrinter instance;
        private SystemMessagePrinter(
            string errorTitle = "",
            string warningTitle = "",
            string successTitle = "",
            string logTitle = "",
            ConsoleColorNullable errorColor = ConsoleColorNullable.Default,
            ConsoleColorNullable warningColor = ConsoleColorNullable.Default,
            ConsoleColorNullable successColor = ConsoleColorNullable.Default,
            ConsoleColorNullable logColor = ConsoleColorNullable.Default
            ) 
        {
            _errorTitle = errorTitle;
            _warningTitle = warningTitle;
            _successTitle = successTitle;
            _logTitle = logTitle;

            _errorColor = errorColor;
            _warningColor = warningColor;
            _successColor = successColor;
            _logColor = logColor;
        }

        public static SystemMessagePrinter GetInstance(
            string errorTitle = "",
            string warningTitle = "",
            string successTitle = "",
            string logTitle = "",
            ConsoleColorNullable errorColor = ConsoleColorNullable.Default,
            ConsoleColorNullable warningColor = ConsoleColorNullable.Default,
            ConsoleColorNullable successColor = ConsoleColorNullable.Default,
            ConsoleColorNullable logColor = ConsoleColorNullable.Default
            )
        {
            if (instance == null)
                instance = new SystemMessagePrinter(
                    errorTitle,
                    warningTitle,
                    successTitle,
                    logTitle,
                    errorColor,
                    warningColor,
                    successColor,
                    logColor);
            return instance;
        }

        private void Print(string message, ConsoleColor color)
        {
            semaphore.Wait();
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
            semaphore.Release();
        }

        internal void PrintError(string message, bool putPrefix = true)
        {
            message = $"{(!string.IsNullOrWhiteSpace(_errorTitle) && putPrefix ? $"{_errorTitle}: " : "")}{message}";
            ConsoleColor color = _errorColor == ConsoleColorNullable.Default 
                ? Console.ForegroundColor 
                : (ConsoleColor)_errorColor;

            Print(message, color);
        }

        internal void PrintWarning(string message, bool putPrefix = true)
        {
            message = $"{(!string.IsNullOrWhiteSpace(_warningTitle) && putPrefix ? $"{_warningTitle}: " : "")}{message}";
            ConsoleColor color = _warningColor == ConsoleColorNullable.Default
                ? Console.ForegroundColor
                : (ConsoleColor)_warningColor;

            Print(message, color);
        }

        internal void PrintSuccess(string message, bool putPrefix = true)
        {
            message = $"{(!string.IsNullOrWhiteSpace(_successTitle) && putPrefix ? $"{_successTitle}: " : "")}{message}";
            ConsoleColor color = _successColor == ConsoleColorNullable.Default
                ? Console.ForegroundColor
                : (ConsoleColor)_successColor;

            Print(message, color);
        }

        internal void PrintLog(string message, bool putPrefix = true)
        {
            message = $"{(!string.IsNullOrWhiteSpace(_logTitle) && putPrefix ? $"{_logTitle}: " : "")}{message}";
            ConsoleColor color = _logColor == ConsoleColorNullable.Default
                ? Console.ForegroundColor
                : (ConsoleColor)_logColor;

            Print(message, color);
        }
    }
}
