using System;

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
