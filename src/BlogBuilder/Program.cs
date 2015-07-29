using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Net.FtpClient.Async;
using System.Security;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// TODO: fix date formats
// TODO: figure out what to do with static files for the blog (CSS, JS, etc)
// TODO: improve the CSS files
// TODO: implement search


namespace BlogBuilder
{
    class BlogBuilder
    {
        /// <summary>
        /// Usage: run this program in the root folder, where it can find the 'content', 'templates' 
        ///     and 'output' folders.
        /// </summary>
        static void Main(string[] args)
        {
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Liquid.UseRubyDateFormat = true;

            var blogBuilder = new BlogBuilder();
            var index = blogBuilder.GenerateOutputs();
            blogBuilder.PublishOutputs(index).GetAwaiter().GetResult();
        }

        private static string TrimOutputPrefix(string path)
        {
            return path.Replace(Globals.outputRoot, "");
        }

        private async Task PublishOutputs(Index index)
        {
            FtpClient conn = await ConnectFtp(index);

            var localFolders = new string[] { Globals.outputRoot }.Concat(ListSubDirectories(Globals.outputRoot)).ToArray();
            foreach (var localFolder in localFolders)
            {
                await PublishDelta(conn, index, localFolder);
            }
        }

        private async Task PublishDelta(FtpClient conn, Index index, string localFolder)
        {
            var localFiles = Directory.EnumerateFiles(localFolder);
            if (localFiles.Count() == 0)
            {
                return;
            }

            var remotePath = Path.Combine(index.FtpDir, TrimOutputPrefix(localFolder));
            FtpListItem[] remoteFiles;
            if (await conn.DirectoryExistsAsync(remotePath))
            {
                remoteFiles = await conn.GetListingAsync(remotePath);
            }
            else
            {
                await conn.CreateDirectoryAsync(remotePath);
                remoteFiles = new FtpListItem[0];
            }

            var list = remoteFiles
                .Where(f => f.Type == FtpFileSystemObjectType.File)
                .FullOuterJoin(localFiles, r => r.Name, l => new FileInfo(l).Name, (r, l, _) => new { remote = r, local = l });

            foreach (var pair in list)
            {
                if (ShouldUpload(pair.remote, pair.local))
                {

                    // Upload changed/new files
                    Console.WriteLine("Should upload {0}", pair.local);
                    using (var fileStream = File.OpenRead(pair.local))
                    {
                        using (var ftpStream = await conn.OpenWriteAsync(Path.Combine(remotePath, new FileInfo(pair.local).Name)))
                        {
                            fileStream.CopyTo(ftpStream);
                        }
                    }
                }
            }
        }

    private static bool ShouldUpload(FtpListItem remote, string local)
        {
            var shouldUpload = false;
            if (remote == null)
            {
                shouldUpload = true;
            }
            else
            {
                if (local != null)
                {
                    FileInfo localInfo = new FileInfo(local);
                    if (//localInfo.Length != remote.Size ||
                        localInfo.LastWriteTime > remote.Modified)
                    {
                        shouldUpload = true;
                    }
                }
            }
            return shouldUpload;
        }



        private async Task<FtpClient> ConnectFtp(Index index)
        {
            FtpClient conn = new FtpClient();

            conn.Host = index.FtpHost;
            Console.WriteLine("Password for user {0} on ftp host {1}", index.FtpUser, index.FtpHost);
            //var password = getPassword();
            conn.Credentials = new NetworkCredential(index.FtpUser, "testtest");

            await conn.ConnectAsync();
            return conn;
        }

        private Index GenerateOutputs()
        {
            var index = LoadIndex();

            OutputEntries(index);
            OutputFrontPage(index);
            OutputRSS(index);
            OutputArchives(index);

            return index;
        }

        private Index LoadIndex()
        {
            var input = new StreamReader(Path.Combine(Globals.contentRoot, "index.yml"));
            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());

            var index = deserializer.Deserialize<Index>(input);
            return index;
        }

        private void OutputEntries(Index index)
        {
            Template template = Template.Parse(Globals.EntryTemplate);

            foreach (var entry in index.Entries)
            {
                if (ShouldSkipEntry(entry))
                {
                    continue;
                }

                var output = template.Render(Hash.FromAnonymousObject(new { entry = entry, index = index }));

                EnsureDirectoryExists(entry.OutputFullPath);
                File.WriteAllText(entry.OutputFullPath, output);
            }
        }

        private bool ShouldSkipEntry(Entry entry)
        {
            var inputInfo = new FileInfo(entry.SourceFullPath);

            if (!File.Exists(entry.OutputFullPath))
            {
                return false;
            }

            var outputInfo = new FileInfo(entry.OutputFullPath);

            if (outputInfo.LastWriteTime > inputInfo.LastWriteTime)
            {
                Console.WriteLine("Skipping entry {0}", entry.Source);
                return true;
            }
            return false;
        }

        private void OutputGeneric(Index index, string templatePath, string outputPath)
        {
            Template template = Template.Parse(templatePath);
            var output = template.Render(Hash.FromAnonymousObject(new { index = index }));
            File.WriteAllText(outputPath, output);
        }

        private void OutputFrontPage(Index index)
        {
            OutputGeneric(index, Globals.FrontPageTemplate, index.FrontPageFullPath);
        }

        private void OutputRSS(Index index)
        {
            OutputGeneric(index, Globals.RSSTemplate, index.RSSFullPath);
        }

        private void OutputArchives(Index index)
        {
            OutputGeneric(index, Globals.ArchivesTemplate, index.ArchivesFullPath);
        }

        private void EnsureDirectoryExists(string path)
        {
            new System.IO.FileInfo(path).Directory.Create();
        }

        public SecureString getPassword()
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

        public IEnumerable<string> ListSubDirectories(string path)
        {
            foreach (var folder in Directory.EnumerateDirectories(path))
            {
                yield return folder;

                foreach (var subFolder in ListSubDirectories(folder))
                {
                    yield return subFolder;
                }
            }
        }


    }

    public static class LinqExtension
    {

        internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TKey> selectKeyA,
            Func<TB, TKey> selectKeyB,
            Func<TA, TB, TKey, TResult> projection,
            TA defaultA = default(TA),
            TB defaultB = default(TB),
            IEqualityComparer<TKey> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TKey>.Default;
            var alookup = a.ToLookup(selectKeyA, cmp);
            var blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                       from xa in alookup[key].DefaultIfEmpty(defaultA)
                       from xb in blookup[key].DefaultIfEmpty(defaultB)
                       select projection(xa, xb, key);

            return join;
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
