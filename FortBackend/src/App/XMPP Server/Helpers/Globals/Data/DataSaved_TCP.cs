﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace FortBackend.src.App.XMPP_Server.Helpers.Globals.Data
{
    public class DataSaved_TCP
    {
        public bool DidUserLoginNotSure = false;
        public string AccountId = string.Empty;
        public string DisplayName = string.Empty;
        public string receivedMessage = ""; // so skunky but works fine
        public bool clientExists = false;
        public string Token = string.Empty;
        public string JID = string.Empty;
        public string Resource = string.Empty;
        public string[] Rooms = new string[] { };

        public static ConcurrentDictionary<TcpClient, string> clientData = new ConcurrentDictionary<TcpClient, string>();
        public static ConcurrentDictionary<string, TcpClient> connectedClients = new ConcurrentDictionary<string, TcpClient>();
    }
}
