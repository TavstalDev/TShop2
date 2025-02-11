using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Models;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;

namespace Tavstal.TShop
{
    [Serializable]
    public class ShopConfiguration : ConfigurationBase
    {
        [JsonProperty(Order = 3)]
        public DatabaseData Database { get; set; }
        [JsonProperty(Order = 4)]
        public float UIButtonDelay { get; set; }
        [JsonProperty(Order = 5)]
        public bool UseQuality { get; set; }
        [JsonProperty(Order = 6)]
        public bool ExpMode { get; set; }
        [JsonProperty(Order = 7)]
        public bool EnableDiscounts { get; set; }
        [JsonProperty(Order = 8)]
        public float MinDiscount { get; set; }
        [JsonProperty(Order = 9)]
        public float MaxDiscount{ get; set; }
        [JsonProperty(Order = 10)]
        public int ItemCountToDiscount { get; set; }
        [JsonProperty(Order = 11)]
        public int VehicleCountToDiscount { get; set; }
        [JsonProperty(Order = 12)]
        public int DiscountInterval { get; set; }
        [JsonProperty(Order = 13)]
        public string DefaultProductIconUrl { get; set; }
        [JsonProperty(Order = 14)]
        public SerializableVector3 VehicleSpawnModifier { get; set; }
        [JsonProperty(PropertyName = "ItemFolders", Order = 15)]
        public List<FileServerFolder> ItemFolders { get; set; }
        [JsonProperty(PropertyName = "VehicleFolders", Order = 16)]
        public List<FileServerFolder> VehicleFolders { get; set; }
        [JsonIgnore]
        public readonly string MessageIcon = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Plugins/icon_plugin_tshop.png";
        [JsonIgnore]
        public readonly ushort EffectID = 8818;

        public override void LoadDefaults()
        {
            DebugMode = false;
            Locale = "en";
            DownloadLocalePacks = true;
            Database = new DatabaseData("tshop_products");
            UIButtonDelay = 0.45f;
            UseQuality = true;
            ExpMode = false;
            EnableDiscounts = true;
            MinDiscount = 5;
            MaxDiscount = 10;
            ItemCountToDiscount = 10;
            VehicleCountToDiscount = 5;
            DiscountInterval = 1800;
            DefaultProductIconUrl = "https://raw.githubusercontent.com/TavstalDev/Icons/master/noimage.png";
            VehicleSpawnModifier = new SerializableVector3(0, 5, 0);
            ItemFolders = new List<FileServerFolder>
            {
                new FileServerFolder("0-2000", "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/", 0, 2000),
                new FileServerFolder("my-server", "https://api.myserver.com/items/", 0, 60000)
            };
            VehicleFolders = new List<FileServerFolder>
            {
                new FileServerFolder("0-2000", "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/Vehicles", 0, 1000),
                new FileServerFolder("my-server", "https://api.myserver.com/vehicles/", 0, 60000),
            };
        }
        
        public ShopConfiguration(string filename, string path) : base(filename, path)
        {
        }

        public ShopConfiguration()
        {
        }
    }
}
