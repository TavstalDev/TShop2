using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Tavstal.TShop.Managers;
using System.Reflection;

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

    public static class ArrayExensions
    {
        public static bool isValidIndex<T>(this List<T> list, int index)
        {
            return list.Count - 1 >= index && index >= 0;
        }

        public static bool isValidIndex<T>(this T[] list, int index)
        {
            return list.Length - 1 >= index && index >= 0;
        }

        public static void Shuffle<T>(this T[] list)
        {
            int count = list.Length;
            while (count > 0)
            {
                count--;
                int index = UnturnedHelper.GenerateRandomNumber(count + 1);
                T value = list[index];
                list[index] = list[count];
                list[count] = value;
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            int count = list.Count;
            while (count > 0)
            {
                count--;
                int index = UnturnedHelper.GenerateRandomNumber(count + 1);
                T value = list[index];
                list[index] = list[count];
                list[count] = value;
            }
        }
    }

    public static class IntegerExtensions
    {
        public static int Clamp(this int value, int maxValue)
        {
            return maxValue < value ? maxValue : value;
        }

        public static int Clamp(this int value, int minValue, int maxValue)
        {
            return minValue > value ? minValue : maxValue < value ? maxValue : value;
        }
    }

    public static class DecimalExtensions
    {
        public static decimal Clamp(this decimal value, decimal maxValue)
        {
            return maxValue < value ? maxValue : value;
        }

        public static decimal Clamp(this decimal value, decimal minValue, decimal maxValue)
        {
            return minValue > value ? minValue : maxValue < value ? maxValue : value;
        }
    }

    public static class EnumerableExtensions
    {
        public static ECurrency ToCurrency(this EPaymentMethod type)
        {
            switch (type)
            {
                case EPaymentMethod.wallet:
                    {
                        return ECurrency.cash;
                    }
                case EPaymentMethod.bank:
                    {
                        return ECurrency.bank;
                    }
                case EPaymentMethod.crypto:
                    {
                        return ECurrency.crypto;
                    }
                default:
                    {
                        return ECurrency.cash;
                    }
            }
        }
        public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            foreach (var obj in src)
            {
                action(obj);
            }
        }

        public static bool None<T>(this IEnumerable<T> src, Func<T, bool> predicate)
        {
            return !src.Any(predicate);
        }

        public static bool DeepEquals<T>(this List<T> src, List<T> dest)
        {
            if (src.Count != dest.Count)
            {
                return false;
            }
            return !src.Where((t, i) => !t.Equals(dest[i])).Any();
        }

        public static class ReflectionExtensions
        {
            public static MethodInfo GetMethod(Type type, string name, BindingFlags flags, Type[] args)
            {
                return type.GetMethod(name, flags, null, CallingConventions.Any, args ?? new Type[0], null);
            }
        }
    }
}
