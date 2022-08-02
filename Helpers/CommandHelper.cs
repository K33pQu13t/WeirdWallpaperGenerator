using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallpaperGenerator.Helpers
{
    public static class CommandHelper
    {
        /// <summary>
        /// splits the string to arguements by spaces
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns>collection of string splitted by spaces and included all characters inside quotes in one element 
        /// without spaces splitted by commas
        /// </returns>
        public static ICollection<string> SplitToArguments(this string commandLine)
        {
            List<string> output = new List<string>();
            string arg = string.Empty;
            bool insideQuotes = false;
            for (int i = 0; i < commandLine.Length; i++)
            {
                if (commandLine[i] == ' ' && !insideQuotes)
                {
                    output.Add(arg);
                    arg = string.Empty;
                    continue;
                }
                if (commandLine[i] == '\"' || commandLine[i] == '\'')
                {
                    insideQuotes = !insideQuotes;
                    continue;
                }
                arg += commandLine[i];
            }
            output.Add(arg);

            return output;
        }

        // TODO: try to make it below methods more generic

        /// <summary>
        /// gets the value next to flag string element
        /// </summary>
        /// <param name="commandLide"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetFlagValue(this List<string> commandLide, string flag)
        {
            if (!commandLide.Contains(flag))
                return string.Empty;

            string output;
            try
            {
                output = commandLide[commandLide.IndexOf(flag) + 1];
                if (output.StartsWith('-') || output.StartsWith('/'))
                    return string.Empty;
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.Empty;
            }

            return output;
        }

        /// <summary>
        /// gets the value next to flag string element
        /// </summary>
        /// <param name="commandLide"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetFlagValue(this List<string> commandLide, string[] flags)
        {
            if (!commandLide.Any(c => flags.Contains(c[1..])))
                return string.Empty;

            string output;
            try
            {
                int index = -1;
                foreach (string flag in flags)
                {
                    index = commandLide.IndexOf($"-{flag}");
                    if (index != -1)
                        break;
                }

                if (index == -1)
                    return string.Empty;
                output = commandLide[index + 1];
                if (output.StartsWith('-') || output.StartsWith('/'))
                    return string.Empty;
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.Empty;
            }

            return output;
        }

        /// <summary>
        /// determines if command list contains flag
        /// </summary>
        /// <param name="commandsList">collection of arguements</param>
        /// <param name="flags">flags without '-'</param>
        /// <returns>true if commands list contains any of flag</returns>
        public static bool ContainsFlag (this List<string> commandsList, string[] flags)
        {
            foreach(string element in commandsList)
            {
                if (flags.Contains(element.Replace("-", "")))
                    return true;
            }
            return false;
        }

        public static bool IsCommand(this List<string> commandsList, string[] commands)
        {
            if (!commandsList[0].Trim().StartsWith("/"))
                return false;
            string command = commandsList[0].Trim().Replace("/", "");
            return commands.Contains(command);
        }

        public static bool IsCommand(this List<string> commandsList, string command)
        {
            if (commandsList[0].Trim() != "/")
                return false;
            return commandsList[0].Trim() == command.Replace("/", "");
        }
    }
}
