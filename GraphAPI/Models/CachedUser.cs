﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Migration_Tool_GraphAPI.Models

{
    // Simple class to serialize user details
    public class CachedUser
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
    }
}