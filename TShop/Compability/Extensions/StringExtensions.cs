using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Tavstal.TShop.Managers;
using System.Reflection;
using Tavstal.TShop.Helpers;

namespace Tavstal.TShop.Compability
{
    public static class StringExtensions
    {

        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool ContainsIgnoreCase(this string str, string part)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, part, CompareOptions.IgnoreCase) >= 0;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string Capitalize(this string str)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLowerInvariant());
        }

    }
}
