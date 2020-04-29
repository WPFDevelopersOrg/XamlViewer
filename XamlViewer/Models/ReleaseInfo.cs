using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlViewer.Models
{
    public class asset
    {
        public string browser_download_url { get; set; }
    }

    public class ReleaseInfo
    {
        public string name { get; set; }
        public asset[] assets { get; set; }
        public string body { get; set; }
    }
}
