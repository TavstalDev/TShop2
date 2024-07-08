using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using Tavstal.TLibrary.Helpers.Unturned;

namespace Tavstal.TShop
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
            int count = 0;
            foreach (var asset in UAssetHelper.GetItemAssets().OrderBy(x => x.id))
            {
                if (count == 300)
                    break;
                if (asset == null)
                    continue;

                await TShop.Database.AddProductAsync(asset.id, false, 1, 1, false, "");
                count++;
            }


            count = 0;
            foreach (var asset in UAssetHelper.GetVehicleAssets().OrderBy(x => x.id))
            {
                if (count == 300)
                    break;

                await TShop.Database.AddProductAsync(asset.id, true, 1, 1, false, "");
                count++;
            }
        }
    }
}
