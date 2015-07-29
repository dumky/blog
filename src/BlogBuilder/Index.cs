using DotLiquid;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace BlogBuilder
{

    public class Index : Drop
    {
        [YamlMember(Alias = "blog-title")]
        public string BlogTitle { get; set; }

        [YamlMember(Alias = "blog-author")]
        public string BlogAuthor { get; set; }

        [YamlMember(Alias = "blog-contact")]
        public string BlogContact { get; set; }

        [YamlMember(Alias = "blog-url")]
        public string BlogUrl { get; set; }

        [YamlMember(Alias = "blog-description")]
        public string BlogDescription { get; set; }

        [YamlMember(Alias = "ftp-host")]
        public string FtpHost { get; set; }

        [YamlMember(Alias = "ftp-user")]
        public string FtpUser { get; set; }

        [YamlMember(Alias = "ftp-dir")]
        public string FtpDir { get { return ftpDir ?? ""; } set { ftpDir = value; } }
        private string ftpDir;

        private List<Entry> entries;
        public List<Entry> Entries
        {
            get { return entries; }
            set { entries = value.OrderByDescending(o => o.Date).ToList(); }
        }

        public string FrontPageFullPath
        {
            get
            {
                return Path.Combine(Globals.outputRoot, "index.html");
            }
        }

        public string RSSFullPath
        {
            get
            {
                return Path.Combine(Globals.outputRoot, "index.rss");
            }
        }

        public string ArchivesFullPath
        {
            get
            {
                return Path.Combine(Globals.outputRoot, "archives.html");
            }
        }
    }

}
