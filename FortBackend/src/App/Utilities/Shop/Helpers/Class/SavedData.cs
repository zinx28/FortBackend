﻿using FortBackend.src.App.Utilities.Shop.Helpers.Class;
using MongoDB.Driver;

namespace FortBackend.src.App.Utilities.Shop.Helpers.Data
{
    public class SavedData
    {
        public List<ItemsSaved> Weekly { get; set; } = new List<ItemsSaved>();
        public List<ItemsSaved> Daily { get; set; } = new List<ItemsSaved>();
    }

    public class ItemsSaved
    {
        public string id { get; set; } = string.Empty;
        public string item { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;    
        public string description { get; set; } = string.Empty;
        public List<Item> items { get; set; } = new List<Item>();
        public int price { get; set; } = -1;
        public int singleprice { get; set; } = -1;
        public List<Variants> variants { get; set; } = new List<Variants>();

        public string BundlePath { get; set; } = string.Empty;
        public string rarity { get; set; } = string.Empty;
        public string type = string.Empty;
        public string[] categories { get; set; } = new string[0];
    }
}
