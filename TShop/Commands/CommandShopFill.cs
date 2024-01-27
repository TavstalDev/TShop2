using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace Tavstal.TShop
{
    public class CommandShopFill : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "shopfill";
        public string Help => "Fills the shop with items. Used for debug";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shfill" };
        public List<string> Permissions => new List<string> { "tshop.shopfill" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            int count = 0;
            foreach (var a in Assets.find(EAssetType.ITEM).OrderBy(x => x.id))
            {
                if (count == 300)
                    break;

                ItemAsset asset = a as ItemAsset;
                if (asset == null)
                    continue;

                if (a is SkinAsset)
                    continue;

                await TShop.Database.AddProduct(asset.id, false, 1, 1, false, "");
                count++;
            }


            count = 0;
            foreach (var a in Assets.find(EAssetType.VEHICLE).OrderBy(x => x.id))
            {
                if (count == 300)
                    break;

                VehicleAsset asset = a as VehicleAsset;
                if (asset == null)
                    continue;

                if (a is SkinAsset)
                    continue;

                await TShop.Database.AddProduct(asset.id, true, 1, 1, false, "");
                count++;
            }
        }
    }
}
