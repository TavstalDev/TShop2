using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
    public class ConsoleFormat
    {
        public string Key { get; set; }
        public ConsoleColor Color { get; set; }

        public ConsoleFormat() { }

        public ConsoleFormat(string key, ConsoleColor color)
        {
            Key = key;
            Color = color;
        }
    }
}
