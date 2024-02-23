﻿namespace FortBackend.src.App.Utilities.Classes.EpicResponses.Profile.Purchases
{
    public class MultiUpdateClass
    {
        public string changeType { get; set; } = string.Empty;
        public string itemId { get; set; } = string.Empty;
        public object item { get; set; }
    }

    public class ApplyProfileChangesClass
    {
        public string changeType { get; set; } = string.Empty;
        public string itemId { get; set; } = string.Empty;
        public int quantity { get; set; } = 0;
    }

    public class NotificationsItemsClass
    {
        public string itemType { get; set; } = string.Empty;
        public string itemGuid { get; set; } = string.Empty;
        public string itemProfile { get; set; } = string.Empty;
        public int quantity { get; set; } = 1;
    }
    /*
     * 
     * itemType = ShopContent.id,
                    itemGuid = ShopContent.item,
                    itemProfile = "athena",
                    quantity = 1

    */
}