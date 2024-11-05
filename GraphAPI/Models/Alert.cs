using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Migration_Tool_GraphAPI.Models
{
    // Used to flash error messages in the app's views.
    public class Alert
    {
        public const string AlertKey = "TempDataAlerts";
        public string Message { get; set; }
        public string Debug { get; set; }
    }
}