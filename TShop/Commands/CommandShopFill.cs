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
    public class CommandShopFill : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "shopfill";
        public string Help => "Opens the shop UI.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "shfill" };
        public List<string> Permissions => new List<string> { "tshop.shopfill" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            int count = 0;
            foreach (var a in Assets.find(EAssetType.ITEM).OrderBy(x => x.id))
            {
                if (count == 100)
                    break;

                ItemAsset asset = a as ItemAsset;
                if (asset == null)
                    continue;

                if (a is SkinAsset)
                    continue;

                TShop.Database.AddProduct(asset.id, false, 1, 1, false, "");
                count++;
            }


            count = 0;
            foreach (var a in Assets.find(EAssetType.VEHICLE).OrderBy(x => x.id))
            {
                if (count == 100)
                    break;

                VehicleAsset asset = a as VehicleAsset;
                if (asset == null)
                    continue;

                if (a is SkinAsset)
                    continue;

                TShop.Database.AddProduct(asset.id, true, 1, 1, false, "");
                count++;
            }
        }
    }
}
