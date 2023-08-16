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
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop
{
    public class CommandCostItem : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "cost";
        public string Help => "Checks the cost of a specific item.";
        public string Syntax => "[itemID]";
        public List<string> Aliases => new List<string> { "costitem", "costi" };
        public List<string> Permissions => new List<string> { "tshop.cost.item" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 1)
            {
                ushort id = 0;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { }

                ItemAsset asset = null;

                if (id > 0)
                    asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                else
                    asset = UAssetHelper.FindItemAsset(args[0]);

                if (asset == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_item_not_found", args[0]);
                    return;
                }
                id = asset.id;

                ShopItem item = TShop.Database.FindItem(id);
                if (item == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_item_not_added", args[0]);
                    return;
                }

                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "success_item_cost", asset.itemName, item.GetBuyCost(), item.GetSellCost(), TShop.economyProvider.GetConfigValue<string>("MoneySymbol"));
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_command_costitem_args");
        }
    }
}
