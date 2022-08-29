using System;
using System.Collections.Generic;
using System.Linq;

namespace WeirdWallpaperGenerator.Models.CommandLineParts
{
    public class Flag : CommandLinePart
    {
        public List<Argument> Arguments { get; set; } = new List<Argument>();
        /// <summary>
        /// If it has only one <see cref="CommandLineParts.Argument"/> then it is faster way to get it
        /// </summary>
        public Argument Argument => Arguments.SingleOrDefault();
        /// <summary>
        /// If it has only one <see cref="CommandLineParts.Argument"/> then it is faster way to get it's value
        /// </summary>
        public string SingleArgumentValue => Argument?.Value ?? null;

        /// <returns>true if this <see cref="Flag"/> contains only one 
        /// <see cref="CommandLineParts.Argument"/> and its value is 
        /// some of valuesVariations</returns>
        public bool IsValue(IEnumerable<string> valuesVariations)
        {
            return Argument != null && valuesVariations.Contains(Argument.Value);
        }

        /// <returns>true if some of <see cref="Arguments"/> have value from valuesVariations</returns>
        public bool ContainsValue(IEnumerable<string> valuesVariations)
        {
            return Arguments.Any(arg => valuesVariations.Contains(arg.Value));
        }
    }
}
