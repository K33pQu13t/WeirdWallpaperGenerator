using System;
using System.Collections.Generic;
using System.Linq;
using WeirdWallpaperGenerator.Constants;
using WeirdWallpaperGenerator.Models.CommandLineParts;

namespace WeirdWallpaperGenerator.Helpers
{
    public class CommandService
    {
        /// <summary>
        /// Parce string to arguements
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns><see cref="Command"/> instance contained all related flags with values </returns>
        public Command SplitToArguments(string commandLine)
        {
            var commands = SplitSmart(commandLine).ToList();

            if (!commands.Any())
            {
                throw ExceptionHelper.GetException(
                 nameof(CommandService),
                 nameof(SplitToArguments),
                 "No command specified");
            }

            if (!LineContainsCommand(commandLine, out string commandValue))
            {
                if (!BasicCommandList.commandHelp.Contains(commands.First()))
                {
                    throw ExceptionHelper.GetException(
                   nameof(Helpers.CommandService),
                   nameof(SplitToArguments),
                   "All commands must be started from command (except help commands). " +
                   "No command specified");
                }
            }

            bool helpFlagFounded = false;
            Command command = new Command() { Value = commandValue };
            Flag flag = new Flag();
            int offset = string.IsNullOrEmpty(commandValue) ? 0 : 1;
            foreach (var part in commands.GetRange(offset, commands.Count - offset))
            {
                if (IsFlag(part, out string flagValue))
                {
                    if (helpFlagFounded)
                    {
                        flag.Arguments.Add(new Argument { Value = part });
                        break;
                    }

                    flag = new Flag();
                    flag.Value = flagValue;
                    AddFlag(command, flag);
                }
                else if (BasicCommandList.commandHelp.Contains(part))
                {
                    // if it's help for help flag
                    if (helpFlagFounded)
                    {
                        flag.Arguments.Add(new Argument { Value = part, Flag = flag });
                        break;
                    }

                    if (!string.IsNullOrEmpty(flag.Value))
                        AddFlag(command, flag);

                    flag = new Flag { Value = part };
                    // if it is help for flag
                    if (command.Flags.Count > 0)
                    {
                        flag.Arguments.AddRange(
                            command.Flags.Select(x => new Argument { Value = $"-{x.Value}", Flag = flag }));

                        AddFlag(command, flag);
                        break;
                    }
                    else
                    {
                        // that flag means to scan for next flag, put it in help's arguements then return
                        helpFlagFounded = true;
                        AddFlag(command, flag);
                    }
                }
                else
                {
                    if (helpFlagFounded)
                        continue;

                    if (string.IsNullOrEmpty(flag.Value))
                        throw ExceptionHelper.GetException(nameof(CommandService), nameof(SplitToArguments),
                            "Bad command sequence. Type help for help"); 
                    flag.Arguments.Add(new Argument() { Value = part, Flag = flag });

                    AddFlag(command, flag);
                }
            }

            // if its help command but with no argument then its help for command exactly
            if (command.IsHelpCommand && command.GetFlagValues(BasicCommandList.commandHelp).Count() == 0
                && !string.IsNullOrEmpty(command.Value))
            {
                var helpFlag = command.Flags.Find(flag => BasicCommandList.commandHelp.Contains(flag.Value));
                helpFlag.Arguments.Add(new Argument { Value = $"/{command.Value}", Flag = helpFlag });
            }

            return command;
        }

        /// <summary>
        /// Adds flag to comand. If flag with same value exist in command - it would be replaced
        /// </summary>
        /// <param name="command"></param>
        /// <param name="flag"></param>
        private void AddFlag(Command command, Flag flag)
        {
            var existedFlag = command.Flags.FirstOrDefault(x => x.Value == flag.Value);
            if (existedFlag == null)
                command.Flags.Add(flag);
            else
                existedFlag = flag;
        }

        private bool IsFlag(string str, out string flagValue)
        {
            flagValue = string.Empty;
            if (str.StartsWith('-') && !str.Contains(' '))
            {
                flagValue = str[1..];
                return true;
            }
            return false;
        }

        /// <param name="commandsLine">line to check</param>
        /// <param name="commandValue">out command's value</param>
        /// <param name="exactlyCommands">array of commands to check if any contains in line</param>
        /// <returns>true if line contains command. Also returns command's value</returns>
        private bool LineContainsCommand(string commandsLine, out string commandValue, string[] exactlyCommands = null)
        {
            commandValue = string.Empty;
            var commands = commandsLine.Split(' ');
            if (!commands[0].Trim().StartsWith("/"))
                return false;

            commandValue = commands[0].Trim().Replace("/", "");
            if (exactlyCommands == null)
                return true;

            return exactlyCommands.Contains(commandValue);
        }

        private IEnumerable<string> SplitSmart(string commandLine)
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
    }
}
