using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandSellItem : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sell";
        public string Help => "Sells a specific amount of items.";
        public string Syntax => "[itemID] <amount>";
        public List<string> Aliases => new List<string> { "sellitem", "selli" };
        public List<string> Permissions => new List<string> { "tshop.sell.item", "tshop.commands.sell.item" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length == 1 || args.Length == 2)
            {
                ushort id = 0;
                int amount = 1;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { /* ignore */ }

                if (args.Length == 2)
                {
                    try
                    {
                        int.TryParse(args[1], out amount);
                    }
                    catch { /* ignore */ }
                }

                ItemAsset asset;
                if (id > 0)
                    asset = UAssetHelper.FindItemAsset(id);
                else
                    asset = UAssetHelper.FindItemAsset(args[0]);

                if (asset == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_item_not_found", args[0]);
                    return;
                }
                id = asset.id;

                Product item = await TShop.DatabaseManager.FindItemAsync(id);
                if (item == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_item_not_added", args[0]);
                    return;
                }
                
                if (item.HasPermission && !callerPlayer.HasPermission(item.Permission))
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_no_permission");
                    return;
                }

                decimal cost = item.GetSellCost(amount);
                List<InventorySearch> search = callerPlayer.Inventory.search(asset.id, true, true);
                if (search.Count < amount)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_item_not_enough");
                    return;
                }

                if (cost == 0)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_item_sell_error");
                    return;
                }

                await TShop.EconomyProvider.DepositAsync(callerPlayer.CSteamID, cost);
                for (int i = 0; i < amount; i++)
                {
                    callerPlayer.Inventory.removeItem(search[i].page, callerPlayer.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                }
                if (TShop.EconomyProvider.HasTransactionSystem())
                    await TShop.EconomyProvider.AddTransactionAsync(callerPlayer.CSteamID, new Transaction(Guid.NewGuid(), ETransaction.SALE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0, callerPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "success_item_sell", asset.itemName, amount, cost, TShop.EconomyProvider.GetCurrencyName());
            }
            else
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_command_sellitem_args");
        }
    }
}
