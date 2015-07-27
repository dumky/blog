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

            OutputEntries(index);
            OutputFrontPage(index);
            OutputRSS(index);
        }

        private static Index LoadIndex()
        {
            var input = new StringReader(File.ReadAllText(@"C:\Users\Julien\Documents\GitHubVisualStudio\blog\content\index.yml"));

            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());

            var index = deserializer.Deserialize<Index>(input);
            return index;
        }

        private static void OutputEntries(Index index)
        {
            Template template = Template.Parse(Program.EntryTemplate);

            foreach (var entry in index.Entries)
            {
                var output = template.Render(Hash.FromAnonymousObject(new { entry = entry, index = index }));

                EnsureDirectoryExists(entry.OutputFullPath);
                File.WriteAllText(entry.OutputFullPath, output);
            }
        }

        private static void OutputFrontPage(Index index)
        {
            Template template = Template.Parse(Program.FrontPageTemplate);
            var output = template.Render(Hash.FromAnonymousObject(new { index = index }));
            File.WriteAllText(index.FrontPageFullPath, output);
        }

        private static void OutputRSS(Index index)
        {
            Template template = Template.Parse(Program.RSSTemplate);
            var output = template.Render(Hash.FromAnonymousObject(new { index = index }));
            File.WriteAllText(index.RSSFullPath, output);
        }

        private static void EnsureDirectoryExists(string path)
        {
            new System.IO.FileInfo(path).Directory.Create();
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
                return Path.Combine(Program.outputRoot, "index.html");
            }
        }

        public string RSSFullPath
        {
            get
            {
                return Path.Combine(Program.outputRoot, "index.rss");
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
