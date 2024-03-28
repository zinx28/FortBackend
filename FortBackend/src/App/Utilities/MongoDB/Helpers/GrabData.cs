﻿using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.MongoDB.Module;
using Newtonsoft.Json;

namespace FortBackend.src.App.Utilities.MongoDB.Helpers
{
    public class GrabData
    {
        public static async Task<ProfileCacheEntry> Profile(string AccountId, bool Auth = false, string AuthToken = "N")
        {
            try
            {
                Console.WriteLine("GRABBING DATA!");
                var GrabData = default(KeyValuePair<string, ProfileCacheEntry>);
                if (CacheMiddleware.GlobalCacheProfiles.Any())
                {
                    Console.WriteLine("TEST");
                    Console.WriteLine(JsonConvert.SerializeObject(CacheMiddleware.GlobalCacheProfiles));
                    if (Auth)
                    {
                        GrabData = CacheMiddleware.GlobalCacheProfiles.FirstOrDefault(e => e.Value.UserData.accesstoken == AuthToken);
                    }
                    else
                    {
                        GrabData = CacheMiddleware.GlobalCacheProfiles.FirstOrDefault(e => e.Key == AccountId);
                    }
                    Console.WriteLine(AccountId);
                    Console.WriteLine("e  " + GrabData);
                }
                if (GrabData.Value == null)
                {
                    Console.WriteLine("why");
                    var UserData = await Handlers.FindOne<User>(Auth ? "accesstoken" : "accountId", Auth ? AuthToken : AccountId);
                    if (UserData != "Error")
                    {
                        User UserDataParsed = JsonConvert.DeserializeObject<User[]>(UserData)![0];
                        if (UserDataParsed != null)
                        {
                            var AccountData = await Handlers.FindOne<Account>("accountId", UserDataParsed.AccountId);
                            var FriendsData = await Handlers.FindOne<UserFriends>("accountId", UserDataParsed.AccountId);
                            Console.WriteLine("GRABBING DATA");

                            if (AccountData != "Error" && FriendsData != "Error")
                            {
                                Account AccountDataParsed = JsonConvert.DeserializeObject<Account[]>(AccountData)![0];
                                UserFriends FriendsDataParsed = JsonConvert.DeserializeObject<UserFriends[]>(FriendsData)![0];
                                Console.WriteLine("GRABBING DATA3");
                                if (AccountDataParsed != null && UserDataParsed != null && FriendsDataParsed != null)
                                {
                                    Console.WriteLine("GRABBING DATA4");

                                    ProfileCacheEntry ProfileCacheEntry = new ProfileCacheEntry
                                    {
                                        AccountId = UserDataParsed.AccountId,
                                        AccountData = AccountDataParsed,
                                        UserData = UserDataParsed,
                                        UserFriends = FriendsDataParsed,
                                        LastUpdated = DateTime.Now,
                                    };
                                    
                                    CacheMiddleware.GlobalCacheProfiles.Add(UserDataParsed.AccountId, ProfileCacheEntry);

                                    return ProfileCacheEntry;
                                }
                            }
                        }
                    }else
                    {
                        Console.WriteLine("Bypass? Invaild Account");
                    }
                }
                else
                {
                    if (GrabData.Value != null)
                    {
                        Console.WriteLine("RETURNING CORRET DATA <3");
                        return GrabData.Value;
                    }
                    Console.WriteLine("what? how");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "GrabData");
            }
           
            return new ProfileCacheEntry();
        }
    }
}
