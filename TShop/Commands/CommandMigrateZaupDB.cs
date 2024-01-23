using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop
{
    public class CommandMigrateZaupDB : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "migratezaupdb";
        public string Help => "Migrates data from the database of the zaupshop plugin.";
        public string Syntax => "[itemtablename] [vehicletablename]";
        public List<string> Aliases => new List<string> { "mzdb" };
        public List<string> Permissions => new List<string> { "tshop.admin.migratezaupdb" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length == 2)
            {
                try
                {
                    List<ShopItem> items = TShop.Database.GetZaupItems(args[0]);
                    List<ShopItem> vehs = TShop.Database.GetZaupVehicles(args[1]);

                    foreach (ShopItem item in items)
                    {
                        TShop.Database.AddProduct(item.UnturnedId, false, item.GetBuyCost(), item.GetSellCost(), false, "");
                    }

                    foreach (ShopItem item in vehs)
                    {
                        TShop.Database.AddProduct(item.UnturnedId, true, item.GetBuyCost(), item.GetSellCost(), false, "");
                    }
                    UChatHelper.SendCommandReply(TShop.Instance, caller,  "success_migrate");
                }
                catch (Exception ex)
                {
                    UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_migrate_console");
                    TShop.Logger.LogError("Migration error: " + ex);
                }
            }
            else
                UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_command_migrate_args");
        }
    }
}
