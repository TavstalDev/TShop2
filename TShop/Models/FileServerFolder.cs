using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Tavstal.TShop.Models
{
    public class FileServerFolder
    {
        [YamlMember(Order = 0)]
        public string DisplayName { get; set; }
        [YamlMember(Order = 1)]
        public string FolderLink { get; set; }
        [YamlMember(Order = 2)]
        public int MinItemId { get; set; }
        [YamlMember(Order = 3)]
        public int MaxItemId { get; set; }

        public FileServerFolder()
        {
            DisplayName = string.Empty;
            FolderLink = string.Empty;
        }

        public FileServerFolder(string displayName, string folderLink, int minItemID, int maxItemID)
        {
            DisplayName = displayName;
            FolderLink = folderLink;
            MinItemId = minItemID;
            MaxItemId = maxItemID;
        }
    }
}