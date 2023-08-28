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
        [YamlMember(Order = 4, Description = "How often can a player click on an ui button. ")]
        public float UIButtonDelay { get; set; }
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
        [YamlMember(Order = 13, Description = "The raw link of the github folder that contains the item image files.\nIf you want to use custom icons I recommend you to make a github repo and upload the images with the help of github desktop.\nYou can check my icon repo as example on https://github.com/TavstalDev/Icons/tree/master/Vanilla \nPS. The name of the folder can be anything, it's not used in the code.")]
        public List<GithubFolders> GithubItemFolders = new List<GithubFolders>();
        [YamlMember(Order = 14, Description = "Same just like item folders, just for vehicles\nWARNING: The default link is just an example!")]
        public List<GithubFolders> GithubVehicleFolders = new List<GithubFolders>();
        [YamlIgnore]
        public readonly string MessageIcon = "https://raw.githubusercontent.com/TavstalDev/Icons/master/Plugins/icon_plugin_tshop.png";
        [YamlIgnore]
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
