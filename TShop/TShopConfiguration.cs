using System.Collections.Generic;
using Rocket.API;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TShop.Compability;
using Newtonsoft.Json;

namespace Tavstal.TShop
{
    public class TShopConfiguration : ConfigurationBase
    {
        [JsonProperty(Order = 3)]
        public DatabaseData Database { get; set; }
        [JsonProperty(Order = 4)]
        public float UIButtonDelay { get; set; }
        [JsonProperty(Order = 5)]
        public bool ExpMode { get; set; }
        [JsonProperty(Order = 6)]
        public bool EnableDiscounts { get; set; }
        [JsonProperty(Order = 7)]
        public float minDiscount { get; set; }
        [JsonProperty(Order = 8)]
        public float maxDiscount{ get; set; }
        [JsonProperty(Order = 9)]
        public int ItemCountToDiscount { get; set; }
        [JsonProperty(Order = 10)]
        public int VehicleCountToDiscount { get; set; }
        [JsonProperty(Order = 11)]
        public int DiscountInterval { get; set; }
        [JsonProperty(Order = 12)]
        public string DefaultProductIconUrl { get; set; }
        [JsonProperty(Order = 13)]
        public List<GithubFolders> GithubItemFolders = new List<GithubFolders>();
        [JsonProperty(Order = 14)]
        public List<GithubFolders> GithubVehicleFolders = new List<GithubFolders>();
        [JsonIgnore]
        public readonly string MessageIcon = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Plugins/icon_plugin_tshop.png";
        [JsonIgnore]
        public readonly ushort EffectID = 8818;

        public override void LoadDefaults()
        {
            Locale = "en";
            DownloadLocalePacks = true;
            Database = new DatabaseData();
            Database.DatabaseTable_Products = "tshop_products";
            UIButtonDelay = 0.45f;
            //UseQuality = true;
            ExpMode = false;
            EnableDiscounts = true;
            minDiscount = 5;
            maxDiscount = 10;
            ItemCountToDiscount = 10;
            VehicleCountToDiscount = 5;
            DiscountInterval = 1800;
            DefaultProductIconUrl = "https://raw.githubusercontent.com/TavstalDev/Icons/master/noimage.png";
            GithubItemFolders = new List<GithubFolders>
            {
                new GithubFolders { FolderName = "0K-2K", FolderLink = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/", MinItemID = 0, MaxItemID = 2000 }
            };
            GithubVehicleFolders = new List<GithubFolders>
            {
                new GithubFolders { FolderName = "veh-0K-1K", FolderLink = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/Vehicles", MinItemID = 0, MaxItemID = 1000 },
            };
        }
    }
}
