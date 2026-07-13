using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Models;
using Tavstal.TLibrary.Models.Config;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TShop.Models;
using YamlDotNet.Serialization;

#pragma warning disable CS8618 // The fields are initialized in LoadDefaults() method and set by json serializer.

namespace Tavstal.TShop
{
    [Serializable]
    public class ShopConfiguration : YamlConfiguration
    {
        [YamlMember(Order = 3, Description = "Database configuration for the plugin.")]
        public DatabaseData Database { get; set; }
        [YamlMember(Order = 4, Description = "Delay in seconds for UI button clicks, default is 0.45.")]
        public float UiButtonDelay { get; set; }
        [YamlMember(Order = 5, Description = "Should the plugin use item quality for pricing, default is true.")]
        public bool UseQuality { get; set; }
        [YamlMember(Order = 6, Description = "Should the plugin use ExpEconomy for pricing, default is false.")]
        public bool ExpMode { get; set; }
        [YamlMember(Order = 7, Description = "Should the plugin enable discounts, default is true.")]
        public bool EnableDiscounts { get; set; }
        [YamlMember(Order = 8, Description = "Minimum discount percentage, default is 5.")]
        public float MinDiscount { get; set; }
        [YamlMember(Order = 9, Description = "Maximum discount percentage, default is 10.")]
        public float MaxDiscount{ get; set; }
        [YamlMember(Order = 10, Description = "Number of items to be discounted, default is 10.")]
        public int ItemCountToDiscount { get; set; }
        [YamlMember(Order = 11,  Description = "Number of items to be discounted, default is 50.")]
        public int VehicleCountToDiscount { get; set; }
        [YamlMember(Order = 12, Description = "Interval in seconds for discounts to be applied, default is 1800.")]
        public int DiscountInterval { get; set; }
        [YamlMember(Order = 13, Description = "Default product icon URL, used when no specific icon is available.")]
        public string DefaultProductIconUrl { get; set; }
        [YamlMember(Order = 14, Description = "Modifier for vehicle spawn position, default is (0, 5, 5).")]
        public SerializableVector3 VehicleSpawnModifier { get; set; }
        [YamlMember(Alias = "ItemFolders", Order = 15, Description = "List of folders for item images, each folder has a name, URL, and ID range.")]
        public List<FileServerFolder> ItemFolders { get; set; }
        [YamlMember(Alias = "VehicleFolders", Order = 16, Description = "List of folders for vehicle images, each folder has a name, URL, and ID range.")]
        public List<FileServerFolder> VehicleFolders { get; set; }
        [YamlIgnore]
        public readonly string MessageIcon = "https://raw.githubusercontent.com/TavstalDev/TShop2/master/assets/icon.png";
        [YamlIgnore]
        public readonly ushort EffectID = 8818;

        public override void LoadDefaults()
        {
            LogLevel = ELogLevel.INFO;
            Locale = "en";
            DownloadLocalePacks = true;
            Database = new DatabaseData("tshop_");
            UiButtonDelay = 0.45f;
            UseQuality = true;
            ExpMode = false;
            EnableDiscounts = true;
            MinDiscount = 5;
            MaxDiscount = 10;
            ItemCountToDiscount = 10;
            VehicleCountToDiscount = 5;
            DiscountInterval = 1800;
            DefaultProductIconUrl = "https://raw.githubusercontent.com/TavstalDev/TShop2/master/assets/noimage.png";
            VehicleSpawnModifier = new SerializableVector3(0, 5, 5);
            ItemFolders = new List<FileServerFolder>
            {
                new FileServerFolder("0-2000", "https://raw.githubusercontent.com/SilksPlugins/UnturnedImages/refs/heads/images/vanilla/items/", 0, 2000),
                new FileServerFolder("my-server", "https://api.myserver.com/items/", 0, 60000)
            };
            VehicleFolders = new List<FileServerFolder>
            {
                new FileServerFolder("0-2000", "https://raw.githubusercontent.com/SilksPlugins/UnturnedImages/refs/heads/images/vanilla/vehicles/", 0, 1000),
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
