﻿using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortBackend.src.App.Utilities.MongoDB.Module;
using FortBackend.src.App.XMPP.Helpers.Resources;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace FortBackend.src.App.Routes.APIS.Accounts
{
    [ApiController]
    [Route("account/api")]
    public class KillController : ControllerBase
    {
        //https://account-public-service-prod.ol.epicgames.com/account/api/oauth/sessions/kill?killType=OTHERS_ACCOUNT_CLIENT_SERVICE
        [HttpDelete("oauth/sessions/kill")]
        public IActionResult KillSessions([FromQuery] string killType)
        {
            switch (killType)
            {
                case "ALL":

                    return NoContent();

                case "OTHERS":
                    return NoContent();

                default:
                    return NoContent();
            }
        }
        // all work on this soon
        [HttpDelete("oauth/sessions/kill/{accesstoken}")]
        public async Task<IActionResult> KillAccessSessions(string accesstoken)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Split("bearer ")[1];

                var accessToken = token.Replace("eg1~", "");

                var handler = new JwtSecurityTokenHandler();
                var decodedToken = handler.ReadJwtToken(accessToken);
                Console.WriteLine("TEST");
                var AccountData = await Handlers.FindOne<Account>("accountId", decodedToken.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value.ToString());

                if (AccountData != "Error")
                {
                    Account AccountDataParsed = JsonConvert.DeserializeObject<Account[]>(AccountData)?[0];
                    if (AccountDataParsed != null)
                    {
                        Console.WriteLine("KILLING TOKEN " + accessToken);
                        var AccessTokenIndex = GlobalData.AccessToken.FindIndex(i => i.token == accesstoken);

                        if (AccessTokenIndex != -1)
                        {
                            var AccessToken = GlobalData.AccessToken[AccessTokenIndex];
                            GlobalData.AccessToken.RemoveAt(AccessTokenIndex);

                            var XmppClient = GlobalData.Clients.Find(i => i.token == AccessToken.token);
                            if (XmppClient != null)
                            {
                                XmppClient.Client.Dispose();
                            }

                            var RefreshTokenIndex = GlobalData.RefreshToken.FindIndex(i => i.accountId == AccessToken.accountId);
                            if (RefreshTokenIndex != -1)
                            {
                                GlobalData.RefreshToken.RemoveAt(RefreshTokenIndex);
                            }
                        }

                        var ClientTokenIndex = GlobalData.ClientToken.FindIndex(i => i.token == accesstoken);
                        if (ClientTokenIndex != -1)
                        {
                            GlobalData.ClientToken.RemoveAt(ClientTokenIndex);
                        }

                        //if (AccessTokenIndex != -1 || ClientTokenIndex != -1) // in the future i'll implement
                        //{
                        //    Console.WriteLine("WOAH");
                        //    await Handlers.UpdateOne<Account>("accountId", AccountDataParsed.AccountId, new Dictionary<string, object>
                        //    {
                        //        {
                        //            "refreshToken", new string[] { }
                        //        },
                        //        {
                        //            "accessToken", new string[] { }
                        //        },
                        //        {
                        //            "clientToken", new string[] {}
                        //        }
                        //    });
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OAUTH KILL " + ex.Message);
            }
            return NoContent();
        }

    }
}
