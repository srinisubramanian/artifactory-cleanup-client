using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtifactoryClient
{
    class Repository
    {
        public string key { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }

    class Folder
    {
        public string uri { get; set; }
        public string folder { get; set; }
    }

    class FolderInfo
    {
        public string metadataUri { get; set; }
        public string repo { get; set; }
        public string path { get; set; }
        public string created { get; set; }
        public string createdBy { get; set; }
        public string lastModified { get; set; }
        public string modifiedBy { get; set; }
        public string lastUpdated { get; set; }
        public IList<Folder> children { get; set; }
        public string uri { get; set; }
    }
}
