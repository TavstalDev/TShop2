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
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();

                //Item Shop
                MySQLCommand.CommandText = "SHOW TABLES LIKE '" + pluginConfig.Database.DatabaseTable_Items + "'";
                MySQLConnection.Open();

                object result = MySQLCommand.ExecuteScalar();
                if (result == null)
                {
                    MySQLCommand.CommandText = "CREATE TABLE " + pluginConfig.Database.DatabaseTable_Items +
                    "(id INT(8) NOT NULL AUTO_INCREMENT," +
                    "itemID INT UNSIGNED NOT NULL," +
                    "buyCost DECIMAL(10,2) NULL," +
                    "sellCost DECIMAL(10,2) NULL," +
                    "hasPermission BOOL NOT NULL," +
                    "permission TEXT NULL," +
                    "isDiscounted BOOL NOT NULL," +
                    "discount TINYINT NULL," +
                    "PRIMARY KEY(id));";
                    MySQLCommand.ExecuteNonQuery();
                }

                //Vehicle Shop
                MySQLCommand.CommandText = "SHOW TABLES LIKE '" + pluginConfig.Database.DatabaseTable_Vehicles + "'";
                result = MySQLCommand.ExecuteScalar();
                if (result == null)
                {
                    MySQLCommand.CommandText = "CREATE TABLE " + pluginConfig.Database.DatabaseTable_Vehicles +
                    "(id INT(8) NOT NULL AUTO_INCREMENT," +
                    "vehicleID INT UNSIGNED NOT NULL," +
                    "buyCost DECIMAL(10,2) NULL," +
                    "sellCost DECIMAL(10,2) NULL," +
                    "hasPermission BOOL NOT NULL," +
                    "permission TEXT NULL," +
                    "isDiscounted BOOL NOT NULL," +
                    "discount TINYINT NULL," +
                    "PRIMARY KEY(id));";
                    MySQLCommand.ExecuteNonQuery();
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public MySqlConnection CreateConnection()
        {
            MySqlConnection MySQLConnection = null;

            try
            {
                if (pluginConfig.Database.Port == 0)
                {
                    pluginConfig.Database.Port = 3306;
                }
                MySQLConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};DEFAULT COMMAND TIMEOUT=120;CharSet=utf8;", new object[] {
                    pluginConfig.Database.DatabaseAddress,
                    pluginConfig.Database.DatabaseName,
                    pluginConfig.Database.DatabaseUser,
                    pluginConfig.Database.DatabasePassword,
                    pluginConfig.Database.Port,
                    pluginConfig.Database.TimeOut
                }));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return MySQLConnection;
        }

        #region Items
        public bool AddItem(ushort ID, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);
                MySQLCommand.Parameters.AddWithValue("@BCOST", buycost.ToString());
                MySQLCommand.Parameters.AddWithValue("@SCOST", sellcost.ToString());
                MySQLCommand.Parameters.AddWithValue("@HP", enableperm);
                MySQLCommand.Parameters.AddWithValue("@P", permission ?? string.Empty);
                MySQLCommand.Parameters.AddWithValue("@IDIS", "False");
                MySQLCommand.Parameters.AddWithValue("@DIS", "0");

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Items + "` WHERE itemID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data == null)
                {
                    MySQLCommand.CommandText = "INSERT INTO `" + pluginConfig.Database.DatabaseTable_Items + "` (ItemID,BuyCost,SellCost,hasPermission,Permission,isDiscounted,discount) " +
                        "VALUES (@ID,@BCOST,@SCOST,@HP,@P,@IDIS,@DIS);";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in AddItem():");
                Logger.LogError(ex);
            }
            return success;
        }  
        
        public bool RemoveItem(ushort ID)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();

                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "DELETE FROM `" + pluginConfig.Database.DatabaseTable_Items + "` WHERE itemID=@ID;";
                MySQLCommand.ExecuteNonQuery();
                success = true;

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in RemoveItem():");
                Logger.LogError(ex);
            }
            return success;
        }
        public bool UpdateItem(ushort ID, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Items + "` WHERE itemID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data != null)
                {
                    MySQLCommand.Parameters.AddWithValue("@BCOST", buycost);
                    MySQLCommand.Parameters.AddWithValue("@SCOST", sellcost);
                    MySQLCommand.Parameters.AddWithValue("@EP", enablepermission);
                    MySQLCommand.Parameters.AddWithValue("@P", permission ?? String.Empty);

                    MySQLCommand.CommandText = "UPDATE `" + pluginConfig.Database.DatabaseTable_Items + "` SET buyCost=@BCOST,sellCost=@SCOST,haspermission=@EP,permission=@P WHERE itemID=@ID;";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in UpdateItem():");
                Logger.LogError(ex);
            }
            return success;
        }
        public bool UpdateItem(ushort ID, bool isdiscounted, decimal percent)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Items + "` WHERE itemID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data != null)
                {
                    MySQLCommand.Parameters.AddWithValue("@DISCOUNTED", isdiscounted);
                    MySQLCommand.Parameters.AddWithValue("@PER", percent);

                    MySQLCommand.CommandText = "UPDATE `" + pluginConfig.Database.DatabaseTable_Items + "` SET isDiscounted=@DISCOUNTED,discount=@PER WHERE itemID=@ID;";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in UpdateItem():");
                Logger.LogError(ex);
            }
            return success;
        }
        public List<ShopItem> GetItems()
        {
            List<ShopItem> i = new List<ShopItem>();
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Items + "`;";
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();

                while (Reader.Read())
                {
                    ShopItem item = new ShopItem(Reader.GetUInt16("itemID"), decimal.Parse(Reader.GetString("buyCost").Replace(".", ",")),
                        decimal.Parse(Reader.GetString("sellCost").Replace(".", ",")), Reader.GetBoolean("hasPermission"),
                        Reader.GetString("permission"), Reader.GetBoolean("isDiscounted"),
                        decimal.Parse(Reader.GetString("discount").Replace(".", ",")));
                    if (item !=  null)
                        i.Add(item);
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in GetItems():");
                Logger.LogError(ex);
            }
            return i;
        }
        public ShopItem FindItem(ushort ItemId)
        {
            ShopItem i = null;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ItemId);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Items + "` WHERE itemID=@ID;";
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();

                if (Reader == null)
                    return null;

                while (Reader.Read())
                {
                    i = new ShopItem(Reader.GetUInt16("itemID"), decimal.Parse(Reader.GetString("buyCost").Replace(".", ",")),
                        decimal.Parse(Reader.GetString("sellCost").Replace(".", ",")), Reader.GetBoolean("hasPermission"),
                        Reader.GetString("permission"), Reader.GetBoolean("isDiscounted"),
                        decimal.Parse(Reader.GetString("discount").Replace(".", ",")));
                    break;    
                }
                Reader.Close();
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in FindItem():");
                Logger.LogError(ex);
            }
            return i;
        }
        #endregion

        #region Vehicles
        public bool AddVehicle(ushort ID, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` WHERE vehicleID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data == null)
                {

                    MySQLCommand.CommandText = "INSERT INTO `" + pluginConfig.Database.DatabaseTable_Vehicles + "` (vehicleID,BuyCost,SellCost,hasPermission,Permission,isDiscounted,discount) " +
                        "VALUES ('" + ID + "','" + buycost + "','" + sellcost + "','" + enableperm + "','" + permission + "','" + "False" + "','" + "0" + "');";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in AddVehicle():");
                Logger.LogError(ex);
            }
            return success;
        }
        public bool RemoveVehicle(ushort ID)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` WHERE vehicleID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data != null)
                {
                    MySQLCommand.CommandText = "DELETE FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` WHERE vehicleID=@ID;";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in RemoveVehicle():");
                Logger.LogError(ex);
            }
            return success;
        }
        public bool UpdateVehicle(ushort ID, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@TABLE", pluginConfig.Database.DatabaseTable_Vehicles);
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` WHERE vehicleID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data != null)
                {
                    MySQLCommand.Parameters.AddWithValue("@ID", ID);
                    MySQLCommand.Parameters.AddWithValue("@BCOST", buycost);
                    MySQLCommand.Parameters.AddWithValue("@SCOST", sellcost);
                    MySQLCommand.Parameters.AddWithValue("@EP", enablepermission);
                    MySQLCommand.Parameters.AddWithValue("@P", permission);

                    MySQLCommand.CommandText = "UPDATE `" + pluginConfig.Database.DatabaseTable_Vehicles + "` SET buyCost=@BCOST,sellCost=@SCOST,haspermission=@EP,permission=@P WHERE vehicleID=@ID;";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in UpdateVehicle():");
                Logger.LogError(ex);
            }
            return success;
        }

        public bool UpdateVehicle(ushort ID, bool isdiscounted, decimal percent)
        {
            bool success = false;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ID);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` WHERE vehicleID=@ID;";
                object data = MySQLCommand.ExecuteScalar();

                Asset a = Assets.find(EAssetType.ITEM, ID);
                if (data != null)
                {
                    MySQLCommand.Parameters.AddWithValue("@DISCOUNTED", isdiscounted);
                    MySQLCommand.Parameters.AddWithValue("@PER", percent);

                    MySQLCommand.CommandText = "UPDATE `" + pluginConfig.Database.DatabaseTable_Vehicles + "` SET isDiscounted=@DISCOUNTED,discount=@PER WHERE vehicleID=@ID;";
                    MySQLCommand.ExecuteNonQuery();
                    success = true;
                }

                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in UpdateVehicle():");
                Logger.LogError(ex);
            }
            return success;
        }

        public List<ShopItem> GetVehicles()
        {
            List<ShopItem> i = new List<ShopItem>();
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "`;";
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();

                while (Reader.Read())
                {
                    ShopItem item = new ShopItem(Reader.GetUInt16("vehicleID"), decimal.Parse(Reader.GetString("buyCost").Replace(".", ",")),
                        decimal.Parse(Reader.GetString("sellCost").Replace(".", ",")), Reader.GetBoolean("hasPermission"),
                        Reader.GetString("permission"), Reader.GetBoolean("isDiscounted"),
                        decimal.Parse(Reader.GetString("discount").Replace(".", ",")));
                    if (item != null)
                        i.Add(item);
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in GetVehicles():");
                Logger.LogError(ex);
            }
            return i;
        }
        public ShopItem FindVehicle(ushort ItemId)
        {
            ShopItem i = null;
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                MySQLConnection.Open();
                MySQLCommand.Parameters.AddWithValue("@ID", ItemId);

                MySQLCommand.CommandText = "SELECT * FROM `" + pluginConfig.Database.DatabaseTable_Vehicles + "` vehicleID=@ID;";
                MySqlDataReader Reader = MySQLCommand.ExecuteReader();

                if (Reader == null)
                    return null;

                while (Reader.Read())
                {
                    i = new ShopItem(Reader.GetUInt16("vehicleID"), decimal.Parse(Reader.GetString("buyCost").Replace(".", ",")),
                        decimal.Parse(Reader.GetString("sellCost").Replace(".", ",")), Reader.GetBoolean("hasPermission"),
                        Reader.GetString("permission"), Reader.GetBoolean("isDiscounted"),
                        decimal.Parse(Reader.GetString("discount").Replace(".", ",")));
                }
                MySQLConnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in FindVehicle():");
                Logger.LogError(ex);
            }
            return i;
        }
        #endregion

        #region Zaup
        public MySqlConnection createConnectionzaup()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (pluginConfig.Database.DatabasePort == 0)
                {
                    pluginConfig.Database.DatabasePort = 3306;
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    pluginConfig.Database.DatabaseAddress,
                    pluginConfig.Database.DatabaseName,
                    pluginConfig.Database.DatabaseUser,
                    pluginConfig.Database.DatabasePassword,
                    pluginConfig.Database.DatabasePort
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
                MySQLConnection.Open();

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
                MySQLConnection.Open();

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
