﻿using Discord;
using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.Discord;
using FortBackend.src.App.Utilities.MongoDB.Module;
using FortBackend.src.App.Utilities.Saved;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json;
using System.Text;
using static FortBackend.src.App.Utilities.Classes.DiscordAuth;

namespace FortBackend.src.App.Routes.Development
{
    public class unbanAndWebhooks
    {
        public static async Task Init(Config DeserializeConfig, UserInfo userinfo, string Message = "false ban", string BodyMessage = "Moderator")
        {
            try
            {
                string webhookUrl = DeserializeConfig.DetectedWebhookUrl;

                if (webhookUrl == "")
                {
                    Logger.Error($"Webhook is null", "BanAndWebhook");
                    return;
                }

                if (DiscordBot.guild != null)
                {
                    var guild = DiscordBot.Client.GetGuild(DeserializeConfig.ServerID);
                    if (ulong.TryParse(userinfo.id, out ulong userId))
                    {
                        try
                        {
                            RequestOptions NoWay = new RequestOptions();
                            NoWay.AuditLogReason = Message;

                            await guild.RemoveBanAsync(userId, NoWay);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Why is this null");
                }

                var embed2 = new
                {
                    title = "Unbanned",
                    footer = new { text = userinfo.username + " Has Been Unbanned!" },
                    fields = new[]
                    {
                    new { name = "UserId", value = userinfo.id.ToString(), inline = false },
                    new { name = "Reason", value = Message, inline = false },
                    new { name = "Unbanned By", value = BodyMessage, inline = false }
                },
                    color = 0x00FFFF
                };

                string jsonPayload2 = JsonConvert.SerializeObject(new { embeds = new[] { embed2 } });

                using (var httpClient = new HttpClient())
                {

                    HttpContent httpContent2 = new StringContent(jsonPayload2, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response2 = await httpClient.PostAsync(webhookUrl, httpContent2);

                        if (!response2.IsSuccessStatusCode)
                        {
                            Logger.Error($"Failed to send message. Status code: {response2.StatusCode}", "BanAndWebhook");
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Logger.Error($"Error sending request: {ex.Message}", "BanAndWebhook");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "BanAndWebhooks");
            }
        }
    }
}
