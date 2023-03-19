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
                int index = MathHelper.Next(count + 1);
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
                int index = MathHelper.Next(count + 1);
                T value = list[index];
                list[index] = list[count];
                list[count] = value;
            }
        }
    }
}
