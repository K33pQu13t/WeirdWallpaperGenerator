using System;
using System.Collections.Generic;
using System.Text;
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

        readonly ConsoleColorNullable _errorColor;
        readonly ConsoleColorNullable _warningColor;
        readonly ConsoleColorNullable _successColor;


        private static SystemMessagePrinter instance;
        private SystemMessagePrinter(
            string errorTitle = "",
            string warningTitle = "",
            string successTitle = "",
            ConsoleColorNullable errorColor = ConsoleColorNullable.Default,
            ConsoleColorNullable warningColor = ConsoleColorNullable.Default,
            ConsoleColorNullable successColor = ConsoleColorNullable.Default
            ) 
        {
            _errorTitle = errorTitle;
            _warningTitle = warningTitle;
            _successTitle = successTitle;

            _errorColor = errorColor;
            _warningColor = warningColor;
            _successColor = successColor;
        }

        public static SystemMessagePrinter GetInstance(
            string errorTitle = "",
            string warningTitle = "",
            string successTitle = "",
            ConsoleColorNullable errorColor = ConsoleColorNullable.Default,
            ConsoleColorNullable warningColor = ConsoleColorNullable.Default,
            ConsoleColorNullable successColor = ConsoleColorNullable.Default
            )
        {
            if (instance == null)
                instance = new SystemMessagePrinter(
                    errorTitle,
                    warningTitle,
                    successTitle,
                    errorColor,
                    warningColor,
                    successColor);
            return instance;
        }

        internal void PrintError(string message)
        {
            semaphore.Wait();
            Console.ForegroundColor = _errorColor == ConsoleColorNullable.Default 
                ? Console.ForegroundColor 
                : (ConsoleColor)_errorColor;
            Console.WriteLine($"{(string.IsNullOrWhiteSpace(_errorTitle) ? "" : $"{_errorTitle}: ")}{message}");
            Console.ResetColor();
            semaphore.Release();
        }

        internal void PrintWarning(string message)
        {
            semaphore.Wait();
            Console.ForegroundColor = _warningColor == ConsoleColorNullable.Default
                ? Console.ForegroundColor
                : (ConsoleColor)_warningColor;
            Console.WriteLine($"{(string.IsNullOrWhiteSpace(_warningTitle) ? "" : $"{_warningTitle}: ")}{message}");
            Console.ResetColor();
            semaphore.Release();
        }

        internal void PrintSuccess(string message)
        {
            semaphore.Wait();
            Console.ForegroundColor = _successColor == ConsoleColorNullable.Default
                ? Console.ForegroundColor
                : (ConsoleColor)_successColor;
            Console.WriteLine($"{(string.IsNullOrWhiteSpace(_successTitle) ? "" : $"{_successTitle}: ")}{message}");
            Console.ResetColor();
            semaphore.Release();
        }
    }
}
