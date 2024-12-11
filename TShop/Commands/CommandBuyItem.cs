using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandBuyItem : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "buy";
        public override string Help => "Buys a specific amount of items.";
        public override string Syntax => "[itemID | itemName] <amount>";
        public override List<string> Aliases => new List<string> { "buyi", "buyitem" };
        public override List<string> Permissions => new List<string> { "tshop.buy.item", "tshop.commands.buy.item" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length < 1 || args.Length > 2)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_buyitem_args");
                return true;
            }

            ushort id = 0;
            int amount = 1;
            // Get ID
            try
            {
                ushort.TryParse(args[0], out id);
            }
            catch
            {
                /* ignore */
            }

            // Get Amount
            if (args.Length == 2)
            {
                try
                {
                    int.TryParse(args[1], out amount);
                }
                catch
                {
                    /* ignore */
                }
            }

            ItemAsset asset;
            if (id > 0)
                asset = UAssetHelper.FindItemAsset(id);
            else
                asset = UAssetHelper.FindItemAsset(args[0]);

            if (asset == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_found", args[0]);
                return true;
            }

            id = asset.id;

            Product item = await TShop.DatabaseManager.FindItemAsync(id);
            if (item == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_added", args[0]);
                return true;
            }

            if (item.HasPermission && !callerPlayer.HasPermission(item.Permission))
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_no_permission");
                return true;
            }

            decimal cost = item.GetBuyCost(amount);

            if (await TShop.EconomyProvider.GetBalanceAsync(callerPlayer.CSteamID) < cost)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_balance_not_enough",
                    cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
                return true;
            }

            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_buy_error");
                return true;
            }

            await TShop.EconomyProvider.WithdrawAsync(callerPlayer.CSteamID, cost);
            await MainThreadDispatcher.RunOnMainThreadAsync(() =>
            {
                for (int i = 0; i < amount; i++)
                {
                    if (!callerPlayer.Inventory.tryAddItem(new Item(asset.id, true), false))
                        ItemManager.dropItem(new Item(asset.id, true), callerPlayer.Position, true, true, false);
                }
            });
            

            if (TShop.EconomyProvider.HasTransactionSystem())
                await TShop.EconomyProvider.AddTransactionAsync(callerPlayer.CSteamID,
                    new Transaction(Guid.NewGuid().ToString(), ETransaction.PURCHASE, comp.PaymentMethod,
                        TShop.Instance.Localize(true, "ui_shopname"), callerPlayer.CSteamID.m_SteamID, 0, cost,
                        DateTime.Now
                        ));
            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_item_buy", asset.itemName, amount,
                cost, TShop.EconomyProvider.GetCurrencyName());
            return true;
        }
    }
}
