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
    public class CommandSellItem : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sell";
        public string Help => "Sells a specific amount of items.";
        public string Syntax => "[itemID] <amount>";
        public List<string> Aliases => new List<string> { "sellitem", "selli" };
        public List<string> Permissions => new List<string> { "tshop.sell.item" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 1 || args.Length == 2)
            {
                ushort id = 0;
                int amount = 1;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { }

                if (args.Length == 2)
                {
                    try
                    {
                        int.TryParse(args[1], out amount);
                    }
                    catch { }
                }

                ItemAsset asset = null;

                if (id > 0)
                    asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                else
                    asset = UAssetHelper.FindItemAsset(args[0]);

                if (asset == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_exists", args[0]));
                    return;
                }
                id = asset.id;

                ShopItem item = TShop.Database.FindItem(id);
                if (item == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_added", args[0]));
                    return;
                }

                decimal cost = item.GetSellCost(amount);
                List<InventorySearch> search = callerPlayer.Inventory.search(asset.id, true, true);
                if (search.Count < amount)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_enough"));
                    return;
                }

                if (cost == 0)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_sell_error"));
                    return;
                }

                TShop.economyProvider.Deposit(callerPlayer, cost);
                for (int i = 0; i < amount; i++)
                {
                    callerPlayer.Inventory.removeItem(search[i].page, callerPlayer.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                }
                TShop.economyProvider.AddTransaction(callerPlayer, new Transaction(ETransaction.SALE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), 0, callerPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_item_sell", asset.itemName, amount, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_sellitem_args"));
        }
    }
}
