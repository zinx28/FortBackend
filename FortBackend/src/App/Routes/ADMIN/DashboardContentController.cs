﻿using FortBackend.src.App.Utilities.ADMIN;
using FortBackend.src.App.Utilities.Helpers.Cached;
using FortBackend.src.App.Utilities.Saved;
using FortLibrary.ConfigHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace FortBackend.src.App.Routes.ADMIN
{
    public class TempDataModel
    {
        public string data { get; set; } = string.Empty;
    }

    [Route("/admin/dashboard/content")]
    public class DashboardContentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
            {
                AdminData adminData = Saved.CachedAdminData.Data?.FirstOrDefault(e => e.AccessToken == authToken);
                if (adminData != null)
                {
                    Console.WriteLine("Valid User!");
                    ViewData["Username"] = adminData.AdminUserName;
                    return View("~/src/App/Utilities/ADMIN/PAGES/Dashboard/Content.cshtml");
                }
            }

            return Redirect("/admin/login");
        }


        [HttpPost("update")]
        public IActionResult UpdateTempDataV2([FromBody] JsonElement tempData)
        {
            /*
             * 
             * TO DO: ADD AUTH SO NORMAL PEOPLE CANT UPDATE OR BREAK IT
             * 
             */
            try
            {

                if (tempData.TryGetProperty("data", out JsonElement dataElement))
                {
                    string dataValue = dataElement.ToString();
                    Console.WriteLine(dataValue);
                    if (!string.IsNullOrEmpty(dataValue))
                    {
                        NewsManager.ContentConfig = JsonConvert.DeserializeObject<ContentConfig>(dataValue);
                        NewsManager.Update();
                        return Json(true);
                    }

                }
                return Json(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating temp data: {ex.Message}");
                return Json(false);
            }
        }
    }
}