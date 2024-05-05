﻿using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.ADMIN;
using FortBackend.src.App.Utilities.Constants;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortBackend.src.App.Utilities.Saved;
using FortLibrary;
using FortLibrary.Dynamics;
using FortLibrary.Encoders;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FortBackend.src.App.Routes.ADMIN
{
    [Route("/admin")]
    public class HomeController : Controller
    {
        [HttpGet("login")]
        public IActionResult Index()
        {
            return View("~/src/App/Utilities/ADMIN/Pages/Index.cshtml");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Index(string email, string password)
        {

            Console.WriteLine(email);
            Console.WriteLine($"Password {password}");
            if (email == Saved.DeserializeConfig.AdminEmail)
            {
                if (password == Saved.DeserializeConfig.AdminPassword)
                {
                    Console.WriteLine($"w");
                    ViewBag.ErrorMessage = "Correct!";
                    var Token = JWT.GenerateRandomJwtToken(24, Saved.DeserializeConfig.JWTKEY);

                    AdminData adminData = Saved.CachedAdminData.Data?.FirstOrDefault(e => e.AdminUser == email);
                    if (adminData != null)
                    {
                        if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
                        {
                            Console.WriteLine(authToken);
                            Console.WriteLine(adminData.AccessToken);
                            if (adminData.AccessToken == authToken)
                            {
                                Console.WriteLine("CORRET TOKEN!");

                                return Redirect("/dashboard");
                            }

                        }
                    }
                    else
                    {
                        if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
                        {
                            Console.WriteLine(authToken);
                            if (adminData != null)
                            {
                                Console.WriteLine(adminData.AccessToken);
                                if (adminData.AccessToken == authToken)
                                {
                                    Console.WriteLine("CORRET TOKEN!");
                                    return Redirect("/dashboard");
                                }
                            }
                        }

                        Saved.CachedAdminData.Data.Add(new AdminData
                        {
                            AccessToken = Token,
                            AdminUser = email,
                            IsForcedAdmin = email == Saved.DeserializeConfig.AdminEmail,
                            RoleId = AdminDashboardRoles.Admin
                        });
                    }
                    Response.Cookies.Delete("AuthToken");

                    Response.Cookies.Append("AuthToken", Token, new CookieOptions
                    {
                       // HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict
                    });

                    //HttpContext.Session.SetString("Key", "Value");

                    return Redirect("/admin/dashboard");
                }
            }
            else
            {
                //ProfileEmail(email);
                AdminProfileCacheEntry FoundAcc = GrabAdminData.AdminUsers?.FirstOrDefault(e => e.profileCacheEntry.UserData.Email == email)!;
                if(FoundAcc != null && !string.IsNullOrEmpty(FoundAcc.profileCacheEntry.AccountId))
                {
                    ProfileCacheEntry profileCacheEntry = FoundAcc.profileCacheEntry;
                    if (profileCacheEntry.UserData.Password == password)
                    {
                        ViewBag.ErrorMessage = "Correct!";
                        var Token = JWT.GenerateRandomJwtToken(24, Saved.DeserializeConfig.JWTKEY);

                        AdminData adminData = Saved.CachedAdminData.Data?.FirstOrDefault(e => e.AdminUser == email);
                        if (adminData != null)
                        {
                            if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
                            {
                                if (adminData.AccessToken == authToken)
                                {
                                    return Redirect("/dashboard");
                                }

                            }
                        }
                        else
                        {
                            if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
                            {
                                if (adminData != null)
                                {
                                    if (adminData.AccessToken == authToken)
                                    {
                                        return Redirect("/dashboard");
                                    }
                                }
                            }

                            Saved.CachedAdminData.Data.Add(new AdminData
                            {
                                AccessToken = Token,
                                AdminUser = FoundAcc.profileCacheEntry.UserData.Email,
                                AdminUserName = FoundAcc.profileCacheEntry.UserData.Username,
                                IsForcedAdmin = email == Saved.DeserializeConfig.AdminEmail,
                                RoleId = FoundAcc.adminInfo.Role
                            });
                        }
                        Response.Cookies.Delete("AuthToken");

                        Response.Cookies.Append("AuthToken", Token, new CookieOptions
                        {
                            // HttpOnly = true,
                            Secure = false,
                            SameSite = SameSiteMode.Strict
                        });


                        return Redirect("/admin/dashboard");
                    }
                }
            }

            ViewBag.ErrorMessage = "Invalid email or password.";

            return View("~/src/App/Utilities/ADMIN/Pages/Index.cshtml");
        }


        [HttpGet("logout")]
        public IActionResult Details()
        {
            if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
            {
                if(!string.IsNullOrEmpty(authToken) && Saved.CachedAdminData.Data != null)
                {
                    AdminData adminData = Saved.CachedAdminData.Data?.FirstOrDefault(e => e.AccessToken == authToken);
                    if (adminData != null)
                    {
                        Saved.CachedAdminData.Data!.Remove(adminData);
                    }
                }
            }

            return Redirect("/admin/login");
        }



    }
}

/*
 * if (context.Request.Path.StartsWithSegments("/css") && context.Request.Path.Value.EndsWith(".css"))
//                {
//                    string cssFileName = Path.GetFileNameWithoutExtension(context.Request.Path.Value);

//                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "src/App/Utilities/ADMIN/CSS", cssFileName + ".css");

//                    Console.WriteLine(filePath);
//                    if (!System.IO.File.Exists(filePath))
//                    {
//                        context.Response.StatusCode = 404;
//                    }
//                    else
//                    {
//                        string cssContent = System.IO.File.ReadAllText(filePath);
                        
//                        Console.WriteLine(cssFileName);
//                        context.Response.ContentType = "text/css";
//                        await context.Response.WriteAsync(cssContent);
                        

                        
//                    }

                   
//                }
*/