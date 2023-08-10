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

namespace Tavstal.TShop
{
    public class CommandMigrateZaupDB : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "migratezaupdb";
        public string Help => "Migrates data from the database of the zaupshop plugin.";
        public string Syntax => "[itemtablename] [vehicletablename]";
        public List<string> Aliases => new List<string> { "mzdb" };
        public List<string> Permissions => new List<string> { "tshop.admin.migratezaupdb" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 2)
            {
                try
                {
                    List<ShopItem> items = TShop.Database.GetZaupItems(args[0]);
                    List<ShopItem> vehs = TShop.Database.GetZaupVehicles(args[1]);

                    foreach (ShopItem item in items)
                    {
                        TShop.Database.AddItem(item.Id, item.GetBuyCost(), item.GetSellCost(), false, "");
                    }

                    foreach (ShopItem item in vehs)
                    {
                        TShop.Database.AddVehicle(item.Id, item.GetBuyCost(), item.GetSellCost(), false, "");
                    }
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "success_migrate");
                }
                catch (Exception ex)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_migrate_console");
                    Logger.LogError("Migration error: " + ex);
                }
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_command_migrate_args");
        }
    }
}
