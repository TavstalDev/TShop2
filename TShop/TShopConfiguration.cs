using System.Collections.Generic;
using Rocket.API;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TShop.Compability;
using YamlDotNet.Serialization;

namespace Tavstal.TShop
{
    public class TShopConfiguration : ConfigurationBase
    {
        [YamlMember(Order = 3, Description = "Database related settings")]
        public DatabaseData Database { get; set; }
        /*[YamlMember(Order = 4)]
        public bool UseQuality { get; set; }*/
        [YamlMember(Order = 5, Description = "Enabling this will mean the plugin will use your experience as balance instead of economy plugin.\nWARNING: Don't enable it if you use UconomyExp or any economy plugin that has exp mode.")]
        public bool ExpMode { get; set; }
        [YamlMember(Order = 6, Description = "Enables the discount system for items and vehicles in the shop.")]
        public bool EnableDiscounts { get; set; }
        [YamlMember(Order = 7, Description = "The minimum discount value in percent. It should be smaller than maxDiscount.\nValue Range: 1.0 - 100.0")]
        public float minDiscount { get; set; }
        [YamlMember(Order = 8, Description = "The maximum discount value in percent. It should be bigger than minDiscount.\nValue Range: 1.0 - 100.0")]
        public float maxDiscount{ get; set; }
        [YamlMember(Order = 9, Description = "The amount of items to be discounted.")]
        public int ItemCountToDiscount { get; set; }
        [YamlMember(Order = 10, Description = "The amount of vehicles to be discounted.")]
        public int VehicleCountToDiscount { get; set; }
        [YamlMember(Order = 11, Description = "The time in seconds that controls how often should change the discounted products.")]
        public int DiscountInterval { get; set; }
        [YamlMember(Order = 12, Description = "This icon will show up when the plugin couldn't found an icon.")]
        public string DefaultProductIconUrl { get; set; }
        [YamlMember(Order = 13, Description = "wip")]
        public List<GithubFolders> GithubItemFolders = new List<GithubFolders>();
        [YamlMember(Order = 14, Description = "wip")]
        public List<GithubFolders> GithubVehicleFolders = new List<GithubFolders>();
        [YamlIgnore]
        public readonly string MessageIcon = "";
        [YamlIgnore]
        public readonly ushort EffectID = 8818;

        public override void LoadDefaults()
        {
            Locale = "en";
            DownloadLocalePacks = false;
            Database = new DatabaseData();
            Database.DatabaseTable_Items = "tshop_items";
            Database.DatabaseTable_Vehicles = "tshop_vehicles";
            //UseQuality = true;
            ExpMode = false;
            EnableDiscounts = true;
            minDiscount = 5;
            maxDiscount = 10;
            ItemCountToDiscount = 10;
            VehicleCountToDiscount = 5;
            DiscountInterval = 1800;
            DefaultProductIconUrl = "https://durmazz.com/writable/uploads/products/default.jpg";
            GithubItemFolders = new List<GithubFolders>
            {
                new GithubFolders { FolderName = "0K-1K", FolderLink = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/Icons1/", MinItemID = 0, MaxItemID = 1000 },
                new GithubFolders { FolderName = "1K-2K", FolderLink = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Vanilla/Icons2/", MinItemID = 1001, MaxItemID = 2000 },
                new GithubFolders { FolderName = "2K-3K",FolderLink = "", MinItemID = 2001, MaxItemID = 3000 },
                new GithubFolders { FolderName = "3K-4K",FolderLink = "", MinItemID = 3001, MaxItemID = 4000 },
                new GithubFolders { FolderName = "4K-5K",FolderLink = "", MinItemID = 4001, MaxItemID = 5000 },
                new GithubFolders { FolderName = "5K-6K",FolderLink = "", MinItemID = 5001, MaxItemID = 6000 },
                new GithubFolders { FolderName = "6K-7K",FolderLink = "", MinItemID = 6001, MaxItemID = 7000 },
                new GithubFolders { FolderName = "7K-8K",FolderLink = "", MinItemID = 7001, MaxItemID = 8000 },
                new GithubFolders { FolderName = "8K-9K",FolderLink = "", MinItemID = 8001, MaxItemID = 9000 },
                new GithubFolders { FolderName = "9K-10K",FolderLink = "", MinItemID = 9001, MaxItemID = 10000 },
                new GithubFolders { FolderName = "10K-11K",FolderLink = "", MinItemID = 10001, MaxItemID = 11000 },
                new GithubFolders { FolderName = "11K-12K",FolderLink = "", MinItemID = 11001, MaxItemID = 12000 },
                new GithubFolders { FolderName = "12K-13K",FolderLink = "", MinItemID = 12001, MaxItemID = 13000 },
                new GithubFolders { FolderName = "13K-14K",FolderLink = "", MinItemID = 13001, MaxItemID = 14000 },
                new GithubFolders { FolderName = "14K-15K",FolderLink = "", MinItemID = 14001, MaxItemID = 15000 },
            };
            GithubVehicleFolders = new List<GithubFolders>
            {
                new GithubFolders { FolderName = "veh-0K-1K", FolderLink = "", MinItemID = 0, MaxItemID = 1000 },
            };
        }
    }
}
