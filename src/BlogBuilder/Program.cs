using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// TODO: fix date formats in RSS
// TODO: figure out what to do with static files for the blog (CSS, JS, etc)
// TODO: improve the CSS files

namespace BlogBuilder
{
    class Program
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

        /// <summary>
        /// Usage: run this program in the root folder, where it can find the 'content', 'templates' 
        ///     and 'output' folders.
        /// </summary>
        static void Main(string[] args)
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Liquid.UseRubyDateFormat = true;

            GenerateOutputs();
        }

        private static void GenerateOutputs()
        {
            var index = LoadIndex();

            OutputEntries(index);
            OutputFrontPage(index);
            OutputRSS(index);
            OutputArchives(index);
        }

        private static Index LoadIndex()
        {
            var input = new StreamReader(Path.Combine(Program.contentRoot, "index.yml"));
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

        private static void OutputGeneric(Index index, string templatePath, string outputPath)
        {
            Template template = Template.Parse(templatePath);
            var output = template.Render(Hash.FromAnonymousObject(new { index = index }));
            File.WriteAllText(outputPath, output);
        }

        private static void OutputFrontPage(Index index)
        {
            OutputGeneric(index, FrontPageTemplate, index.FrontPageFullPath);
        }

        private static void OutputRSS(Index index)
        {
            OutputGeneric(index, RSSTemplate, index.RSSFullPath);
        }

        private static void OutputArchives(Index index)
        {
            OutputGeneric(index, ArchivesTemplate, index.ArchivesFullPath);
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

        public string ArchivesFullPath
        {
            get
            {
                return Path.Combine(Program.outputRoot, "archives.html");
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
                return Path.Combine(Program.outputRoot, @"archives\", Source).Replace(".md", ".html");
            }
        }

    }
}
