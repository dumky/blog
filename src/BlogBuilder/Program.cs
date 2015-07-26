using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BlogBuilder
{
    class Program
    {
        public static string contentRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\content\";
        public static string templateRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\templates\";
        public static string staticRoot = @"C:\Users\Julien\Documents\GitHubVisualStudio\blog\static\";

        public static string FrontPageTemplate
        {
            get
            {
                // TODO: add caching
                return File.ReadAllText(Path.Combine(templateRoot, "index.template"));
            }
        }

        static void Main(string[] args)
        {

            var index = LoadIndex();

            index.OutputEntries();
        }

        private static Index LoadIndex()
        {
            var input = new StringReader(File.ReadAllText(@"C:\Users\Julien\Documents\GitHubVisualStudio\blog\content\index.yml"));

            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());

            var index = deserializer.Deserialize<Index>(input);
            return index;
        }
    }

    public class Index
    {
        [YamlMember(Alias = "blog-title")]
        public string BlogTitle { get; set; }

        [YamlMember(Alias = "blog-author")]
        public string BlogAuthor { get; set; }

        [YamlMember(Alias = "blog-url")]
        public string BlogUrl { get; set; }

        public List<Entry> Entries
        {
            get; set;
        }

        public void OutputEntries()
        {
            Template template = Template.Parse(Program.FrontPageTemplate);
            var output = template.Render(Hash.FromAnonymousObject(new { name = "tobi" }));

        }

        public void OutputFrontPage()
        {

        }

        public void OutputRSS()
        {

        }
    }

    public class Entry
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

    }
}
