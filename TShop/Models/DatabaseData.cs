using System;
using Tavstal.TLibrary.Models.Config;
using YamlDotNet.Serialization;

namespace Tavstal.TShop.Models
{
    [Serializable]
    public class DatabaseData : DatabaseConfigBase
    {
        [YamlMember(Order = 7, Description = "Database table prefix, default is 'tshop_'.")]
        public string TablePrefix { get; set; }

        public DatabaseData()
        {
            TablePrefix = "tshop_";
        }
        
        public DatabaseData(string tablePrefix)
        {
            TablePrefix = tablePrefix;
        }
    }
}