﻿using FortBackend.src.App.Utilities.Classes.EpicResponses;
using FortBackend.src.App.Utilities.Classes.EpicResponses.Lightswitch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FortBackend.src.App.Routes.APIS.Lightswitch
{
    [ApiController]
    [Route("lightswitch/api/service/bulk/status")]
    public class LightSwitchApiController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<LightSwitchData>>> LightSwitch()
        {
            Response.ContentType = "application/json";

            return new List<LightSwitchData>
            {
                new LightSwitchData()
            };
        }
    }
}