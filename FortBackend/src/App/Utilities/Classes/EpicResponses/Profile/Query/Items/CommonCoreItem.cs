﻿using Newtonsoft.Json;

namespace FortBackend.src.App.Utilities.Classes.EpicResponses.Profile.Query.Items
{
    public class CommonCoreItem
    {
        [JsonProperty("templateId")]
        public string templateId { get; set; } = string.Empty;
        [JsonProperty("attributes")]
        public CommonCoreItemAttributes attributes { get; set; } = new CommonCoreItemAttributes();
        [JsonProperty("quantity")]
        public int quantity { get; set; } = 1;
    }

    public class CommonCoreItemAttributes
    {
        [JsonProperty("platform")]
        public string platform { get; set; } = "EpicPC";

    }
 }