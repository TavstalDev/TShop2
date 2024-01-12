using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;

using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;

namespace Tavstal.TShop
{
    public class CommandShopUI : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "Shop";
        public string Help => "Opens the shop UI.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shui" };
        public List<string> Permissions => new List<string> { "tshop.shop" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            HUDManager.Show(p);
            HUDManager.UpdateProductPage(p);
        }
    }
}
