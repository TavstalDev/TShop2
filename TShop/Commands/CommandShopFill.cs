using Rocket.API;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;

namespace Tavstal.TShop.Commands
{
    public class CommandShopFill : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "shopfill";
        public override string Help => "Fills the shop with items. Used for debug";
        public override string Syntax => "";
        public override List<string> Aliases => new List<string> { "shfill" };
        public override List<string> Permissions => new List<string> { "tshop.shopfill", "tshop.commands.shopfill" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            if (!TShop.Instance.Config.DebugMode)
            {
                TShop.Instance.SendPlainCommandReply(caller,"&cYou must enable debugMode to use 'shopfill'.");
                return true;
            }

            int count = 0;
            foreach (var asset in UAssetHelper.GetItemAssets().OrderBy(x => x.id))
            {
                if (count == 100)
                    break;

                if (asset.id == 0)
                    continue;

                await TShop.DatabaseManager.AddProductAsync(asset.id, false, null, 1, 1, false, "");
                count++;
            }


            count = 0;
            foreach (var asset in UAssetHelper.GetVehicleAssets().OrderBy(x => x.id))
            {
                if (count == 100)
                    break;

                if (asset.id == 0)
                    continue;

                await TShop.DatabaseManager.AddProductAsync(asset.id, true,  null,1, 1, false, "");
                count++;
            }
            return true;
        }
    }
}
