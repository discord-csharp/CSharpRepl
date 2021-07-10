using System;
using System.Collections.Generic;
using System.Text;

namespace CSDiscordService
{
    public static class StringExtensions
    {
        public static string TruncateTo(this string str, int length)
        {
            if (str.Length < length)
            {
                return str;
            }

            return str.Substring(0, length);
        }
    }
}
