using Migration_Tool_GraphAPI.Models;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
///Calender
using Migration_Tool_GraphAPI.TokenStorage;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Migration_Tool_GraphAPI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Migration_Tool_GraphAPI.Helpers;
using Owin;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using System;
using System.ComponentModel;
///
namespace Migration_Tool_GraphAPI.Helpers
{
    public static class GraphHelper
    {
        // Load configuration settings from PrivateSettings.config
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static List<string> graphScopes =
            new List<string>(ConfigurationManager.AppSettings["ida:AppScopes"].Split(' '));
        private static GraphServiceClient graphClient;

        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var events = await graphClient.Me.Events.Request()
                .Select("subject,organizer,start,end")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return events.CurrentPage;
        }

        public static GraphServiceClient GetAuthenticatedClient()
        {
            if (graphClient == null)
            {
                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(appId)
                    .WithClientSecret(appSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                    .Build();

                var authProvider = new DelegateAuthenticationProvider(async (request) =>
                {
                    var tokenResult = await confidentialClient
                        .AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                        .ExecuteAsync();

                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                });

                graphClient = new GraphServiceClient(authProvider);
            }

            return graphClient;
        }

        public static async Task<CachedUser> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            var user = await graphClient.Me.Request()
                .Select(u => new
                {
                    u.DisplayName,
                    u.Mail,
                    u.UserPrincipalName,

                })
                .GetAsync();

            return new CachedUser
            {
                Avatar = string.Empty,
                DisplayName = user.DisplayName,
                Email = string.IsNullOrEmpty(user.Mail) ?
                    user.UserPrincipalName : user.Mail
            };
        }
        public static async Task UploadFileToSharePointAsync(string filePath, string sharepointSiteUrl, string folderPath)
        {
            var graphClient = GetAuthenticatedClient();

            // Get site ID based on SharePoint URL
            var site = await graphClient.Sites.GetByPath(new Uri(sharepointSiteUrl).AbsolutePath, "microsoft.sharepoint.com").Request().GetAsync();

            // Get folder reference in SharePoint
            var folder = await graphClient.Sites[site.Id].Drive.Root.ItemWithPath(folderPath).Request().GetAsync();

            // Read file as a stream
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // Upload the file to SharePoint
                await graphClient.Sites[site.Id].Drives[folder.ParentReference.DriveId]
                    .Items[folder.Id]
                    .ItemWithPath(Path.GetFileName(filePath))
                    .Content.Request()
                    .PutAsync<DriveItem>(fileStream);
            }
        }
    }
}

