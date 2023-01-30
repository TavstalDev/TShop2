using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop.Helpers
{
    public static class FormatHelper
    {
        public readonly static List<TextFormat> DefaultFormats = new List<TextFormat>()
        {
            new TextFormat("&0", "<color=#000000>", "</color>", false),
            new TextFormat("&1", "<color=#0000AA>", "</color>", false),
            new TextFormat("&2", "<color=#00AA00>", "</color>", false),
            new TextFormat("&3", "<color=#00AAAA>", "</color>", false),
            new TextFormat("&4", "<color=#AA0000>", "</color>", false),
            new TextFormat("&5", "<color=#AA00AA>", "</color>", false),
            new TextFormat("&6", "<color=#FFAA00>", "</color>", false),
            new TextFormat("&7", "<color=#AAAAAA>", "</color>", false),
            new TextFormat("&8", "<color=#555555>", "</color>", false),
            new TextFormat("&9", "<color=#5555FF>", "</color>", false),
            new TextFormat("&a", "<color=#55FF55>", "</color>", false),
            new TextFormat("&b", "<color=#55FFFF>", "</color>", false),
            new TextFormat("&c", "<color=#FF5555>", "</color>", false),
            new TextFormat("&d", "<color=#FF55FF>", "</color>", false),
            new TextFormat("&e", "<color=#FFFF55>", "</color>", false),
            new TextFormat("&f", "<color=#FFFFFF>", "</color>", false),
            new TextFormat("&l", "<b>", "</b>", true),
            new TextFormat("&o", "<i>", "</i>", true),
        };

        public readonly static List<ConsoleFormat> ConsoleFormats = new List<ConsoleFormat>
        {
            new ConsoleFormat("&0", ConsoleColor.Black),
            new ConsoleFormat("&1", ConsoleColor.DarkBlue),
            new ConsoleFormat("&2", ConsoleColor.DarkGreen),
            new ConsoleFormat("&3", ConsoleColor.DarkCyan),
            new ConsoleFormat("&4", ConsoleColor.DarkRed),
            new ConsoleFormat("&5", ConsoleColor.DarkMagenta),
            new ConsoleFormat("&6", ConsoleColor.DarkYellow),
            new ConsoleFormat("&7", ConsoleColor.Gray),
            new ConsoleFormat("&8", ConsoleColor.DarkGray),
            new ConsoleFormat("&9", ConsoleColor.Blue),
            new ConsoleFormat("&a", ConsoleColor.Green),
            new ConsoleFormat("&b", ConsoleColor.Cyan),
            new ConsoleFormat("&c", ConsoleColor.Red),
            new ConsoleFormat("&d", ConsoleColor.Magenta),
            new ConsoleFormat("&e", ConsoleColor.Yellow),
            new ConsoleFormat("&f", ConsoleColor.White),
        };

        public static string FormatTextV2(string text)
        {
            string formated = string.Empty;
            List<TextFormat> activeFormats = new List<TextFormat>();

            char lastChar = ' ';
            bool isHex = false;
            string hexString = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char s = text[i];
                // Should be formatted
                if (lastChar == '&' && s != ' ')
                {
                    string key = new string(new char[] { lastChar, s });
                    // &r means reset, resets the format if it has active formats already
                    if (key == "&r")
                    {
                        if (activeFormats.Count > 0)
                        {
                            activeFormats.Reverse();
                            foreach (TextFormat f in activeFormats)
                                formated += f.EndTag;
                            activeFormats = new List<TextFormat>();
                        }
                    }
                    else if (s == '#') // Hex found, tries to format to hex color
                    {
                        isHex = true;
                        s = '&'; // sets current char to & because it will be later on set to lastChar
                    }
                    else if (isHex && hexString.Length != 6)
                    {
                        hexString += s;

                        if (hexString.Length == 6)
                        {
                            if (!int.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int result))
                            {
                                isHex = false;
                                hexString = string.Empty;
                            }
                        }
                        s = '&'; // sets current char to & because it will be later on set to lastChar
                    }
                    else
                    {
                        TextFormat newFormat = null;
                        if (isHex)
                            newFormat = new TextFormat(null, string.Format("<color=#{0}>", hexString), "</color>", false);
                        else
                            newFormat = DefaultFormats.Find(x => x.Key == key);

                        if (newFormat != null)
                        {
                            if (activeFormats.Count > 0)
                            {
                                activeFormats.Reverse();
                                foreach (TextFormat f in activeFormats)
                                    formated += f.EndTag;
                            }

                            TextFormat anotherFormat = activeFormats.Find(x => x.isDecoration != newFormat.isDecoration);
                            if (anotherFormat != null)
                            {
                                activeFormats = new List<TextFormat>()
                                {
                                    newFormat,
                                    anotherFormat
                                };
                                formated += newFormat.StartTag + anotherFormat.StartTag;
                            }
                            else
                            {
                                activeFormats = new List<TextFormat>()
                                {
                                    newFormat,
                                };
                                formated += newFormat.StartTag;
                            }

                            // If the currentFormat is hex then it ads the current char because normaly the current char should be the part of the key.
                            // For example: I would like to TEXT to be white, so I combine &#FFFFFF + TEXT. But If I remove this function this will happen:
                            // &#FFFFFFTEXT -> EXT
                            if (isHex)
                            {
                                isHex = false;
                                hexString = string.Empty;
                                formated += s;
                            }
                        }
                    }
                    lastChar = s;
                }
                else if (isHex) // Empty char found while trying to build a hex string
                {
                    isHex = false;
                    hexString = string.Empty;
                }
                else // Ads the character to the formatted string
                {
                    lastChar = s;
                    if (s != '&')
                        formated += s;
                }

                // If it gets to the end of the string, it will close the formatting
                if (i == text.Length - 1 && activeFormats.Count > 0)
                {
                    int endIndex = formated.Length - 1;
                    //formated += s;
                    foreach (TextFormat f in activeFormats)
                        formated += f.EndTag;
                }
            }

            return formated;
        }

        public static void SendFormatedConsole(string text)
        {
            string formated = string.Empty;
            ConsoleColor oldColor = Console.ForegroundColor;
            char lastChar = ' ';
            bool isHex = false;
            string hexString = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char s = text[i];
                // Should be formatted
                if (lastChar == '&' && s != ' ')
                {
                    string key = new string(new char[] { lastChar, s });
                    // &r means reset, resets the format if it has active formats already
                    if (key == "&r")
                        Console.ForegroundColor = ConsoleColor.White;
                    else if (s == '#') // Hex found, tries to format to hex color
                    {
                        isHex = true;
                        s = '&'; // sets current char to & because it will be later on set to lastChar
                    }
                    else if (isHex && hexString.Length != 6)
                    {
                        hexString += s;

                        if (hexString.Length == 6)
                        {
                            if (!int.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int result))
                            {
                                isHex = false;
                                hexString = string.Empty;
                            }
                        }
                        s = '&'; // sets current char to & because it will be later on set to lastChar
                    }
                    else
                    {
                        ConsoleFormat newFormat = null;
                        if (!isHex)
                            newFormat = ConsoleFormats.Find(x => x.Key == key);

                        if (newFormat != null)
                        {
                            Console.ForegroundColor = newFormat.Color; //formated += newFormat.StartTag;

                            // If the currentFormat is hex then it ads the current char because normaly the current char should be the part of the key.
                            // For example: I would like to TEXT to be white, so I combine &#FFFFFF + TEXT. But If I remove this function this will happen:
                            // &#FFFFFFTEXT -> EXT
                            if (isHex)
                            {
                                isHex = false;
                                hexString = string.Empty;
                                //formated += s;
                                Console.Write(s);
                            }
                        }
                    }
                    lastChar = s;
                }
                else if (isHex) // Empty char found while trying to build a hex string
                {
                    isHex = false;
                    hexString = string.Empty;
                }
                else // Ads the character to the formatted string
                {
                    lastChar = s;
                    if (s != '&')
                        Console.Write(s); //formated += s;
                }

                // If it gets to the end of the string, it will close the formatting
                if (i == text.Length - 1)
                {
                    Console.ForegroundColor = oldColor;
                    Console.CursorLeft = 0;
                    Console.CursorTop += 1;
                }
            }
        }

        // It's for removing formaters from the text. Mostly used for the console.
        public static string ClearFormaters(string text)
        {
            string formated = string.Empty;
            char lastChar = ' ';
            for (int i = 0; i < text.Length; i++)
            {
                char s = text[i];
                if (lastChar == '&' && s != ' ')
                {
                    lastChar = s;
                }
                else // Ads the character to the formatted string
                {
                    lastChar = s;
                    if (s != '&')
                        formated += s;
                }
            }

            return formated;
        }
    }
}
