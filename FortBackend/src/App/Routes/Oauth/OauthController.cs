﻿using FortBackend.src.App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using FortLibrary.Encoders;
using System.IdentityModel.Tokens.Jwt;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortBackend.src.App.XMPP_Server.Globals;
using FortBackend.src.App.Utilities.Saved;
using FortLibrary.EpicResponses.Errors;
using FortLibrary.EpicResponses.Oauth;
using FortLibrary;
using Microsoft.IdentityModel.Tokens;


namespace FortBackend.src.App.Routes.Oauth
{
    [ApiController]
    [Route("account/api")]
    public class OauthApiController : ControllerBase
    {

        [HttpGet("oauth/websocket/addnew/token")]
        public async Task<IActionResult> NewToken()
        {
            try
            {
                string accessToken = Request.Headers["Authorization"]!;
                string refreshToken = Request.Headers["RefreshToken"]!;

                Console.WriteLine(accessToken);
                Console.WriteLine(refreshToken);

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var decodedToken = handler.ReadJwtToken(accessToken);
                    string[] tokenParts = decodedToken.ToString().Split('.');

                    if (tokenParts.Length == 2)
                    {
                        var payloadJson = tokenParts[1];
                        dynamic payload = JsonConvert.DeserializeObject(payloadJson)!;
                        if (payload == null)
                        {
                            return BadRequest(new { });
                        }

                        // wrong type of token
                        if (string.IsNullOrEmpty(payload.sub.ToString()))
                        {
                            return BadRequest(new { });
                        }

                        // check the account
                        ProfileCacheEntry profileCacheEntry = await GrabData.Profile(payload.sub.ToString());
                        Console.WriteLine(profileCacheEntry.AccountId);
                        if (!string.IsNullOrEmpty(profileCacheEntry.AccountId) && profileCacheEntry.UserData.banned != true)
                        {
                            var FindAccount = GlobalData.AccessToken.FirstOrDefault(e => e.accountId == profileCacheEntry.AccountId);
                            if (FindAccount != null)
                            {
                                GlobalData.AccessToken.Remove(FindAccount);
                                GlobalData.AccessToken.Add(new TokenData
                                {
                                    token = $"eg1~{accessToken}",
                                    creation_date = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                                    accountId = FindAccount.accountId,
                                });
                            }
                            else
                            {
                                GlobalData.AccessToken.Add(new TokenData
                                {
                                    token = $"eg1~{accessToken}",
                                    creation_date = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                                    accountId = payload.sub,
                                });
                            }

                            var RefreshAccount = GlobalData.RefreshToken.FirstOrDefault(e => e.accountId == profileCacheEntry.AccountId);
                            if (RefreshAccount != null)
                            {
                                GlobalData.RefreshToken.Remove(RefreshAccount);
                                GlobalData.RefreshToken.Add(new TokenData
                                {
                                    token = $"eg1~{accessToken}",
                                    creation_date = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                                    accountId = RefreshAccount.accountId,
                                });
                            }
                            else
                            {
                                GlobalData.RefreshToken.Add(new TokenData
                                {
                                    token = $"eg1~{accessToken}",
                                    creation_date = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                                    accountId = payload.sub,
                                });
                            }
                        }

                    }
                }
                return Ok(new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "CUSTOM~OAUTH");
            }
            return BadRequest(new { });
        }

        [HttpGet("oauth/verify")]
        public async Task<IActionResult> VerifyToken()
        {
            Response.ContentType = "application/json";
            try
            {
                var tokenArray = Request.Headers["Authorization"].ToString().Split("bearer ");
                var token = tokenArray.Length > 1 ? tokenArray[1] : "";

                bool FoundAccount = false;
                if (GlobalData.AccessToken.Any(e => e.token == token))
                    FoundAccount = true;
                else if (GlobalData.RefreshToken.Any(e => e.token == token))
                    FoundAccount = true;

                if (FoundAccount)
                {
                    var accessToken = token.Replace("eg1~", "");

                    var handler = new JwtSecurityTokenHandler();
                    var decodedToken = handler.ReadJwtToken(accessToken);

                    //Console.WriteLine(decodedToken);
                    string[] tokenParts = decodedToken.ToString().Split('.');

                    if (tokenParts.Length == 2)
                    {
                        var payloadJson = tokenParts[1];
                        dynamic payload = JsonConvert.DeserializeObject(payloadJson)!;
                        if (payload == null)
                        {
                            return BadRequest(new { });
                        }
 
                        ProfileCacheEntry profileCacheEntry = await GrabData.Profile(payload.sub.ToString());

                        if (profileCacheEntry != null)
                        {
                            if (profileCacheEntry.UserData.banned != true)
                            {
                                return Ok(new
                                {
                                    token = $"{token}",
                                    session_id = payload.jti.ToString(),
                                    token_type = "bearer",
                                    client_id = payload.clid.ToString(),
                                    internal_client = true,
                                    client_service = "fortnite",
                                    account_id = profileCacheEntry.UserData.AccountId,
                                    expires_in = 28800,
                                    expires_at = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                    auth_method = payload.am.ToString(),
                                    display_name = profileCacheEntry.UserData.Username,
                                    app = "fortnite",
                                    in_app_id = payload.sub.ToString(),
                                    device_id = payload.dvid.ToString()
                                });
                            }                           
                        }
                    }
                }

                throw new BaseError
                {
                    errorCode = "errors.com.epicgames.common.authentication.authentication_failed",
                    errorMessage = $"Authentication failed for /account/api/oauth/verify",
                    messageVars = new List<string> { $"/account/api/oauth/verify" },
                    numericErrorCode = 1032,
                    originatingService = "any",
                    intent = "prod",
                    error_description = $"Authentication failed for /account/api/oauth/verify",
                };
            }
            catch (BaseError ex)
            {
                var jsonResult = JsonConvert.SerializeObject(BaseError.FromBaseError(ex));
                StatusCode(500);
                return new ContentResult()
                {
                    Content = jsonResult,
                    ContentType = "application/json",
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"[OauthApi:Verify] -> {ex.Message}");
            }
     
            // shouldn't never call this
            return BadRequest(new { });
        }

        [HttpPost("oauth/token")]
        public async Task<IActionResult> LoginToken()
        {
            Response.ContentType = "application/json";
            try
            {
                var Headers = Request.Headers;
                var FormRequest = HttpContext.Request.Form;

                string grant_type = "";
                string DisplayName = "";
                string Email = "";
                string AccountId = "";
                string Password = "";
                string refresh_token = "";
                string exchange_token = "";
                bool IsMyFavUserBanned = false;

                if (FormRequest.TryGetValue("grant_type", out var GrantType))
                {
                    if(!string.IsNullOrEmpty(GrantType))
                        grant_type = GrantType!;
                }

                if (FormRequest.TryGetValue("username", out var username))
                {
                    if (!string.IsNullOrEmpty(username))
                        Email = username!;
                }

                if (FormRequest.TryGetValue("exchange_code", out var ExchangeCode))
                {
                    if (!string.IsNullOrEmpty(ExchangeCode))
                        exchange_token = ExchangeCode!;
                }

                if (FormRequest.TryGetValue("refresh_code", out var Refresh_code))
                {
                    if (!string.IsNullOrEmpty(Refresh_code))
                        refresh_token = Refresh_code!;
                }

                if (FormRequest.TryGetValue("password", out var password))
                {
                    if (!string.IsNullOrEmpty(password))
                        Password = password!;
                }

                Console.WriteLine(grant_type);

                string clientId = "";
                try
                {
                    if (Saved.DeserializeConfig.LunaPROD)
                    {
                        //foreach (var header in Request.Headers)
                        //{
                        //    Console.WriteLine($"{header.Key}: {header.Value}");
                        //}
                        string LunaDevTesting = Request.Headers["lunaprod"]!;
                       // Console.WriteLine(LunaDevTesting);
                        if(LunaDevTesting != "verytrue") {
                            Logger.Error("FAKE CURL?");
                            return BadRequest(new BaseError
                            {
                                errorCode = "errors.com.epicgames.account.invalid_client",
                                errorMessage = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                                messageVars = new List<string>(),
                                numericErrorCode = 1011,
                                originatingService = "any",
                                intent = "prod",
                                error_description = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                                error = "invalid_client"
                            }); 
                        }
                    }

                    string AuthorizationToken = Request.Headers["Authorization"]!;

                    if (string.IsNullOrEmpty(AuthorizationToken))
                    {
                        throw new Exception("Authorization header is missing");
                    }

                    string base64String = AuthorizationToken.Substring(6);
                    byte[] base64Bytes = Convert.FromBase64String(base64String);
                    string decodedString = Encoding.UTF8.GetString(base64Bytes);
                    string[] credentials = decodedString.Split(':');

                    if (credentials.Length < 2)
                    {
                        throw new Exception("Invalid credentials format");
                    }

                    clientId = credentials[0];

                    if (string.IsNullOrEmpty(clientId))
                    {
                        throw new Exception("Invalid Client ID");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("OAUTH -> " + ex.Message);
                    Response.StatusCode = 400;
                    return Ok(new BaseError
                    {
                        errorCode = "errors.com.epicgames.account.invalid_client",
                        errorMessage = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                        messageVars = new List<string>(),
                        numericErrorCode = 1011,
                        originatingService = "any",
                        intent = "prod",
                        error_description = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                        error = "invalid_client"
                    });
                }
               
                switch (grant_type)
                {
                    case "exchange_code":
                        if (string.IsNullOrEmpty(exchange_token))
                        {
                            return BadRequest(new BaseError
                            {
                                errorCode = "errors.com.epicgames.common.oauth.invalid_request",
                                errorMessage = "Sorry the exchange code you supplied was not found. It is possible that it was no longer valid",
                                messageVars = new List<string>(),
                                numericErrorCode = 18057,
                                originatingService = "any",
                                intent = "prod",
                                error_description = "Sorry the exchange code you supplied was not found. It is possible that it was no longer valid"
                            });
                        }

                        ProfileCacheEntry profileCacheEntry = await GrabData.Profile("", true, exchange_token);
                        if (!string.IsNullOrEmpty(profileCacheEntry.UserData.AccountId))
                        {
                            DisplayName = profileCacheEntry.UserData.Username;
                            AccountId = profileCacheEntry.UserData.AccountId;
                            IsMyFavUserBanned = profileCacheEntry.UserData.banned;
                        }                       
                        
                        break;

                    case "client_credentials":
                        if (string.IsNullOrEmpty(clientId))
                        {
                            return BadRequest(new BaseError
                            {
                                errorCode = "errors.com.epicgames.account.invalid_client",
                                errorMessage = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                                messageVars = new List<string>(),
                                numericErrorCode = 1011,
                                originatingService = "any",
                                intent = "prod",
                                error_description = "It appears that your Authorization header may be invalid or not present, please verify that you are sending the correct headers.",
                                error = "invalid_client"
                            });
                        }

                        string ClientToken = JWT.GenerateJwtToken(new[]
                        {
                            new Claim("p", Base64.GenerateRandomString(128)),
                            new Claim("clsvc", "fortnite"),
                            new Claim("t", "s"),
                            new Claim("mver", false.ToString()),
                            new Claim("clid", clientId),
                            new Claim("ic", "true"),
                            new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(240).ToUnixTimeSeconds().ToString()),
                            new Claim("am", "client_credentials"),
                            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                            new Claim("jti", Hex.GenerateRandomHexString(32).ToLower()),
                        }, 24);

                        if(ClientToken != null) // this is never null BUT fixes a warning
                        {
                            GlobalData.ClientToken.Add(new TokenData
                            {
                                accountId = AccountId,
                                token = $"eg1~{ClientToken}"
                            });

                            return Ok(new
                            {
                                access_token = $"eg1~{ClientToken}",
                                expires_in = 28800,
                                expires_at = DateTimeOffset.UtcNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                token_type = "bearer",
                                client_id = clientId,
                                internal_client = true,
                                client_service = "fortnite"
                            });
                        }
                    break;

                    case "refresh_token":
                        Logger.Error("THIS IS UNFINISHED SO YOU MAY HAVE LOGIN ISSUES");
                        if (string.IsNullOrEmpty(refresh_token))
                        {
                            return BadRequest(new BaseError
                            {
                                errorCode = "errors.com.epicgames.common.oauth.invalid_request",
                                errorMessage = "Refresh token is required.",
                                messageVars = new List<string>(),
                                numericErrorCode = 18057,
                                originatingService = "any",
                                intent = "prod",
                                error_description = "Refresh token is required."
                            });
                        }

                        var refreshTokenIndex = GlobalData.RefreshToken.FindIndex(x => x.token == refresh_token);
                        if(refreshTokenIndex != -1)
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var decodedRefreshToken = handler.ReadJwtToken(GlobalData.RefreshToken[refreshTokenIndex].token.Replace("eg1~", ""));
             
                            if (decodedRefreshToken != null)
                            {
                                var creationDateClaim = decodedRefreshToken.Claims.FirstOrDefault(claim => claim.Type == "creation_date");

                                if (creationDateClaim != null)
                                {
                                    DateTime creationDate = DateTime.Parse(creationDateClaim.Value);
                                    int hoursToExpire = 1920; 

                                    DateTime expirationDate = creationDate.AddHours(hoursToExpire);

                                    if (expirationDate <= DateTime.UtcNow)
                                    {
                                        return BadRequest(new BaseError
                                        {
                                            errorCode = "errors.com.epicgames.common.oauth.invalid_request",
                                            errorMessage = "EXPIED",
                                            messageVars = new List<string>(),
                                            numericErrorCode = 18057,
                                            originatingService = "any",
                                            intent = "prod",
                                            error_description = "EXPIED."
                                        });
                                    }

                                    GlobalData.RefreshToken[refreshTokenIndex].creation_date = expirationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
                                }

                                return Ok(new OauthToken
                                {
                                    token_type = "bearer",
                                    account_id = AccountId,
                                    client_id = clientId,
                                    internal_client = true,
                                    client_service = "fortnite",
                                    refresh_token = $"{GlobalData.RefreshToken[refreshTokenIndex].token}",
                                    refresh_expires = 115200,
                                    refresh_expires_at = DateTimeOffset.UtcNow.AddHours(32).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                    displayName = DisplayName,
                                    app = "fortnite",
                                    in_app_id = AccountId,
                                    device_id = Hex.GenerateRandomHexString(16)
                                });
                            }
                            else
                            {
                                Console.WriteLine("FAILED TO DECODE TOEK");
                                throw new Exception("Failed to decode refresh token.");
                            }
                        }
                        break;
                }

                if (IsMyFavUserBanned)
                {
                    var jsonResult = JsonConvert.SerializeObject(new BaseError
                    {
                        errorCode = "errors.com.epicgames.account.account_not_active",
                        errorMessage = $"You have been permanently banned from {Saved.DeserializeConfig.DiscordBotMessage}.", // why not use this
                        messageVars = new List<string>(),
                        numericErrorCode = -1,
                        originatingService = "any",
                        intent = "prod",
                        error_description = $"You have been permanently banned from {Saved.DeserializeConfig.DiscordBotMessage}."
                    });

                    StatusCode(400);
                    return new ContentResult()
                    {
                        Content = jsonResult,
                        ContentType = "application/json",
                        StatusCode = 400
                    };
                }

                if(string.IsNullOrEmpty(AccountId))
                {
                    return BadRequest(new BaseError
                    {
                        errorCode = "errors.com.epicgames.common.oauth.invalid_request",
                        errorMessage = "Server Issue",
                        messageVars = new List<string>(),
                        numericErrorCode = 18057,
                        originatingService = "any",
                        intent = "prod",
                        error_description = "Server Issue"
                    });
                }


                if(GlobalData.AccessToken.Any(e => e.accountId == AccountId))
                {
                    if (GlobalData.RefreshToken.Any(e => e.accountId == AccountId))
                    {
                        var AccessData = GlobalData.AccessToken.FirstOrDefault(e => e.accountId == AccountId);
                        if (AccessData == null) {  return BadRequest(new BaseError {}); } // never would call

                        var RefreshData = GlobalData.RefreshToken.FirstOrDefault(e => e.accountId == AccountId);
                        if (RefreshData == null) { return BadRequest(new BaseError { }); }

                        var AccessToken = AccessData.token.Replace("eg1~", "");

                        var handler = new JwtSecurityTokenHandler();
                        var decodedToken = handler.ReadJwtToken(AccessToken);

                        string[] tokenParts = decodedToken.ToString().Split('.');

                        Console.WriteLine(tokenParts.Length);
                        Console.WriteLine(tokenParts);
                        if (tokenParts.Length == 2)
                        {
                            var payloadJson = tokenParts[1];
                            dynamic payload = JsonConvert.DeserializeObject(payloadJson)!;
                            if (payload == null)
                            {
                                return BadRequest(new { });
                            }

                            if (!string.IsNullOrEmpty(payload.dvid.ToString()))
                            {

                                //GlobalData.RefreshToken.Add(RefreshTokenClient);

                                return Ok(new OauthToken
                                {
                                    access_token = $"eg1~{AccessToken}",
                                    expires_in = 28800,
                                    expires_at = DateTimeOffset.UtcNow.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                    token_type = "bearer",
                                    account_id = AccountId,
                                    client_id = clientId,
                                    internal_client = true,
                                    client_service = "fortnite",
                                    refresh_token = $"{RefreshData}",
                                    refresh_expires = 115200,
                                    refresh_expires_at = DateTimeOffset.UtcNow.AddHours(32).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                    displayName = DisplayName,
                                    app = "fortnite",
                                    in_app_id = AccountId,
                                    device_id = payload.dvid
                                });
                            }

                        }
                        else
                        {
                            Console.WriteLine("FAKE TOKEN?"); // never should happen
                        }
                    }else
                    {
                        Console.WriteLine("TOKEN REFRESH NOT FOUND");
                    }
                }else
                {
                    Console.WriteLine("TOKEN ACCESS NOT FOUND");
                }
             
            }
            catch (Exception ex)
            {
                Logger.Error("OauthToken -> " + ex.Message);
            }

            return BadRequest(new BaseError
            {
                errorCode = "errors.com.epicgames.account.invalid_account_credentials",
                errorMessage = "Seems like there has been a error on the backend",
                messageVars = new List<string>(),
                numericErrorCode = 18031,
                originatingService = "any",
                intent = "prod",
                error_description = "Seems like there has been a error on the backend",
                error = "invalid_grant"
            });
        }

    }
}
