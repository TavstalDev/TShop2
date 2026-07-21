using System;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Utils.Helpers;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandSellItem : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "sell";
        public override string Help => "Sells a specific amount of items.";
        public override string Syntax => "[itemID] <amount>";
        public override List<string> Aliases => new List<string> { "sellitem", "selli" };
        public override List<string> Permissions => new List<string> { "tshop.sell.item", "tshop.commands.sell.item" };
        public override List<ISubcommand>? SubCommands => null;

        protected override async Task<bool> HandleExecuteAsync(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();
            if (args.Length < 1 || args.Length > 2)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_sellitem_args", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            ushort.TryParse(args[0], out var id);

            byte amount = 1;
            if (args.Length == 2)
                if (!byte.TryParse(args[1], out amount))
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_arg_not_number", TShop.Instance.Config.General.MessageIcon, args[1]);
                    return true;
                }

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

            await MainThreadDispatcher.RunAsync(async () =>
            {
                decimal? cost = ShopHelper.RemoveAndGetCost(callerPlayer, product, amount);
                if (cost == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_sell_error", TShop.Instance.Config.General.MessageIcon);
                    return;
                }

                decimal totalCost = cost.Value;

                await BackgroundThreadDispatcher.RunAsync(async () =>
                {
                    // Deposit the earnings into the seller's account
                    await TShop.EconomyProvider.DepositAsync(callerPlayer.CSteamID, totalCost);

                    // Add a transaction record if the transaction system is enabled
                    if (!TShop.EconomyProvider.HasTransactionSystem())
                        return;

                    await TShop.EconomyProvider.AddTransactionAsync(
                        callerPlayer.CSteamID,
                        new Transaction(
                            Guid.NewGuid().ToString(),
                            ETransaction.SALE,
                            comp.PaymentMethod,
                            TShop.Instance.Localize(true, "ui_shop_name"),
                            0,
                            callerPlayer.CSteamID.m_SteamID,
                            totalCost,
                            DateTime.Now
                        )
                    );
                });

                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_item_sell", TShop.Instance.Config.General.MessageIcon, asset.itemName,
                    amount,
                    cost, TShop.EconomyProvider.GetCurrencyName());
            });
            return true;
        }
    }
}
