using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Tavstal.TShop.Components;
using Tavstal.TShop.Utils.Managers;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandShopUI : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "shop";
        public string Help => "Opens the shop UI.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shui", "shopui" };
        public List<string> Permissions => new List<string> { "tshop.shop", "tshop.commands.shop", "tshop.commands.shopui" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();
            if (comp.IsUIOpened)
            {
                UIManager.Hide(callerPlayer);
                return;
            }

            UIManager.Show(callerPlayer);
            UIManager.UpdateProductPage(callerPlayer);
        }
    }
}
