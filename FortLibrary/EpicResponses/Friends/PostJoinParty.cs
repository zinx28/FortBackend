﻿
namespace FortLibrary.EpicResponses.Friends
{
    public class PostJoinParty
    {
        public Connection connection { get; set; } = new Connection();
        public Dictionary<string, object> meta { get; set; } = new Dictionary<string, object>();
    }

    public class Connection
    {
        public string id { get; set; } = string.Empty;
        public Dictionary<string, object> meta { get; set; } = new Dictionary<string, object>();
        public bool yield_leadership { get; set; } = false;
    }
}
