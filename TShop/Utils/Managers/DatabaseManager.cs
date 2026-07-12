using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.Database;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Managers;
using Tavstal.TLibrary.Models.Database;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;

namespace Tavstal.TShop.Utils.Managers
{
    /// <summary>
    /// Represents a database manager derived from <see cref="DatabaseManagerBase"/>.
    /// </summary>
    public class DatabaseManager : DatabaseManagerBase
    {
        // ReSharper disable once InconsistentNaming
        private static ShopConfiguration _pluginConfig => TShop.Instance.Config;
        public MySqlRepository<ulong, Product> Products { get; }

        public DatabaseManager() : base(TShop.Instance, _pluginConfig.Database)
        {
            Products = new MySqlRepository<ulong, Product>(this, _pluginConfig.Database.ProductsTable);
        }

        /// <summary>
        /// Checks the schema of the database.
        /// </summary>
        public override async Task CheckSchemaAsync()
        {
            try
            {
                await using var connection = CreateConnection();
                await Products.CheckSchemaAsync(connection);
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Error in checkSchema:", ex);
            }
        }

        /// <summary>
        /// Asynchronously adds a product to the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="vehicleColor">HEX color code of a vehicle</param>
        /// <param name="buyCost">The buy cost of the product.</param>
        /// <param name="sellCost">The sell cost of the product.</param>
        /// <param name="enablePerm">A boolean value indicating whether the product requires permission to be enabled.</param>
        /// <param name="permission">The permission required to enable the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully added to the database.
        /// </returns>
        public async Task<bool> AddProductAsync(ushort id, bool isVehicle, string? vehicleColor, decimal buyCost, decimal sellCost, bool enablePerm, string? permission)
        {
            await using var connection = CreateConnection();
            Product product = new Product(id, isVehicle, vehicleColor, buyCost, sellCost, enablePerm, permission, false, 0);
            return await Products.AddAsync(product, connection) != null;
        }

        /// <summary>
        /// Asynchronously removes a product from the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully removed from the database.
        /// </returns>
        public async Task<bool> RemoveProductAsync(ushort id, bool isVehicle)
        {
            return await Products.DeleteAsync(QueryParameter.eq("UnturnedId", id), QueryParameter.eq("IsVehicle", isVehicle));
        }

        /// <summary>
        /// Asynchronously updates a product in the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="buyCost">The new buy cost of the product.</param>
        /// <param name="sellCost">The new sell cost of the product.</param>
        /// <param name="enablePermission">A boolean value indicating whether the product requires permission to be enabled.</param>
        /// <param name="permission">The new permission required to enable the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully updated in the database.
        /// </returns>
        public async Task<bool> UpdateProductAsync(ushort id, bool isVehicle, decimal buyCost, decimal sellCost, bool enablePermission, string? permission)
        {
            return await Products.UpdateAsync(new List<UpdateParameter>
            {
                UpdateParameter.Get<Product>(x => x.BuyCost, buyCost),
                UpdateParameter.Get<Product>(x => x.SellCost, sellCost),
                UpdateParameter.Get<Product>(x => x.HasPermission, enablePermission),
                UpdateParameter.Get<Product>(x => x.Permission!, permission)
            }, QueryParameter.eq("UnturnedId", id), QueryParameter.eq("IsVehicle", isVehicle));
        }

        /// <summary>
        /// Asynchronously updates a product in the database with discount information.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="isDiscounted">A boolean value indicating whether the product is discounted.</param>
        /// <param name="percent">The discount percentage of the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully updated in the database.
        /// </returns>
        public async Task<bool> UpdateProductAsync(ushort id, bool isVehicle, bool isDiscounted, decimal percent)
        {
            return await Products.UpdateAsync(new List<UpdateParameter>
            {
                UpdateParameter.Get<Product>(x => x.IsDiscounted, isDiscounted),
                UpdateParameter.Get<Product>(x => x.DiscountPercent, percent)
            }, QueryParameter.eq("UnturnedId", id), QueryParameter.eq("IsVehicle", isVehicle));
        }
        
        /// <summary>
        /// Asynchronously updates a product in the database with vehicleColor.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="vehicleColor">HEX color code of a vehicle</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully updated in the database.
        /// </returns>
        public async Task<bool> UpdateProductAsync(ushort id, string vehicleColor)
        {
            return await Products.UpdateAsync(new List<UpdateParameter>
            {
                UpdateParameter.Get<Product>(x => x.VehicleColor!, vehicleColor)
            }, QueryParameter.eq("UnturnedId", id), QueryParameter.eq("IsVehicle", true));
        }

        /// <summary>
        /// Asynchronously retrieves a list of products from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of products retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetProductsAsync() =>
            await Products.GetAsync(int.MaxValue) ?? new List<Product>();

        /// <summary>
        /// Asynchronously retrieves a list of items from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of items retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetItemsAsync() => 
            await Products.GetAsync(int.MaxValue, QueryParameter.eq("IsVehicle", false)) ?? new List<Product>();

        /// <summary>
        /// Asynchronously retrieves a list of items from the database based on the specified filter.
        /// </summary>
        /// <param name="filter">An optional filter to apply to the items.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of items retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetItemsAsync(EItemFilter? filter)
        {
            List<Product> items = await GetItemsAsync();
            if (filter == null)
                return items;
            List<Product> local = new List<Product>();
            foreach (var item in items)
            {
                ItemAsset? asset = UAssetHelper.FindItemAsset(item.UnturnedId);
                if (asset == null)
                    continue;

                switch (asset.type)
                {
                    case EItemType.HAT:
                    case EItemType.BACKPACK:
                    case EItemType.GLASSES:
                    case EItemType.MASK:
                    case EItemType.PANTS:
                    case EItemType.SHIRT:
                    case EItemType.VEST:
                        {
                            if (filter == EItemFilter.Clothing)
                                local.Add(item);

                            break;
                        }
                    case EItemType.FOOD:
                    case EItemType.WATER:
                        {
                            if (filter == EItemFilter.Food)
                                local.Add(item);
                            break;
                        }
                    case EItemType.MEDICAL:
                        {
                            if (filter == EItemFilter.Medical)
                                local.Add(item);
                            break;
                        }
                    case EItemType.TOOL:
                    case EItemType.COMPASS:
                    case EItemType.FISHER:
                    case EItemType.KEY:
                    case EItemType.OPTIC:
                    case EItemType.MAP:
                        {
                            if (filter == EItemFilter.Tools)
                                local.Add(item);
                            break;
                        }
                    case EItemType.BARRICADE:
                    case EItemType.BOX:
                    case EItemType.CLOUD:
                    case EItemType.FARM:
                    case EItemType.STORAGE:
                    case EItemType.LIBRARY:
                    case EItemType.GROWER:
                        {
                            if (filter == EItemFilter.Barricades)
                                local.Add(item);
                            break;
                        }
                    case EItemType.STRUCTURE:
                        {
                            if (filter == EItemFilter.Structures)
                                local.Add(item);
                            break;
                        }
                    case EItemType.SENTRY:
                    case EItemType.GENERATOR:
                    case EItemType.BEACON:
                        {
                            if (filter == EItemFilter.Electronic)
                                local.Add(item);
                            break;
                        }
                    case EItemType.VEHICLE_REPAIR_TOOL:
                    case EItemType.TIRE:
                        {
                            if (filter == EItemFilter.Vehicles)
                                local.Add(item);
                            break;
                        }
                    case EItemType.FUEL:
                    case EItemType.REFILL:
                    case EItemType.OIL_PUMP:
                        {
                            if (filter == EItemFilter.Fuel)
                                local.Add(item);
                            break;
                        }
                    case EItemType.MELEE:
                        {
                            if (filter == EItemFilter.Melees)
                                local.Add(item);
                            break;
                        }
                    case EItemType.GUN:
                    case EItemType.THROWABLE:
                        {
                            if (filter == EItemFilter.Guns)
                                local.Add(item);
                            break;
                        }
                    case EItemType.BARREL:
                    case EItemType.GRIP:
                    case EItemType.MAGAZINE:
                    case EItemType.SIGHT:
                    case EItemType.TACTICAL:
                        {
                            if (filter == EItemFilter.Attachments)
                                local.Add(item);
                            break;
                        }
                    case EItemType.ARREST_END:
                    case EItemType.ARREST_START:
                    case EItemType.CHARGE:
                    case EItemType.DETONATOR:
                    case EItemType.FILTER:
                    case EItemType.SUPPLY:
                    case EItemType.TANK:
                    case EItemType.TRAP:
                        {
                            if (filter == EItemFilter.Misc)
                                local.Add(item);
                            break;
                        }
                }
            }
            return local;
        }

        /// <summary>
        /// Asynchronously retrieves a list of vehicles from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of vehicles retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetVehiclesAsync() => 
            await Products.GetAsync(int.MaxValue, QueryParameter.eq("IsVehicle", true)) ?? new List<Product>();

        /// <summary>
        /// Asynchronously retrieves a list of vehicles from the database based on the specified engine type.
        /// </summary>
        /// <param name="engine">An optional engine type to filter the vehicles.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of vehicles retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetVehiclesAsync(EEngine? engine)
        {
            List<Product> vehicles = await GetVehiclesAsync();
            if (engine == null)
                return vehicles;
            List<Product> local = new List<Product>();
            foreach (var vehicle in vehicles)
            {
                VehicleAsset? asset = UAssetHelper.FindVehicleAsset(vehicle.UnturnedId);
                if (asset == null)
                    continue;

                if (asset.engine == engine)
                    local.Add(vehicle);
            }
            return local;
        }

        /// <summary>
        /// Asynchronously finds an item in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the item to find.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the item found in the database, if any.
        /// </returns>
        public async Task<Product?> FindItemAsync(ushort id)
        {
            var result = await Products.GetAsync(1, QueryParameter.eq("UnturnedId", id),
                QueryParameter.eq("IsVehicle", false));
            if (result == null || result.Count == 0)
                return null;
            return result[0];
        }

        /// <summary>
        /// Asynchronously finds a vehicle in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle to find.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the vehicle found in the database, if any.
        /// </returns>
        public async Task<Product?> FindVehicleAsync(ushort id)
        {
            var result = await Products.GetAsync(1, QueryParameter.eq("UnturnedId", id),
                QueryParameter.eq("IsVehicle", true));
            if (result == null || result.Count == 0)
                return null;
            return result[0];
        }

        #region Zaup
        /// <summary>
        /// Creates a MySQL connection to the Zaup database.
        /// </summary>
        /// <returns>
        /// A MySqlConnection object representing the connection to the Zaup database.
        /// </returns>
        public MySqlConnection? CreateZaupConnection()
        {
            MySqlConnection? mySqlConnection = null;
            try
            {
                if (_pluginConfig.Database.Port == 0)
                {
                    _pluginConfig.Database.Port = 3306;
                    _pluginConfig.SaveConfig();
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    _pluginConfig.Database.Host,
                    _pluginConfig.Database.DatabaseName,
                    _pluginConfig.Database.UserName,
                    _pluginConfig.Database.UserPassword,
                    _pluginConfig.Database.Port
                }));
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Failed to create zaup connection.", ex);
            }
            return mySqlConnection;
        }

        /// <summary>
        /// Asynchronously retrieves a list of Zaup products from the specified item and vehicle tables in the Zaup database.
        /// </summary>
        /// <param name="itemTable">The name of the table containing item products.</param>
        /// <param name="vehicleTable">The name of the table containing vehicle products.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of Zaup products retrieved from the specified tables.
        /// </returns>
        public async Task<List<ZaupProduct>> GetZaupProductsAsync(string itemTable, string vehicleTable)
        {
            List<ZaupProduct> items = new List<ZaupProduct>();
            try
            {
                MySqlConnection? mySqlConnection = CreateZaupConnection();
                if (mySqlConnection == null)
                    return items;
                
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                await mySqlConnection.OpenSafeAsync();

                // Get Items
                mySqlCommand.CommandText = "SELECT * FROM " + itemTable;
                var reader = await mySqlCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ZaupProduct prod;
                    try
                    {
                        prod = new ZaupProduct(reader.GetUInt16("id"), false, reader.GetDecimal("cost"), reader.GetDecimal("buyback"));
                    }
                    catch (OverflowException) {
                        uint id = reader.GetUInt32("id");
                        prod = new ZaupProduct((ushort)id, false, reader.GetDecimal("cost"), reader.GetDecimal("buyback"));
                    }
                    items.Add(prod);
                }
                reader.Close();

                // Get Vehicles
                mySqlCommand.CommandText = "SELECT * FROM " + vehicleTable;
                reader = await mySqlCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ZaupProduct prod;
                    try
                    {
                        prod = new ZaupProduct(reader.GetUInt16("id"), true, reader.GetDecimal("cost"), 0);
                    }
                    catch (OverflowException) {
                        uint id = reader.GetUInt32("id");
                        prod = new ZaupProduct((ushort)id, true, reader.GetDecimal("cost"), 0);
                    }
                    items.Add(prod);
                }
                await mySqlConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("This error might be caused by the database because it does not use ushort (uint16) as itemId, or decimal as price.", ex);
            }
            return items;
        }
        #endregion
    }
}
