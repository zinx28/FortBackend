﻿using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.Helpers;
using FortLibrary.EpicResponses.FortniteServices.Content;
using FortLibrary.ConfigHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net;
using ZstdSharp.Unsafe;
using static FortBackend.src.App.Utilities.Helpers.Grabber;
using FortBackend.src.App.Utilities.Constants;
using FortBackend.src.App.Utilities.Helpers.Cached;

namespace FortBackend.src.App.Routes.API
{
    [ApiController]
    [Route("content/api/pages/fortnite-game")]

    // I NEED TO REDO THIS AND MAKE IT SUPPORT OTHERS
    public class ContentController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<ContentJson>> ContentApi([FromServices] IMemoryCache memoryCache)
        {
            Response.ContentType = "application/json";
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                string season = "";
                season = (await SeasonUserAgent(Request)).Season.ToString();
                if (season == "10")
                {
                    season = "x";
                }

                var cacheKey = $"ContentEndpointKey-{season}";
                if (memoryCache.TryGetValue(cacheKey, out ContentJson? cachedResult))
                {
                    if(cachedResult != null) { return cachedResult; }  
                }



                var ContentJsonResponse = new ContentJson();
                ContentJsonResponse = NewsManager.ContentJsonResponse;

                ContentJsonResponse.dynamicbackgrounds = new DynamicBackground()
                {
                    backgrounds = new DynamicBackgrounds()
                    {
                        backgrounds = new List<DynamicBackgroundList>
                            {
                                new DynamicBackgroundList
                                {
                                    stage = $"season{season}",
                                    _type = "DynamicBackground",
                                    key = "lobby"
                                }
                            }

                    }
                };

                //NewsManager



                memoryCache.Set(cacheKey, ContentJsonResponse, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                //var jsonData1 = System.IO.File.ReadAllText(Path.Combine(PathConstants.BaseDir, $"src\\Resources\\Json\\ph.json"));
                //ContentJson contentconfig1 = JsonConvert.DeserializeObject<ContentJson>(jsonData); //dynamicbackgrounds.news

                //return Ok(contentconfig1);
                var jsonResponse = JsonConvert.SerializeObject(ContentJsonResponse, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                return Content(jsonResponse, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(new { });
            }

        }
    }
}
