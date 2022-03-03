using System;
using System.Linq;

namespace Auto
{
    public class Names
    {
        public string Slugify(string input)
        {
            return new String(input
                .Replace(" ", "_")
                .ToCharArray()
                .Where(p => Char.IsLetterOrDigit(p))
                .ToArray());
        }
    }
}