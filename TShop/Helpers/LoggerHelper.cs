using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop.Helpers
{
    internal static class LoggerHelper
    {
        private static string Name = Assembly.GetExecutingAssembly().GetName().Name;
        private static bool IsDebug = false;

        public static void LogRich(object message, string prefix = "&a[INFO] >&f")
        {
            string text = string.Format("&b[{0}] {1} {2}", Name, prefix, message.ToString());
            try
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                using (StreamWriter streamWriter = File.AppendText(Path.Combine(Rocket.Core.Environment.LogsDirectory, Rocket.Core.Environment.LogFile)))
                {
                    streamWriter.WriteLine(string.Concat("[", DateTime.Now, "] ", FormatHelper.ClearFormaters(text)));
                    streamWriter.Close();
                }
                FormatHelper.SendFormatedConsole(text);
                Console.ForegroundColor = oldColor;
            }
            catch
            {
                Rocket.Core.Logging.Logger.Log(text);
            }
        }

        public static void LogRichWarning(object message, string prefix = "&e[WARNING] >&f")
        {
            LogRich(message, prefix);
        }

        public static void LogRichException(object message, string prefix = "&6[EXCEPTION] >&f")
        {
            LogRich(message, prefix);
        }

        public static void LogRichError(object message, string prefix = "&c[ERROR] >&f")
        {
            LogRich(message, prefix);
        }

        public static void LogRichCommand(object message, string prefix = "&9[COMMAND] >&f")
        {
            LogRich(message, prefix);
        }

        public static void LogRichDebug(object message, string prefix = "&d[DEBUG] >&f")
        {
            if (IsDebug)
                LogRich(message, prefix);
        }

        public static void Log(object message, ConsoleColor color = ConsoleColor.Green, string prefix = "[INFO] >")
        {

            string text = string.Format("[{0}] {1} {2}", Name, prefix, message.ToString());
            try
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                using (StreamWriter streamWriter = File.AppendText(Path.Combine(Rocket.Core.Environment.LogsDirectory, Rocket.Core.Environment.LogFile)))
                {
                    streamWriter.WriteLine(string.Concat("[", DateTime.Now, "] ", text));
                    streamWriter.Close();
                }
                Console.WriteLine(text);
                Console.ForegroundColor = oldColor;
            }
            catch
            {
                Rocket.Core.Logging.Logger.Log(text.Replace($"[{Name}] ", ""), color);
            }
        }

        public static void LogWarning(object message, ConsoleColor color = ConsoleColor.Yellow, string prefix = "[WARNING] >")
        {
            Log(message, color, prefix);
        }

        public static void LogException(object message, ConsoleColor color = ConsoleColor.DarkYellow, string prefix = "[EXCEPTION] >")
        {
            Log(message, color, prefix);
        }

        public static void LogError(object message, ConsoleColor color = ConsoleColor.Red, string prefix = "[ERROR] >")
        {
            Log(message, color, prefix);
        }

        public static void LogDebug(object message, ConsoleColor color = ConsoleColor.Magenta, string prefix = "[DEBUG] >")
        {
            if (IsDebug)
                Log(message, color, prefix);
        }

        public static void LogLateInit()
        {
            ConsoleColor oldFGColor = Console.ForegroundColor;
            ConsoleColor oldBGColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            string text = $"######## {Name} LATE INIT ########";
            try
            {
                using (StreamWriter streamWriter = File.AppendText(Path.Combine(Rocket.Core.Environment.LogsDirectory, Rocket.Core.Environment.LogFile)))
                {
                    streamWriter.WriteLine(string.Concat("[", DateTime.Now, "] ", string.Format("[{0}] {1}", Name, text)));
                    streamWriter.Close();
                }
                Console.WriteLine(text);
            }
            catch
            {
                Rocket.Core.Logging.Logger.Log(text);
            }
            Console.ForegroundColor = oldFGColor;
            Console.BackgroundColor = oldBGColor;
        }

        public static void LogCommand(object message, ConsoleColor color = ConsoleColor.Blue, string prefix = "[Command] >")
        {
            string msg = message.ToString().Replace("((", "{").Replace("))", "}").Replace("[TShop]", "");
            int amount = msg.Split('{').Length;
            for (int i = 0; i < amount; i++)
            {
                Regex regex = new Regex(string.Format("{0}(.*?){1}", Regex.Escape("{"), Regex.Escape("}")), RegexOptions.RightToLeft);
                msg = regex.Replace(msg, "{" + "}");
            }

            Log(msg.Replace("{", "").Replace("}", ""), color, prefix);
        }
    }
}
