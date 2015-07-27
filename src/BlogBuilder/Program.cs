using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// TODO: fix date formats in RSS

namespace BlogBuilder
{
    class Program
    {
        public static string contentRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\content\";
        public static string templateRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\templates\";
        public static string staticRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\static\";
        public static string outputRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\output\";

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

        static void Main(string[] args)
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Liquid.UseRubyDateFormat = true;

            var index = LoadIndex();

            index.OutputEntries();
            index.OutputFrontPage();
            index.OutputRSS();
        }

        private static Index LoadIndex()
        {
            var input = new StringReader(File.ReadAllText(@"C:\Users\Julien\Documents\GitHubVisualStudio\blog\content\index.yml"));

            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());

            var index = deserializer.Deserialize<Index>(input);
            return index;
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

        // TODO: sort entries when set
        private List<Entry> entries;
        public List<Entry> Entries
        {
            get { return entries; }
            set { entries = value.OrderByDescending(o => o.Date).ToList(); }
        }

        public void OutputEntries()
        {
            Template template = Template.Parse(Program.EntryTemplate);

            foreach (var entry in Entries)
            {
                // TODO: check if content is newer and output should be deleted
                var output = template.Render(Hash.FromAnonymousObject(new { current = entry, index = this }));

                EnsureDirectoryExists(entry.OutputFullPath);
                File.WriteAllText(entry.OutputFullPath, output);
            }
        }

        public void OutputFrontPage()
        {
            Template template = Template.Parse(Program.FrontPageTemplate);
            var output = template.Render(Hash.FromAnonymousObject(new { index = this }));
            File.WriteAllText(FrontPageFullPath, output);
        }

        public string FrontPageFullPath
        {
            get
            {
                return Path.Combine(Program.outputRoot, "index.html");
            }
        }

        public void OutputRSS()
        {
            Template template = Template.Parse(Program.RSSTemplate);
            var output = template.Render(Hash.FromAnonymousObject(new { index = this }));
            File.WriteAllText(RSSFullPath, output);
        }

        public string RSSFullPath
        {
            get
            {
                return Path.Combine(Program.outputRoot, "index.rss");
            }
        }

        public void EnsureDirectoryExists(string path)
        {
            new System.IO.FileInfo(path).Directory.Create();
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
                return Path.Combine(Program.contentRoot, Source);
            }
        }

        public string MD
        {
            get
            {
                return File.ReadAllText(SourceFullPath);
            }
        }

        public string Html
        {
            get
            {
                var md = new MarkdownDeep.Markdown();
                md.ExtraMode = false;
                md.SafeMode = false;
                string output = md.Transform(MD);
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
                return Path.Combine(Program.outputRoot, @"archives\", Source).Replace(".md", ".html");
            }
        }

    }
}
