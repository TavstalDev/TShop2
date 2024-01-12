using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;

using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using System.Reflection;

namespace Tavstal.TShop
{
    public class CommandVersion : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => ("v" + Assembly.GetExecutingAssembly().GetName().Name);
        public string Help => "Gets the version of the plugin";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "tshop.version" };
        

        public void Execute(IRocketPlayer caller, string[] command)
        {
            TShop.Logger.Log("#########################################");
            TShop.Logger.Log(string.Format("# Build Version: {0}", TShop.Version));
            TShop.Logger.Log(string.Format("# Build Date: {0}", TShop.BuildDate));
            TShop.Logger.Log("#########################################");
        }
    }
}
