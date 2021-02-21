using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkLogGenerator.Utilities
{
    public static class ExtendedString
    {
        private static readonly string[] c_newlineAsStringArray = new string[] { Environment.NewLine };
        public static string SplitFirst(this string what, char delimiter, out string remaining)
        {
            if (what == null)
            {
                remaining = null;
                return null;
            }

            int delimiterIndex = what.IndexOf(delimiter);
            if (delimiterIndex < 0)
            {
                remaining = null;
                return what;
            }

            var first = what.Substring(0, delimiterIndex);
            remaining = what.Substring(delimiterIndex + 1);
            return first;
        }

        public static string[] SplitLines(this string what, StringSplitOptions options = StringSplitOptions.None, StringComparison comparison = StringComparison.Ordinal)
        {
            // TODO: A quick-and-dirty implementation. Might consider writing
            //       our own in the future
            if (options == StringSplitOptions.RemoveEmptyEntries)
            {
                return what.Split(c_newlineAsStringArray, options);
            }

            // TODO: Need to make sure this follows the string.Split() conventions:
            if (what == null)
            {
                return new string[0];
            }

            if (what == string.Empty)
            {
                return new string[] { string.Empty };
            }

            if (!what.EndsWith(Environment.NewLine, comparison))
            {
                return what.Split(c_newlineAsStringArray, StringSplitOptions.None);
            }

            // The string ends in a newline. String.Split will return an "extra"
            // entry for the empty space between the newline and the end-of-string.
            // Remove that baggage:
            var ret = what.Split(c_newlineAsStringArray, StringSplitOptions.None);
            var length = ret.Length;
            if (length > 1)
            {
                ret = ret.SlowRemoveByIndex(length - 1);
            }
            return ret;
        }
    }
}
