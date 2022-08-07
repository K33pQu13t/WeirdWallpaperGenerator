using System;
using System.Collections.Generic;
using System.Text;

namespace WeirdWallpaperGenerator.Helpers
{
    public static class ExceptionHelper
    {
        public static Exception GetException(string className, string methodName, string message)
        {
            return new Exception(
                $"{(!string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(methodName) ? $"{className} -> {methodName}: " : "")}" +
                $"{message}");
        }
    }
}
