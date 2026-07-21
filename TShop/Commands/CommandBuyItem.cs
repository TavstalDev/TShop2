using System;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDG.Unturned;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;

// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandBuyItem : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;

        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "buy";
        public override string Help => "Buys a specific amount of items.";
        public override string Syntax => "[itemID | itemName] <amount>";
        public override List<string> Aliases => new List<string> { "buyi", "buyitem" };
        public override List<string> Permissions => new List<string> { "tshop.buy.item", "tshop.commands.buy.item" };
        public override List<ISubcommand>? SubCommands => null;

        protected override async Task<bool> HandleExecuteAsync(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length < 1 || args.Length > 2)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_buyitem_args", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            byte amount = 1;
            ushort.TryParse(args[0], out var id);

            // Get Amount
            if (args.Length == 2)
                byte.TryParse(args[1], out amount);

            var asset = id > 0 ? UAssetHelper.FindItemAsset(id) : UAssetHelper.FindItemAsset(args[0]);

            if (asset == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_found", TShop.Instance.Config.General.MessageIcon, args[0]);
                return true;
            }

            id = asset.id;

            Product? product = await TShop.DatabaseManager.FindItemAsync(id);
            if (product == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_added", TShop.Instance.Config.General.MessageIcon, args[0]);
                return true;
            }

            if (product.HasPermission && !callerPlayer.HasPermission(product.Permission))
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_no_permission", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            decimal cost = product.GetBuyCost(amount);
            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_buy_error", TShop.Instance.Config.General.MessageIcon);
                return true;
            }
            
            if (await TShop.EconomyProvider.GetBalanceAsync(callerPlayer.CSteamID) < cost)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_balance_not_enough", TShop.Instance.Config.General.MessageIcon,
                    cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
                return true;
            }
            
            await TShop.EconomyProvider.WithdrawAsync(callerPlayer.CSteamID, cost, comp.PaymentMethod);

            if (TShop.EconomyProvider.HasTransactionSystem())
            {
                await TShop.EconomyProvider.AddTransactionAsync(
                    callerPlayer.CSteamID,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.PURCHASE,
                        comp.PaymentMethod,
                        TShop.Instance.Localize(true, "ui_shop_name"),
                        0,
                        callerPlayer.CSteamID.m_SteamID,
                        cost,
                        DateTime.Now
                    )
                );
            }

            await MainThreadDispatcher.RunAsync(() =>
            {
                for (int i = 0; i < amount; i++)
                {
                    var item = new Item(asset.id, true);
                    if (!callerPlayer.Inventory.tryAddItem(item, false))
                        ItemManager.dropItem(item, callerPlayer.Position, true, true, false);
                }
                    
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_item_buy", TShop.Instance.Config.General.MessageIcon, asset.itemName, amount,
                    cost, TShop.EconomyProvider.GetCurrencyName());
            });
            return true;
        }
    }
}
