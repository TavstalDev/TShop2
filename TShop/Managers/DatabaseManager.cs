using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Steamworks;
using System.Reflection;
using Rocket.API;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using static SDG.Provider.SteamGetInventoryResponse;
using Tavstal.TLibrary.Managers;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Helpers;
using Tavstal.TLibrary.Compatibility.Database;
using Tavstal.TLibrary.Extensions;
using Rocket.Core.Logging;
using Tavstal.TShop.Compatibility.Enums;
using System.Web.Caching;

namespace Tavstal.TShop
{
    public class DatabaseManager : DatabaseManagerBase
    {
        private static TShopConfiguration pluginConfig => TShop.Instance.Config;

        public DatabaseManager(IConfigurationBase configuration) : base(configuration)
        {
        }

        public override void CheckSchema()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.OpenSafe();
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        throw new Exception("# Failed to connect to the database. Please check the plugin's config file.");
                    }
                }

                MySqlConnection MySQLConnection = CreateConnection();
                if (MySQLConnection == null)
                {
                    throw new Exception("# Failed to connect to the database. Please check the plugin's config file.");
                }

                //Item Shop
                if (MySQLConnection.DoesTableExist<ShopItem>(pluginConfig.Database.DatabaseTable_Items))
                    MySQLConnection.CheckTable<ShopItem>(pluginConfig.Database.DatabaseTable_Items);
                else
                    MySQLConnection.CreateTable<ShopItem>(pluginConfig.Database.DatabaseTable_Items);


                //Vehicle Shop
                if (MySQLConnection.DoesTableExist<ShopItem>(pluginConfig.Database.DatabaseTable_Vehicles))
                    MySQLConnection.CheckTable<ShopItem>(pluginConfig.Database.DatabaseTable_Vehicles);
                else
                    MySQLConnection.CreateTable<ShopItem>(pluginConfig.Database.DatabaseTable_Vehicles);

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        #region Items
        public bool AddItem(ushort itemId, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.AddTableRow(tableName: pluginConfig.Database.DatabaseTable_Items, new ShopItem(itemId, buycost, sellcost, enableperm, permission, false, 0));
        }  
        
        public bool RemoveItem(ushort itemId)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.RemoveTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Items, $"Id='{itemId}'", null);
        }
        public bool UpdateItem(ushort itemId, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.UpdateTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Items, $"Id='{itemId}'", new List<SqlParameter>
            {
                SqlParameter.Get<ShopItem>(x => x.BuyCost, buycost),
                SqlParameter.Get<ShopItem>(x => x.SellCost, sellcost),
                SqlParameter.Get<ShopItem>(x => x.hasPermission, enablepermission),
                SqlParameter.Get<ShopItem>(x => x.Permission, permission)
            });
        }
        public bool UpdateItem(ushort itemId, bool isdiscounted, decimal percent)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.UpdateTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Items, $"Id='{itemId}'", new List<SqlParameter>
            {
                SqlParameter.Get<ShopItem>(x => x.isDiscounted, isdiscounted),
                SqlParameter.Get<ShopItem>(x => x.discountPercent, percent)
            });
        }
        public List<ShopItem> GetItems()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.GetTableRows<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Items, whereClause: string.Empty, null);
        }

        public List<ShopItem> GetItems(EItemFilter? filter)
        {
            List<ShopItem> items = GetItems();
            if (filter == null)
                return items;
            List<ShopItem> local = new List<ShopItem>();
            foreach (var item in items)
            {
                ItemAsset asset = (ItemAsset)Assets.find(EAssetType.ITEM, item.Id);
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
                    case EItemType.WATER:
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
        public ShopItem FindItem(ushort itemId)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.GetTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Items, whereClause: $"Id='{itemId}'", null);
        }
        #endregion

        #region Vehicles
        public bool AddVehicle(ushort vehicleId, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.AddTableRow(tableName: pluginConfig.Database.DatabaseTable_Vehicles, new ShopItem(vehicleId, buycost, sellcost, enableperm, permission, false, 0));
        }
        public bool RemoveVehicle(ushort vehicleId)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.RemoveTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Vehicles, $"Id='{vehicleId}'", null);
        }
        public bool UpdateVehicle(ushort vehicleId, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.UpdateTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Vehicles, $"Id='{vehicleId}'", new List<SqlParameter>
            {
                SqlParameter.Get<ShopItem>(x => x.BuyCost, buycost),
                SqlParameter.Get<ShopItem>(x => x.SellCost, sellcost),
                SqlParameter.Get<ShopItem>(x => x.hasPermission, enablepermission),
                SqlParameter.Get<ShopItem>(x => x.Permission, permission)
            });
        }

        public bool UpdateVehicle(ushort vehicleId, bool isdiscounted, decimal percent)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.UpdateTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Vehicles, $"Id='{vehicleId}'", new List<SqlParameter>
            {
                SqlParameter.Get<ShopItem>(x => x.isDiscounted, isdiscounted),
                SqlParameter.Get<ShopItem>(x => x.discountPercent, percent)
            });
        }

        public List<ShopItem> GetVehicles()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.GetTableRows<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Vehicles, whereClause: string.Empty, null);
        }

        public List<ShopItem> GetVehicles(EEngine? engine)
        {
            List<ShopItem> vehicles = GetVehicles();
            if (engine == null)
                return vehicles;
            List<ShopItem> local = new List<ShopItem>();
            foreach (var vehicle in vehicles)
            {
                VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicle.Id);
                if (asset == null)
                    continue;

                if (asset.engine == engine)
                    local.Add(vehicle);
            }
            return local;
        }
        public ShopItem FindVehicle(ushort vehicleId)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return MySQLConnection.GetTableRow<ShopItem>(tableName: pluginConfig.Database.DatabaseTable_Vehicles, whereClause: $"Id='{vehicleId}'", null);
        }
        #endregion

        #region Zaup
        public MySqlConnection CreateZaupConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (pluginConfig.Database.Port == 0)
                {
                    pluginConfig.Database.Port = 3306;
                    pluginConfig.SaveConfig();
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    pluginConfig.Database.Host,
                    pluginConfig.Database.DatabaseName,
                    pluginConfig.Database.UserName,
                    pluginConfig.Database.UserPassword,
                    pluginConfig.Database.Port
                }));
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return mySqlConnection;
        }

        public List<ShopItem> GetZaupItems(string tablename)
        {
            List<ShopItem> i = new List<ShopItem>();
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.OpenSafe();

                MySQLCommand.CommandText = "SELECT * FROM " + tablename;
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();
                while (Reader.Read())
                    i.Add(new ShopItem(Reader.GetUInt16("id"), Reader.GetDecimal("cost"), Reader.GetDecimal("buyback")));
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return i;
        }

        public List<ShopItem> GetZaupVehicles(string tablename)
        {
            List<ShopItem> i = new List<ShopItem>();
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.OpenSafe();

                MySQLCommand.CommandText = "SELECT * FROM " + tablename;
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();
                while (Reader.Read())
                    i.Add(new ShopItem(Reader.GetUInt16("id"), Reader.GetDecimal("cost"), 0));
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return i;
        }
        #endregion
    }
}
