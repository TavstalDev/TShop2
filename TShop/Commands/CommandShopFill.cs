using Rocket.API;
using System.Collections.Generic;
using System.Linq;
using Tavstal.TLibrary.Helpers.Unturned;

namespace Tavstal.TShop.Commands
{
    public class CommandShopFill : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "shopfill";
        public string Help => "Fills the shop with items. Used for debug";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shfill" };
        public List<string> Permissions => new List<string> { "tshop.shopfill", "tshop.commands.shopfill" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            if (!TShop.Instance.Config.DebugMode)
            {
                TShop.Instance.SendPlainCommandReply(caller,"&cYou must enable debugMode to use 'shopfill'.");
                return;
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
        }
    }
}
