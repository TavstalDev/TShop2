using Rocket.API;
using System.Collections.Generic;
using System.Reflection;
using Tavstal.TLibrary.Helpers.Unturned;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandVersion : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => ("v" + Assembly.GetExecutingAssembly().GetName().Name);
        public string Help => "Gets the version of the plugin";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "tshop.version", "tshop.commands.version" };


        public void Execute(IRocketPlayer caller, string[] command)
        {
            var instance = TShop.Instance;
            var config = instance.Config.General;
            var icon = config.MessageIcon;
            string message = string.Join(System.Environment.NewLine, 
                $"&b&l[{instance.GetPluginName()}]&r System Info:",
                $"&b • Version: &r{TShop.Version}",
                $"&b • Build Date: &r{TShop.BuildDate}",
                "&b • Developer: &rTavstal");
            
            instance.SendPlainCommandReply(caller, message, icon);
        }
    }
}
