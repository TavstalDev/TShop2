using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Components;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Commands
{
    public class CommandShopUI : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "shop";
        public override string Help => "Opens the shop UI.";
        public override string Syntax => "";
        public override List<string> Aliases => new List<string> { "shui", "shopui" };
        public override List<string> Permissions => new List<string> { "tshop.shop", "tshop.commands.shop", "tshop.commands.shopui" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            ShopComponent comp = p.GetComponent<ShopComponent>();
            if (comp.IsUIOpened)
            {
                UIManager.Hide(p);
            }
            else
            {
                UIManager.Show(p);
                await UIManager.UpdateProductPage(p);
            }
            return true;
        }
    }
}
