using System;
using System.Collections.Generic;
using System.Linq;
using WeirdWallpaperGenerator.Constants;

namespace WeirdWallpaperGenerator.Models.CommandLineParts
{
    public class Command : CommandLinePart
    {

        public List<Flag> Flags { get; set; } = new List<Flag>();

        public string GetFlagValue(IEnumerable<string> flagVariations)
        {
            return Flags.Find(x => flagVariations.Contains(x.Value))
                .Argument?.Value;
        }

        public IEnumerable<string> GetFlagValues(IEnumerable<string> flagVariations)
        {
            return Flags.Find(x => flagVariations.Contains(x.Value))
                ?.Arguments.Select(x => x.Value) ?? new List<string>();
        }

        public bool ContainsFlag(IEnumerable<string> flagVariations)
        {
            return Flags.Any(x => flagVariations.Any(variation => variation == x.Value));
        }

        public bool IsHelpCommand => Flags.Any(flag => BasicCommandList.commandHelp.Contains(flag.Value));

        public bool IsCommand()
        {
            return Value != null;
        }

        public bool IsCommand(IEnumerable<string> commandVariations)
        {
            return commandVariations.Contains(Value);
        }
    }
}
