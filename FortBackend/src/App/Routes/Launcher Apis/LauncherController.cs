﻿using FortBackend.src.App.Utilities.Helpers.Encoders;
using FortBackend.src.App.Utilities.Helpers.UserManagement;
using FortBackend.src.App.Utilities.Helpers;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortBackend.src.App.Utilities.MongoDB.Module;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using static FortBackend.src.App.Utilities.Classes.DiscordAuth;
using FortBackend.src.App.Utilities.Saved;
using Newtonsoft.Json;
using FortBackend.src.App.Utilities;

namespace FortBackend.src.App.Routes.LUNA_CUSTOMS
{
    [ApiController]
    [Route("launcher/api/v1")]
    public class TempController : ControllerBase
    {
        private IMongoDatabase _database;
        public TempController(IMongoDatabase database)
        {
            _database = database;
        }


        [HttpGet("callback")]
        public async Task<IActionResult> CallBack([FromQuery] string code)
        {
            try
            {
                Config config = Saved.DeserializeConfig;

                if (string.IsNullOrEmpty(config.ApplicationClientID) || string.IsNullOrEmpty(config.ApplicationURI) || string.IsNullOrEmpty(config.ApplicationSecret))
                {
                    throw new Exception("BLANK APPLICATION INFO");
                    return Ok(new { test = "Blank Application Info" });
                }
                var Client = new HttpClient();
                var formData = new Dictionary<string, string>()
                {
                    { "client_id", config.ApplicationClientID },
                    { "client_secret", config.ApplicationSecret },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", config.ApplicationURI }
                };
                var content = new FormUrlEncodedContent(formData);
                var response = await Client.PostAsync("https://discord.com/api/oauth2/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                if (responseData == null)
                {
                    return Ok(new { test = "Null!" });
                }

                if (responseData.TryGetValue("access_token", out var accessToken))
                {
                    var client2 = new HttpClient();
                    client2.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    var response2 = await client2.GetAsync("https://discord.com/api/users/@me/guilds");
                    var responseContent2 = await response2.Content.ReadAsStringAsync();

                    List<Server> responseData2 = JsonConvert.DeserializeObject<List<Server>>(responseContent2)!;
                    bool IsInServer = false;
                    if (responseData2 != null && responseData2.Count > 0)
                    {
                        foreach (Server item in responseData2)
                        {
                            //Console.WriteLine(item.id);
                            if (item.id == Saved.DeserializeConfig.ServerID)
                            {
                                IsInServer = true;
                            }
                        }
                    }
                    HttpContext httpContext = HttpContext;
                    if (httpContext == null)
                    {
                        return Ok(new { test = "Context is null" });
                    }

                    if (IsInServer)
                    {
                        // Only call this api if they are in the server
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        response = await client.GetAsync("https://discord.com/api/users/@me");
                        responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent == null)
                        {
                            return Ok(new { test = "Server Sided Error" });
                        }

                        UserInfo responseData1 = JsonConvert.DeserializeObject<UserInfo>(responseContent)!;

                        if (responseData1 == null)
                        {
                            return Ok(new { test = "Server Sided Error -> 2" });
                        }
                        var username = responseData1.username;
                        var GlobalName = responseData1.global_name;
                        var id = responseData1.id;
                        var email = responseData1.email;

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(id))
                        {
                            return Ok(new { test = "why is the response wrong" });
                        }

                        var FindDiscordID = await Handlers.FindOne<User>("DiscordId", id);
                        if (FindDiscordID != "Error")
                        {
                            Console.WriteLine("li");
                            string NewAccessToken = JWT.GenerateRandomJwtToken(15, "FortBackendIsSoCoolLetMeNutAllOverYou!@!@!@!@!");

                            var UpdateResponse = await Handlers.UpdateOne<User>("DiscordId", id, new Dictionary<string, object>()
                            {
                                { "accesstoken", NewAccessToken }
                            });

                            var Ip = "";
                            if (Saved.DeserializeConfig.Cloudflare)
                            {
                                Ip = httpContext.Request.Headers["CF-Connecting-IP"];
                            }
                            else
                            {
                                Ip = httpContext.Connection.RemoteIpAddress!.ToString();
                            }

                            if (UpdateResponse != "Error")
                            {
                                User UserData = JsonConvert.DeserializeObject<User[]>(FindDiscordID)?[0]!;
                                if (UserData != null)
                                {
                                    if (UserData.banned)
                                    {
                                        return Ok(new { test = "Banned" });
                                    }

                                    Console.WriteLine("TEST");
                                    string[] UserIp = new string[] { Ip };

                                    IMongoCollection<StoreInfo> StoreInfocollection = _database.GetCollection<StoreInfo>("StoreInfo");
                                    var filter = Builders<StoreInfo>.Filter.AnyEq(b => b.UserIps, Ip);
                                    var count = await StoreInfocollection.CountDocumentsAsync(filter);
                                    if (count > 0)
                                    {

                                        var update = Builders<StoreInfo>.Update.Combine();

                                        var existingIPs = await StoreInfocollection.Find(filter).Project(b => b.UserIps).FirstOrDefaultAsync();
                                        var newIps = UserData.UserIps.Except(existingIPs).ToArray();
                                        if (newIps.Count() > 0)
                                        {
                                            update = update.PushEach(b => b.UserIps, newIps);
                                        }

                                        var existingIds = await StoreInfocollection.Find(filter).Project(b => b.UserIds).FirstOrDefaultAsync();
                                        string[] SoReal = new string[] { UserData.AccountId };
                                        var newIds = SoReal.Except(existingIds).ToArray();

                                        if (newIds.Count() > 0)
                                        {
                                            update = update.PushEach(b => b.UserIds, newIds);
                                        }

                                        if (!update.Equals(Builders<StoreInfo>.Update.Combine()))
                                        {

                                            var updateResult = await StoreInfocollection.UpdateManyAsync(filter, update);

                                            if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
                                            {
                                                await BanAndWebHooks.Init(Saved.DeserializeConfig, responseData1);

                                                await Handlers.UpdateOne<User>("DiscordId", UserData.DiscordId, new Dictionary<string, object>()
                                                {
                                                   { "banned", true }
                                                });

                                                return Ok(new { test = "Banned" });
                                            }
                                            else
                                            {
                                                return Ok(new { test = "Server Issue" });
                                            }
                                        }
                                        else
                                        {
                                            return Ok(new { test = "Server Issue: Already Banned! YOU NAUGHTY BOY!" });
                                        }
                                    }
                                    else
                                    {
                                        if (!UserData.UserIps.Contains(UserIp[0]))
                                        {
                                            await Handlers.PushOne<User>("DiscordId", id, new Dictionary<string, object>()
                                            {
                                                { "UserIps", Ip }
                                            }, false);
                                        }
                                    }

                                    return Redirect("http://127.0.0.1:2158/callback?code=" + NewAccessToken);
                                }
                            }
                            else
                            {
                                return Ok(new { test = "error!! couldnt login" });
                            }
                        }
                        else
                        {
                            var FindUserId = await Handlers.FindOne<User>("username", GlobalName);
                            if (FindUserId != "Error")
                            {
                                FindUserId = await Handlers.FindOne<User>("username", GlobalName);
                                if (FindUserId != "Error")
                                {
                                    // Create the account
                                    return Ok(new { test = "username is in use but lkets make you a acc" });
                                }
                                else
                                {
                                    string NewAccessToken = await CreateAccount.Init(httpContext, _database, responseData1, true);
                                    if (NewAccessToken == "ERROR")
                                    {
                                        return Ok(new { test = "ERROR CREATING ACCOUNTS PLEASE TELL DEVS" });
                                    }
                                    return Redirect("http://127.0.0.1:2158/callback?code=" + NewAccessToken);
                                }
                            }
                            else
                            {
                                string NewAccessToken = await CreateAccount.Init(httpContext, _database, responseData1);
                                if (NewAccessToken == "ERROR")
                                {
                                    return Ok(new { test = "ERROR CREATING ACCOUNTS PLEASE TELL DEVS" });
                                }
                                return Redirect("http://127.0.0.1:2158/callback?code=" + NewAccessToken);
                            }
                        }
                    }
                    else
                    {
                        return Ok(new { test = "User is not in discord server!" });
                    }


                    //https://discord.com/api/users/@me
                    //Console.WriteLine(responseContent);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return Ok(new { test = "Unknown Issue!" });
        }

    }
}