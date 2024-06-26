﻿namespace FortLibrary.EpicResponses.Storefront
{
    public class Catalog
    {
        public int refreshIntervalHrs { get; set; } = 1;
        public int dailyPurchaseHrs { get; set; } = 24;
        public string expiration { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        public List<dynamic> storefronts  { get; set;} = new List<dynamic>();

    }

    public class StoreFront
    {
        public string name { get; set; } = string.Empty;
        public List<dynamic> catalogEntries { get; set; } = new List<dynamic>();
    }
}
