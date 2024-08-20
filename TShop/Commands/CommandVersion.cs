using Rocket.API;
using System.Collections.Generic;
using System.Reflection;
using Tavstal.TLibrary.Helpers.Unturned;

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
            TShop.Instance.SendPlainCommandReply(caller, "#########################################");
            TShop.Instance.SendPlainCommandReply(caller, $"# Build Version: {TShop.Version}");
            TShop.Instance.SendPlainCommandReply(caller, $"# Build Date: {TShop.BuildDate}");
            TShop.Instance.SendPlainCommandReply(caller, "#########################################");
        }
    }
}
