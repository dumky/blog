using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Net.FtpClient.Async;
using System.Security;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// TODO: fix date formats
// TODO: figure out what to do with static files for the blog (CSS, JS, etc)
// TODO: improve the CSS files
// TODO: implement search


namespace BlogBuilder
{
    class Program
    {
        /// <summary>
        /// Usage: run this program in the root folder, where it can find the 'content', 'templates' 
        ///     and 'output' folders.
        /// </summary>
        static void Main(string[] args)
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Liquid.UseRubyDateFormat = true;

            var index = GenerateOutputs();
            PublishOutputs(index);
        }

        private static async void PublishOutputs(Index index)
        {
            FtpClient conn = await ConnectFtp(index);

            var fileList = await conn.GetListingAsync(index.FtpDir);
            foreach (var file in fileList)
            {
                //file.Modified
                //file.Name
                //file.Size
            }

            // List local and remote folders and compare
            // Upload changed/new files

        }

        private static async System.Threading.Tasks.Task<FtpClient> ConnectFtp(Index index)
        {
            FtpClient conn = new FtpClient();

            conn.Host = index.FtpHost;
            Console.WriteLine("Password for user {0} on ftp host {1}", index.FtpUser, index.FtpHost);
            var password = getPassword();
            conn.Credentials = new NetworkCredential(index.FtpUser, password);

            await conn.ConnectAsync();
            return conn;
        }

        private static Index GenerateOutputs()
        {
            var index = LoadIndex();

            OutputEntries(index);
            OutputFrontPage(index);
            OutputRSS(index);
            OutputArchives(index);

            return index;
        }

        private static Index LoadIndex()
        {
            var input = new StreamReader(Path.Combine(Globals.contentRoot, "index.yml"));
            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());

            var index = deserializer.Deserialize<Index>(input);
            return index;
        }

        private static void OutputEntries(Index index)
        {
            Template template = Template.Parse(Globals.EntryTemplate);

            foreach (var entry in index.Entries)
            {
                var output = template.Render(Hash.FromAnonymousObject(new { entry = entry, index = index }));

                EnsureDirectoryExists(entry.OutputFullPath);
                File.WriteAllText(entry.OutputFullPath, output);
            }
        }

        private static void OutputGeneric(Index index, string templatePath, string outputPath)
        {
            Template template = Template.Parse(templatePath);
            var output = template.Render(Hash.FromAnonymousObject(new { index = index }));
            File.WriteAllText(outputPath, output);
        }

        private static void OutputFrontPage(Index index)
        {
            OutputGeneric(index, Globals.FrontPageTemplate, index.FrontPageFullPath);
        }

        private static void OutputRSS(Index index)
        {
            OutputGeneric(index, Globals.RSSTemplate, index.RSSFullPath);
        }

        private static void OutputArchives(Index index)
        {
            OutputGeneric(index, Globals.ArchivesTemplate, index.ArchivesFullPath);
        }

        private static void EnsureDirectoryExists(string path)
        {
            new System.IO.FileInfo(path).Directory.Create();
        }

        public static SecureString getPassword()
        {
            SecureString pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }
    }

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
        public string FtpDir { get; set; }

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

    public class Entry : Drop
    {
        public string Title
        {
            get; set;
        }
        public DateTime Date { get; set; }

        [YamlMember(Alias = "src")]
        public string Source { get; set; }

        public string SourceFullPath
        {
            get
            {
                return Path.Combine(Globals.contentRoot, Source);
            }
        }

        public string Html
        {
            get
            {
                var mdContent = File.ReadAllText(SourceFullPath);

                var md = new MarkdownDeep.Markdown();
                md.ExtraMode = false;
                md.SafeMode = false;
                string output = md.Transform(mdContent);
                return output;
            }
        }

        public string WebRelativePath
        {
            get
            {
                return Path.Combine(@"\archives\", Source).Replace(".md", ".html").Replace(@"\", @"/");
            }
        }

        public string OutputFullPath
        {
            get
            {
                return Path.Combine(Globals.outputRoot, @"archives\", Source).Replace(".md", ".html");
            }
        }
    }

    public class Globals
    {
        public static string contentRoot = @"content\";
        public static string templateRoot = @"templates\";
        public static string staticRoot = @"static\";
        public static string outputRoot = @"output\";

        public static string FrontPageTemplate
        {
            get
            {
                return File.ReadAllText(Path.Combine(templateRoot, "index.template"));
            }
        }
        public static string EntryTemplate
        {
            get
            {
                return File.ReadAllText(Path.Combine(templateRoot, "entry.template"));
            }
        }

        public static string RSSTemplate
        {
            get
            {
                return File.ReadAllText(Path.Combine(templateRoot, "rss.template"));
            }
        }
        public static string ArchivesTemplate
        {
            get
            {
                return File.ReadAllText(Path.Combine(templateRoot, "archives.template"));
            }
        }
    }
}
