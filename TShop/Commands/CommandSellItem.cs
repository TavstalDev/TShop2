﻿using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Utils.Helpers;

namespace Tavstal.TShop.Commands
{
    public class CommandSellItem : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "sell";
        public override string Help => "Sells a specific amount of items.";
        public override string Syntax => "[itemID] <amount>";
        public override List<string> Aliases => new List<string> { "sellitem", "selli" };
        public override List<string> Permissions => new List<string> { "tshop.sell.item", "tshop.commands.sell.item" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length < 1 || args.Length > 2)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_sellitem_args");
                return true;
            }

            ushort id = 0;
            byte amount = 1;
            try
            {
                ushort.TryParse(args[0], out id);
            }
            catch
            {
                /* ignore */
            }

            if (args.Length == 2)
            {
                try
                {
                    byte.TryParse(args[1], out amount);
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

            decimal cost = item.GetSellCost(amount);
            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_sell_error");
                return true;
            }

            if (!await ShopHelper.SellItemAsync(callerPlayer, id, cost, amount, comp.PaymentMethod))
                return true;
            
            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_item_sell", asset.itemName, amount,
                cost, TShop.EconomyProvider.GetCurrencyName());
            return true;
        }
    }
}
