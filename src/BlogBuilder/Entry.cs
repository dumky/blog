using DotLiquid;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace BlogBuilder
{
    public class Entry : Drop
    {
        public string Title
        {
            get; set;
        }
        public DateTime Date { get; set; }

        [YamlMember(Alias = "src")]
        public string Source { get; set; }

        [YamlMember(Alias = "draft")]
        public bool Draft { get; set; }

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

}
