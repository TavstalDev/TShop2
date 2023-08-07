using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
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
            TShopComponent cp = p.GetComponent<TShopComponent>();

            p.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, p.SteamPlayer().transportConnection, true, "Panel_Shop", true);
            UIManager.UpdateItemsPage(p);
            UIManager.UpdateVehiclessPage(p);
        }
    }
}
