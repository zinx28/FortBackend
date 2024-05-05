﻿using FortLibrary.MongoDB.Modules;
using FortLibrary;
using MongoDB.Driver;
using FortLibrary.EpicResponses.Oauth;
using FortBackend.src.App.Utilities.ADMIN;

namespace FortBackend.src.App.Utilities.MongoDB.Helpers
{
    public class GrabAdminData
    {
        public static List<AdminProfileCacheEntry> AdminUsers { get; set; } = new List<AdminProfileCacheEntry>();
        // theres better ways and this is the most retarded way i think

        public static async Task<bool> EditAdmin(AdminInfo adminInfo)
        {
            try
            {
                if (MongoDBStart.Database != null)
                {
                    IMongoCollection<AdminInfo> AdminInfocollection = MongoDBStart.Database.GetCollection<AdminInfo>("AdminInfo");

                    var filter = Builders<AdminInfo>.Filter.Eq(x => x.DiscordId, adminInfo.DiscordId);

                 
                    AdminInfocollection.ReplaceOne(filter, adminInfo);

                    AdminProfileCacheEntry checkvalue = AdminUsers.FirstOrDefault(e => e.adminInfo.AccountId == adminInfo.AccountId);
                    if (checkvalue != null)
                    {
                        AdminData cachedAdminData = Saved.Saved.CachedAdminData.Data?.FirstOrDefault(e => e.AdminUser == checkvalue.profileCacheEntry.UserData.Email);
                        if(cachedAdminData != null)
                        {
                            Console.WriteLine("CHANGING ROLES");
                            cachedAdminData.RoleId = adminInfo.Role;
                        }

                        checkvalue.adminInfo = adminInfo;
                    }

                    //var existingAdmin = await AdminInfocollection.Find(x => x.DiscordId == adminInfo.DiscordId).FirstOrDefaultAsync();
                    Logger.Log($"Admin with Discord ID {adminInfo.DiscordId} updated successfully.", "ADMIN ADD PANEL");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "ADMIN ADD PANEL");
            }

            return false;
        }

        public static async Task<bool> AddAdmin(string DiscordIds)
        {
            try
            {
                if (MongoDBStart.Database != null)
                {
                    IMongoCollection<AdminInfo> AdminInfocollection = MongoDBStart.Database.GetCollection<AdminInfo>("AdminInfo");

                    var existingAdmin = await AdminInfocollection.Find(x => x.DiscordId == DiscordIds).FirstOrDefaultAsync();

                    if(existingAdmin == null)
                    {
                        ProfileCacheEntry profileCacheEntry = await GrabData.ProfileDiscord(DiscordIds);
                        if (!string.IsNullOrEmpty(profileCacheEntry.AccountId))
                        {
                            AdminInfo newAdmin = new AdminInfo
                            {
                                AccountId = profileCacheEntry.UserData.AccountId,
                                DiscordId = profileCacheEntry.UserData.DiscordId
                            };
                            await AdminInfocollection.InsertOneAsync(newAdmin);

                            await GrabAdminData.GrabAllAdmin();

                            Logger.Log($"Admin with Discord ID {DiscordIds} added successfully.", "ADMIN ADD PANEL");

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "ADMIN ADD PANEL");
            }

            return false;
        }

        public static async Task GrabAllAdmin()
        {
            //AdminUsers
            try
            {
                Logger.Log("Grabbing all admins", "ADMIN PANEL");
                AdminUsers.Clear();
                if (MongoDBStart.Database != null)
                {
                    
                    IMongoCollection<AdminInfo> AdminInfocollection = MongoDBStart.Database.GetCollection<AdminInfo>("AdminInfo");
                 
                    List<AdminInfo> adminInfoList = await AdminInfocollection.Find(_ => true).ToListAsync();
                 
                    foreach (AdminInfo adminInfo in adminInfoList)
                    {
                        ProfileCacheEntry profileCacheEntry = await GrabData.Profile(adminInfo.AccountId);
                        if (!string.IsNullOrEmpty(profileCacheEntry.AccountId))
                        {
                            Logger.Log(adminInfo.AccountId);
                            AdminUsers.Add(new AdminProfileCacheEntry() {
                                profileCacheEntry = profileCacheEntry,
                                adminInfo = adminInfo
                            });
                        }
                    }
                    //var UserData = await Handlers.
                    //var UserData = await Handlers.FindOne<User>("email", UsersEmail);


                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "ADMIN PANEL");
            }
        }
    }
}
