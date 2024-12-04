using Rocket.API;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandMigrateZaupDB : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "migratezaupdb";
        public string Help => "Migrates data from the database of the zaupshop plugin.";
        public string Syntax => "[itemtablename] [vehicletablename]";
        public List<string> Aliases => new List<string> { "mzdb" };
        public List<string> Permissions => new List<string> { "tshop.admin.migratezaupdb", "tshop.commands.migratezaupdb" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length == 2)
            {
                try
                {
                    TShop.Logger.LogRichWarning("Started migrating the zaup db...");
                    List<ZaupProduct> products = await TShop.DatabaseManager.GetZaupProductsAsync(args[0], args[1]);

                    TShop.Logger.LogRichWarning("Migrating items...");
                    int successCount = 0;
                    List<ZaupProduct> productsToCheck = products.FindAll(x => !x.IsVehicle);
                    foreach (ZaupProduct product in productsToCheck)
                    {
                        if (UAssetHelper.FindItemAsset(product.UnturnedId) == null)
                        {
                            TShop.Logger.LogRichWarning($"&6Failed to get &citem &6asset with &e'{product.UnturnedId}' &6id.");
                            continue;
                        }

                        await TShop.DatabaseManager.AddProductAsync(product.UnturnedId, false, null, product.BuyCost, product.SellCost, false, "");
                        successCount++;
                    }
                    TShop.Logger.LogRich($"&a{successCount}&6/&2{productsToCheck.Count} &6items have been successfully migrated to TShop's table.");

                    TShop.Logger.LogRichWarning("Migrating vehicles...");
                    successCount = 0;
                    productsToCheck = products.FindAll(x => x.IsVehicle);
                    foreach (ZaupProduct product in productsToCheck)
                    {
                        if (UAssetHelper.FindVehicleAsset(product.UnturnedId) == null)
                        {
                            TShop.Logger.LogRichWarning($"&6Failed to get &cvehicle &6asset with &e'{product.UnturnedId}' &6id.");
                            continue;
                        }

                        await TShop.DatabaseManager.AddProductAsync(product.UnturnedId, true, null, product.BuyCost, product.SellCost, false, "");
                        successCount++;
                    }
                    TShop.Logger.LogRich($"&a{successCount}&6/&2{productsToCheck.Count} &6vehicles have been successfully migrated to TShop's table.");
                    TShop.Logger.LogRich("&bIf there are any items or vehicles that were not migrated then please check Zaup's database or the workshop mod on the server. The problem is not on TShop's side.");
                    TShop.Instance.SendCommandReply(caller,  "success_migrate");
                }
                catch (Exception ex)
                {
                    TShop.Instance.SendCommandReply(caller,  "error_migrate_console");
                    TShop.Logger.LogError("Migration error: " + ex);
                }
            }
            else
                TShop.Instance.SendCommandReply(caller,  "error_command_migrate_args");
        }
    }
}
