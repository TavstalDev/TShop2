using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Tavstal.TShop.Managers;

namespace Tavstal.TShop
{
    public class CommandShopUI : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "Shop";
        public string Help => "Opens the shop UI.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shui", "shopui" };
        public List<string> Permissions => new List<string> { "tshop.shop" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            TShopComponent comp = p.GetComponent<TShopComponent>();
            if (comp.IsUIOpened)
            {
                UIManager.Hide(p);
            }
            else
            {
                UIManager.Show(p);
                UIManager.UpdateProductPage(p);
            }
        }
    }
}
