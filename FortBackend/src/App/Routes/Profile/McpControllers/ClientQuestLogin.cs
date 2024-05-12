﻿using Amazon.Runtime.Internal.Transform;
using FortBackend.src.App.Routes.Profile.McpControllers.QueryResponses;
using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.Helpers;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortLibrary.MongoDB.Module;
using FortBackend.src.App.Utilities.Quests;
using FortLibrary.Dynamics;
using FortLibrary.EpicResponses.Profile;
using FortLibrary.EpicResponses.Profile.Purchases;
using FortLibrary.EpicResponses.Profile.Quests;
using Newtonsoft.Json;
using System.Collections.Generic;
using static FortBackend.src.App.Utilities.Helpers.Grabber;
using FortLibrary;
using System.Net.Http.Json;
using FortLibrary.EpicResponses.Profile.Query.Items;
using FortBackend.src.App.Utilities.Helpers.BattlepassManagement;
using FortBackend.src.App.Utilities.Constants;
using FortLibrary.EpicResponses.Profile.Query;
using MongoDB.Bson.IO;
using FortLibrary.Shop;
using Discord;

namespace FortBackend.src.App.Routes.Profile.McpControllers
{
    public class ClientQuestLogin
    {
        // This IS TEMP code ~ i'll rewrite mcp at some point (equiping is fine)
        public static async Task<Mcp> Init(string AccountId, string ProfileId, VersionClass Season, int RVN, ProfileCacheEntry profileCacheEntry)
        {
            string currentDate = DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            if (ProfileId == "athena" || ProfileId == "profile0")
            {
                // adds stats shocked
                 
                //pc_m0_p2 ~ solos
                //pc_m0_p10 ~ duos
                //pc_m0_p9 ~ squads
                foreach (var item in UpdateLeaderBoard.GetStatNames())
                {
                    if (!profileCacheEntry.StatsData.stats.Keys.Contains(item))
                    {
                        profileCacheEntry.StatsData.stats.Add(item, 0);
                    }
                }


                // Daily Quests WIP
                  
                if (profileCacheEntry.AccountData.commoncore.Seasons.Any(x => x.SeasonNumber == Season.Season))
                {
                    // Response Data ~ DONT CHANGE
                    List<object> MultiUpdates = new List<object>();
                    int BaseRev = profileCacheEntry.AccountData.athena.RVN;

                    if (Season.Season == 0)
                    {
                        if (BaseRev != RVN)
                        {
                            Mcp test = await AthenaResponse.Grab(AccountId, ProfileId, Season, RVN, profileCacheEntry);
                            MultiUpdates = test.profileChanges;
                        }

                        return new Mcp
                        {
                            profileRevision = profileCacheEntry.AccountData.athena.RVN,
                            profileId = ProfileId,
                            profileChangesBaseRevision = BaseRev,
                            profileChanges = MultiUpdates,
                            profileCommandRevision = profileCacheEntry.AccountData.athena.CommandRevision,
                            serverTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            responseVersion = 1
                        };
                    }


                    // Temp data
                    //int AddedXP = 0; // xp added to the users account
                    //int AddedLevel = 0; // level added to the users account

                    // Season Data
                    SeasonClass FoundSeason = profileCacheEntry.AccountData.commoncore?.Seasons.FirstOrDefault(x => x.SeasonNumber == Season.Season)!;

                        

                    if(FoundSeason.DailyQuests.Interval != currentDate)
                    {
                        FoundSeason.DailyQuests.Interval = currentDate;
                        FoundSeason.DailyQuests.Rerolls = 1; // give 1 reroll everyday
                             
                        MultiUpdates.Add(new MultiUpdateClassV2
                        {
                            changeType = "statModified",
                            name = "quest_manager",
                            value = new
                            {
                                dailyLoginInterval = currentDate,
                                dailyQuestRerolls = 1
                            }
                        });

                        if (!(FoundSeason.DailyQuests.Daily_Quests.Count > 3))
                        {
                            var DailyCount = 3 - FoundSeason.DailyQuests.Daily_Quests.Count;
                               
                            for (int i = 0; i < DailyCount; i++)
                            {
                                try
                                {
                                    DailyQuestsJson dailyQuests = await DailyQuestsManager.GrabRandomQuest(FoundSeason);

                                    if (!string.IsNullOrEmpty(dailyQuests.Name))
                                    {
                                        if(dailyQuests.Properties.Objectives.Count > 1)
                                        {
                                            Logger.Error("FEATURE NOT IMPLEMENTED");
                                        }
                                        else
                                        {
                                            DailyQuestsData dailyQuestsData = new DailyQuestsData
                                            {
                                                templateId = $"Quest:{dailyQuests.Name}",
                                                attributes = new DailyQuestsDataDB
                                                {
                                                    sent_new_notification = false,
                                                    ObjectiveState = new List<DailyQuestsObjectiveStates>
                                                    {
                                                        new DailyQuestsObjectiveStates
                                                        {
                                                            Name = $"completion_{dailyQuests.Properties.Objectives[0].BackendName}",
                                                            Value = 0
                                                        }
                                                    }
                                                },
                                                quantity = 1
                                            };

                                            // so skunked but should wokr
                                            MultiUpdates.Add(new MultiUpdateClass
                                            {
                                                changeType = "itemAdded",
                                                itemId = dailyQuests.Name,
                                                item = new
                                                {
                                                    templateId = $"Quest:{dailyQuests.Name}",
                                                    attributes = new Dictionary<string, object>
                                                    {
                                                        { "creation_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                        { "level", -1 },
                                                        { "item_seen", false },
                                                        { "playlists", new List<object>() },
                                                        { "sent_new_notification", true },
                                                        { "challenge_bundle_id", "" },
                                                        { "xp_reward_scalar", 1 },
                                                        { "challenge_linked_quest_given", "" },
                                                        { "quest_pool", "" },
                                                        { "quest_state", "Active" },
                                                        { "bucket", "" },
                                                        { "last_state_change_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                        { "challenge_linked_quest_parent", "" },
                                                        { "max_level_bonus", 0 },
                                                        { "xp", 0 },
                                                        { "quest_rarity", "uncommon" },
                                                        { "favorite", false },
                                                        { $"completion_{dailyQuests.Properties.Objectives[0].BackendName}", 0 }
                                                    },
                                                    quantity = 1
                                                }
                                            });

                                            FoundSeason.DailyQuests.Daily_Quests.Add(dailyQuests.Name, dailyQuestsData);
                                        }
                                            
                                    }
                                    else
                                    {
                                        Logger.Error("GRABBED EMPTY STUFF FOR DAUKY QUESTS?", "DAILY QUESTS!");
                                    }
                                }catch (Exception ex)
                                {
                                    Logger.Error(ex.Message, "DAILY QUESTS!");
                                }
                            }
                            // 
                        } // else how?
                    }

                    // CLAIMING QUEST SYSTEM
                    // Theres a whole different endpoint for claiming but in here we remove and give xp
                    // since i only worked on season 1 quests hasn't been implemented on other seasons until i work on season 2

                    foreach (var item in FoundSeason.DailyQuests.Daily_Quests)
                    {
                        if(item.Value != null) // shoudn't ever be null
                        {
                            DailyQuestsData DailyQuestsObject = item.Value;
                            if (DailyQuestsObject == null) continue;

                            if(DailyQuestsObject.attributes.quest_state == "Claimed")
                            {
                                // AddedXP += DailyQuestsObject.attributes.
                                DailyQuestsJson dailyQuestsJson = DailyQuestsManager.ReturnQuestInfo(item.Key, Season.Season);
                                if (string.IsNullOrEmpty(dailyQuestsJson.Name))
                                {
                                    FoundSeason.BookXP += dailyQuestsJson.Properties.SeasonXP;
                                    FoundSeason.DailyQuests.Daily_Quests.Remove(item.Key); // removes the quest
                                }
                            }
                        }
                    }

                    // END OF CLAIMING QUEST SYTEM

                    // START OF BATTLE PASS QUESTS SYSTEM

                    // Until i work on a season that changes this (season 10 omg) i need to redo this
                    if (WeeklyQuestManager.WeeklyQuestsSeasonAboveDictionary.TryGetValue($"Season{FoundSeason.SeasonNumber}", out List<WeeklyQuestsJson> WeeklyQuestsArray))
                    {
                        if (WeeklyQuestsArray.Count > 0)
                        {
                            List<string> ResponseIgIdrk = new List<string>();
                            var ResponseId = "";
                            foreach (var kvp in WeeklyQuestsArray)
                            {
                                ResponseIgIdrk.Add($"ChallengeBundle:{kvp.BundleId}");
                                ResponseId = $"ChallengeBundleSchedule:{kvp.BundleSchedule}";
                                //ResponseId = $"ChallengeBundleSchedule:{kvp.BundleSchedule}";
                                //ResponseIgIdrk.Add($"ChallengeBundle:{kvp.BundleId}");
                                //kvp.BundleId
                                List<string> grantedquestinstanceids = new List<string>();
                                foreach (var FreeBundles in kvp.FreeBundleObject)
                                {
                                    
                                    //grantedquestinstanceids.Add(FreeBundles.templateId);
                                    DailyQuestsData QuestData = FoundSeason.Quests.FirstOrDefault(e => e.Key == FreeBundles.templateId).Value;
                                    if (QuestData == null)
                                    {
                                        List<DailyQuestsObjectiveStates> QuestObjectStats = new List<DailyQuestsObjectiveStates>();

                                        foreach (WeeklyObjectsObjectives ObjectiveItems in FreeBundles.Objectives)
                                        {
                                            QuestObjectStats.Add(new DailyQuestsObjectiveStates
                                            {
                                                Name = $"completion_{ObjectiveItems.BackendName}",
                                                Value = 0,
                                                MaxValue = ObjectiveItems.Count
                                            });
                                        }

                                        FoundSeason.Quests.Add($"{FreeBundles.templateId}", new DailyQuestsData
                                        {
                                            templateId = $"{FreeBundles.templateId}",
                                            attributes = new DailyQuestsDataDB
                                            {
                                                challenge_bundle_id = $"ChallengeBundle:{kvp.BundleId}",
                                                sent_new_notification = false,
                                                ObjectiveState = QuestObjectStats,
                                            },
                                            quantity = 1
                                        });


                                        var ItemObjectResponse = new
                                        {
                                            templateId = $"{FreeBundles.templateId}",
                                            attributes = new Dictionary<string, object>
                                            {
                                                { "creation_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                { "level", -1 },
                                                { "item_seen", false },
                                                { "playlists", new List<object>() },
                                                { "sent_new_notification", true },
                                                { "challenge_bundle_id", $"ChallengeBundle:{kvp.BundleId}" },
                                                { "xp_reward_scalar", 1 },
                                                { "challenge_linked_quest_given", "" },
                                                { "quest_pool", "" },
                                                { "quest_state", "Active" },
                                                { "bucket", "" },
                                                { "last_state_change_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                { "challenge_linked_quest_parent", "" },
                                                { "max_level_bonus", 0 },
                                                { "xp", 0 },
                                                { "quest_rarity", "uncommon" },
                                                { "favorite", false },
                                                // { $"completion_{dailyQuests.Properties.Objectives[0].BackendName}", 0 }
                                            },
                                            quantity = 1
                                        };

                                        foreach (DailyQuestsObjectiveStates yklist in QuestObjectStats)
                                        {
                                            ItemObjectResponse.attributes.Add(yklist.Name, yklist.Value);
                                        }

                                        MultiUpdates.Add(new MultiUpdateClass
                                        {
                                            changeType = "itemAdded",
                                            itemId = $"{FreeBundles.templateId}",
                                            item = ItemObjectResponse
                                        });
                                    }
                                }

                                if (FoundSeason.BookPurchased)
                                {
                                    foreach (var PaidBundles in kvp.PaidBundleObject)
                                    {
                                        // grantedquestinstanceids.Add(PaidBundles.templateId);

                                        DailyQuestsData QuestData = FoundSeason.Quests.FirstOrDefault(e => e.Key == PaidBundles.templateId).Value;
                                        if (QuestData == null)
                                        {
                                            List<DailyQuestsObjectiveStates> QuestObjectStats = new List<DailyQuestsObjectiveStates>();

                                            foreach (WeeklyObjectsObjectives ObjectiveItems in PaidBundles.Objectives)
                                            {
                                                QuestObjectStats.Add(new DailyQuestsObjectiveStates
                                                {
                                                    Name = $"completion_{ObjectiveItems.BackendName}",
                                                    Value = 0,
                                                    MaxValue = ObjectiveItems.Count
                                                });
                                            }

                                            FoundSeason.Quests.Add($"{PaidBundles.templateId}", new DailyQuestsData
                                            {
                                                templateId = $"{PaidBundles.templateId}",
                                                attributes = new DailyQuestsDataDB
                                                {
                                                    challenge_bundle_id = $"ChallengeBundle:{kvp.BundleId}",
                                                    sent_new_notification = false,
                                                    ObjectiveState = QuestObjectStats
                                                },
                                                quantity = 1
                                            });


                                            var ItemObjectResponse = new
                                            {
                                                templateId = $"{PaidBundles.templateId}",
                                                attributes = new Dictionary<string, object>
                                                {
                                                    { "creation_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                    { "level", -1 },
                                                    { "item_seen", false },
                                                    { "playlists", new List<object>() },
                                                    { "sent_new_notification", false },
                                                    { "challenge_bundle_id", $"ChallengeBundle:{kvp.BundleId}" },
                                                    { "xp_reward_scalar", 1 },
                                                    { "challenge_linked_quest_given", "" },
                                                    { "quest_pool", "" },
                                                    { "quest_state", "Active" },
                                                    { "bucket", "" },
                                                    { "last_state_change_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                                    { "challenge_linked_quest_parent", "" },
                                                    { "max_level_bonus", 0 },
                                                    { "xp", 0 },
                                                    { "quest_rarity", "uncommon" },
                                                    { "favorite", false },
                                                    // { $"completion_{dailyQuests.Properties.Objectives[0].BackendName}", 0 }
                                                },
                                                quantity = 1
                                            };

                                            foreach (DailyQuestsObjectiveStates yklist in QuestObjectStats)
                                            {
                                                ItemObjectResponse.attributes.Add(yklist.Name, yklist.Value);
                                            }

                                            MultiUpdates.Add(new MultiUpdateClass
                                            {
                                                changeType = "itemAdded",
                                                itemId = $"{PaidBundles.templateId}",
                                                item = ItemObjectResponse
                                            });
                                        }

                                    }
                                }
                            }

                            var ItemTestObjectResponse = new
                            {
                                templateId = ResponseId,
                                attributes = new Dictionary<string, object>
                                {
                                    { "unlock_epoch", DateTime.MinValue.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                    { "max_level_bonus", 0 },
                                    { "level", 0 },
                                    { "item_seen", true },
                                    { "xp", 0 },
                                    { "favorite", false },
                                    { "granted_bundles", ResponseIgIdrk.ToArray() }
                                },
                                quantity = 1,
                            };

                            MultiUpdates.Add(new
                            {
                                changeType = "itemRemoved",
                                itemId = ResponseId,
                            });


                            MultiUpdates.Add(new MultiUpdateClass
                            {
                                changeType = "itemAdded",
                                itemId = ResponseId,
                                item = ItemTestObjectResponse
                            });

                            // ProfileChange.Profile.items.Add(ResponseId, AthenaItemDynamicData);


                        }
                    }

                    // END OF BATTLE PASS QUESTS SYSTEM

                    // LEVEL SYSTEM & XP

                    var SeasonXPFolder = Path.Combine(PathConstants.BaseDir, $"Json\\Season\\Season{Season.Season}\\SeasonXP.json");
                    var SeasonBattleStarsFolder = Path.Combine(PathConstants.BaseDir, $"Json\\Season\\Season{Season.Season}\\SeasonBP.json");
                      
                    int BookLevelOG = FoundSeason.BookLevel;
                    bool NeedItems = false;
                    // unsupported seasons will not go though.. so it doesn't break stuff
                    // I will try to add it for most seasons when i'm not doing other things
                    if (File.Exists(SeasonXPFolder))
                    {
                        //FoundSeason
                        (FoundSeason, NeedItems) = await LevelUpdater.Init(Season.Season, FoundSeason, NeedItems);

                        var currencyItem = profileCacheEntry.AccountData.commoncore.Items["Currency"];
                        if (currencyItem != null)
                        {


                            // BATTLE PASS SYSTEM

                            List<Battlepass> FreeTier = BattlepassManager.FreeBattlePassItems.FirstOrDefault(e => e.Key == Season.Season).Value;

                            if(FreeTier != null)
                            {
                                if (FreeTier.Count > 0)
                                {
                                    if (Season.Season > 1)
                                    {
                                        List<Battlepass> PaidTier = BattlepassManager.PaidBattlePassItems.FirstOrDefault(e => e.Key == Season.Season).Value;

                                        if(PaidTier != null)
                                        {
                                            if (PaidTier.Count > 0)
                                            {
                                                foreach (var BattlePass in FreeTier)
                                                {
                                                    if (!NeedItems) break;
                                                    //if (BookLevelOG <= BattlePass.Level) continue;
                                                    if (BattlePass.Level > FoundSeason.BookLevel) break;

                                                    List<NotificationsItemsClassOG> unlessfunc;
                                                    (profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems, unlessfunc) = await BattlePassRewards.Init(BattlePass.Rewards, profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems);
                                                }

                                                foreach (var BattlePass in PaidTier)
                                                {
                                                    if (!NeedItems) break;
                                                    //if (BookLevelOG <= BattlePass.Level) continue;
                                                    if (BattlePass.Level > FoundSeason.BookLevel) break;

                                                    List<NotificationsItemsClassOG> unlessfunc;
                                                    (profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems, unlessfunc) = await BattlePassRewards.Init(BattlePass.Rewards, profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems);
                                                }
                                            }
                                            else
                                            {
                                                Logger.Error("PaidTier file is null [] ? battlepass tiering disabled");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log("Unsupported season");
                                        }
                                    }
                                    else
                                    {
                                        // season 1 only free tier
                                        foreach (var BattlePass in FreeTier)
                                        {
                                            if (!NeedItems) break;
                                            if (BookLevelOG <= BattlePass.Level) continue;
                                            if (BattlePass.Level > FoundSeason.Level) break;

                                            List<NotificationsItemsClassOG> unlessfunc;
                                            (profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems, unlessfunc) = await BattlePassRewards.Init(BattlePass.Rewards, profileCacheEntry, FoundSeason, MultiUpdates, currencyItem, NeedItems);
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.Error("FreeTier file is null [] ? battlepass tiering disabled");
                                }
                            }
                            else
                            {
                                Logger.Error($"This season is *NOT* supported ~ {Season.Season}", "ClientQuestLogin");
                            }
                            
                            
                                    

                            MultiUpdates.Add(new
                            {
                                changeType = "itemQuantityChanged",
                                name = "Currency",
                                value = currencyItem.quantity
                            });
                        }


                        List<SeasonXP> SeasonXpIg = BattlepassManager.SeasonBattlePassXPItems.FirstOrDefault(e => e.Key == FoundSeason.SeasonNumber).Value;
                        int BeforeLevelXP = SeasonXpIg.FirstOrDefault(e => e.Level == (FoundSeason.BookLevel)).XpTotal;
                        int CurrentLevelXP = SeasonXpIg.FirstOrDefault(e => e.XpToNextLevel >= (BeforeLevelXP + FoundSeason.SeasonXP)).XpTotal + FoundSeason.SeasonXP;

                        foreach (var Quests in FoundSeason.Quests)
                        {
                            int TempIntNum = 0;
                            bool NeedUpdating = false;
                            foreach (var objectiveState in Quests.Value.attributes.ObjectiveState)
                            {
                                if (objectiveState.Name.Contains("xp"))
                                {
                                    
                                    DailyQuestsObjectiveStates ValueIsSoProper = FoundSeason.Quests[Quests.Key].attributes.ObjectiveState[TempIntNum];
                                    if (CurrentLevelXP >= objectiveState.Value)
                                    {
                                        NeedUpdating = true;
                                        FoundSeason.Quests[Quests.Key].attributes.ObjectiveState[TempIntNum].Value = ValueIsSoProper.MaxValue;
                                    }
                                    else
                                    {
                                        if(FoundSeason.Quests[Quests.Key].attributes.ObjectiveState[TempIntNum].Value != CurrentLevelXP)
                                        {
                                            NeedUpdating = true;
                                        }

                                        FoundSeason.Quests[Quests.Key].attributes.ObjectiveState[TempIntNum].Value = CurrentLevelXP;
                                    }

                                    FoundSeason.Quests[Quests.Key].attributes.creation_time = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                                    //DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

                                }

                                TempIntNum += 1;
                            }

                            // IF THERE IS A BETTER METHOD PLEASE PULL REQUEST OR TELL ME (or ill just look in the future)
                            if (NeedUpdating)
                            {
                                var AthenaItemChallengeBundle = new
                                {
                                    templateId = Quests.Value.templateId,
                                    attributes = new Dictionary<string, object>
                                    {
                                        { "creation_time", Quests.Value.attributes.creation_time },
                                        { "level", -1 },
                                        { "item_seen", false },
                                        { "playlists", new List<object>() },
                                        { "sent_new_notification", true },
                                        { "challenge_bundle_id", Quests.Value.attributes.challenge_bundle_id },
                                        { "xp_reward_scalar", 1 },
                                        { "challenge_linked_quest_given", "" },
                                        { "quest_pool", "" },
                                        { "quest_state", "Active" },
                                        { "bucket", "" },
                                        { "last_state_change_time", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                                        { "challenge_linked_quest_parent", "" },
                                        { "max_level_bonus", 0 },
                                        { "xp", 0 },
                                        { "quest_rarity", "uncommon" },
                                        { "favorite", false },
                                    },
                                    quantity = 1,
                                };

                                foreach (DailyQuestsObjectiveStates yklist in FoundSeason.Quests[Quests.Key].attributes.ObjectiveState)
                                {
                                    AthenaItemChallengeBundle.attributes.Add(yklist.Name, yklist.Value);
                                }

                                MultiUpdates.Add(new
                                {
                                    changeType = "itemRemoved",
                                    itemId = Quests.Value.templateId,
                                });

                                MultiUpdates.Add(new MultiUpdateClass
                                {
                                    changeType = "itemAdded",
                                    itemId = Quests.Value.templateId,
                                    item = AthenaItemChallengeBundle
                                });
                            }
                          
                        }




                        // END OF BATTLE PASS SYSTEM

                        /// TO-DO ONLY USE THIS IF THE STAT IS CHANGING

                        // need to check if they need to actually need to be updated
                        MultiUpdates.Add(new
                        {
                            changeType = "statModified",
                            name = "level",
                            value = FoundSeason.Level
                        });

                        MultiUpdates.Add(new
                        {
                            changeType = "statModified",
                            name = "book_level",
                            value = FoundSeason.BookLevel
                        });

                        //book_xp


                        MultiUpdates.Add(new
                        {
                            changeType = "statModified",
                            name = "book_xp",
                            value = FoundSeason.BookXP
                        });

                        MultiUpdates.Add(new
                        {
                            changeType = "statModified",
                            name = "xp",
                            value = FoundSeason.SeasonXP
                        });

                       
                    }

                    // END OF LEVEL SYTEM & XP

                    if (MultiUpdates.Count > 0)
                    {
                        profileCacheEntry.LastUpdated = DateTime.Now;
                        profileCacheEntry.AccountData.athena.RVN += 1;
                        profileCacheEntry.AccountData.athena.CommandRevision += 1;
                    }

                    if (BaseRev != RVN)
                    {
                        Mcp test = await AthenaResponse.Grab(AccountId, ProfileId, Season, RVN, profileCacheEntry);
                        MultiUpdates = test.profileChanges;
                    }

                    //Console.WriteLine(JsonConvert.SerializeObject(new Mcp
                    //{
                    //    profileRevision = profileCacheEntry.AccountData.athena.RVN,
                    //    profileId = ProfileId,
                    //    profileChangesBaseRevision = BaseRev,
                    //    profileChanges = MultiUpdates,
                    //    profileCommandRevision = profileCacheEntry.AccountData.athena.CommandRevision,
                    //    serverTime = DateTime.Parse(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                    //    responseVersion = 1
                    //}));

                    string mcpJson = Newtonsoft.Json.JsonConvert.SerializeObject(new Mcp
                    {
                        profileRevision = profileCacheEntry.AccountData.athena.RVN,
                        profileId = ProfileId,
                        profileChangesBaseRevision = BaseRev,
                        profileChanges = MultiUpdates,
                        profileCommandRevision = profileCacheEntry.AccountData.athena.CommandRevision,
                        serverTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        responseVersion = 1
                    }, Formatting.Indented);
                    Console.WriteLine(mcpJson);


                    return new Mcp
                    {
                        profileRevision = profileCacheEntry.AccountData.athena.RVN,
                        profileId = ProfileId,
                        profileChangesBaseRevision = BaseRev,
                        profileChanges = MultiUpdates,
                        profileCommandRevision = profileCacheEntry.AccountData.athena.CommandRevision,
                        serverTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        responseVersion = 1
                    };
                }
                //var DailyQuests =
                Mcp response = await AthenaResponse.Grab(AccountId, ProfileId, Season, RVN, profileCacheEntry);

                return response;
                ////Console.WriteLine("fas");
                //List<AthenaItem> contentconfig = JsonConvert.DeserializeObject<List<AthenaItem>>(jsonData); //dynamicbackgrounds.news
                ////Console.WriteLine("GR");

                //ProfileChange test1 = response.profileChanges[0] as ProfileChange;
                //foreach (AthenaItem test in contentconfig)
                //{
                //    //Console.WriteLine("TET");
                //    test1.Profile.items.Add(test.templateId, test);
                //}
                //return response;
            }
            else
            {
                Logger.Error("ClientQuestLogin might not function well");
            }
       

            return new Mcp();
        }
    }
}
