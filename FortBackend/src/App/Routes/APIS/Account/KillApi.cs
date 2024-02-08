﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FortBackend.src.App.Routes.APIS.Account
{
    [ApiController]
    [Route("account/api")]
    public class KillController : ControllerBase
    {
        private IMongoDatabase _database;

        public KillController(IMongoDatabase database)
        {
            _database = database;
        }

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
            return NoContent();
        }

    }
}
