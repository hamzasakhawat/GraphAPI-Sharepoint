using Microsoft.Graph;
using Migration_Tool_GraphAPI.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Linq;

namespace Migration_Tool_GraphAPI.Controllers
{
    public class DashboardController : Controller
    {
        // Render the dashboard view
        public ActionResult Index()
        {
            return View();
        }

        // Start transfer action
        [HttpPost]
        public async Task<ActionResult> StartTransfer(HttpPostedFileBase file, string destinationUrl)
        {
            if (file == null || string.IsNullOrWhiteSpace(destinationUrl))
            {
                ViewBag.Error = "File or destination URL is missing.";
                return View("Index");
            }

            try
            {
                // Get Microsoft Graph client
                var graphClient = GraphHelper.GetAuthenticatedClient();

                // Read the file into a byte array
                byte[] fileData;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }

                // Extract site information from destination URL
                var sharePointUri = new Uri(destinationUrl);
                var site = await graphClient
                    .Sites
                    .GetByPath(path, [hostURL])
                    .Request()
                    .GetAsync();

                var siteId = site.Id;
                var drives = await graphClient.Sites[siteId].Drives.Request().GetAsync();
                var documentsDrive = drives.CurrentPage.FirstOrDefault(d => d.Name == "Documents" || d.Name == "Shared Documents");

                // Upload the file to SharePoint
                var uploadResult = await graphClient
                    .Sites[siteId]
                    .Drives[documentsDrive.Id]
                    .Root
                    .ItemWithPath(file.FileName)
                    .Content
                    .Request()
                    .PutAsync<DriveItem>(new MemoryStream(fileData));

                ViewBag.Message = "File uploaded successfully!";
            }
            catch (ServiceException ex)
            {
                ViewBag.Error = $"An error occurred: {ex.Message}";
            }

            return View("Index");
        }
    }
}
