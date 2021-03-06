﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace family_archive_server.Models
{
    public class RawFIleUpload
    {
        public string Details { get; set; }
        public IFormFile File { get; set; }
    }
    public class FIleUpload
    {
        public string Name { get; set; }
        public Location Location { get; set; }
        public UpdateDate Date { get; set; }
        public string Description { get; set; }
        public IList<PeopleList> People { get; set; }
    }
}
