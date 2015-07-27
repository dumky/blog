I came back from vacation with the intent to blog more regularly (in particular, I will start a series on the fundamentals of computing). Being an efficient procrastinator, I decided this would involve moving away from MovableType first ;-)

What tool to use? I don't like set up or administering SQL instances. I appreciate static files (easier to backup and migrate to new hosts) and the idea of managing my content in git/GitHub (versioning, access control, Markdown preview). There are a few [flat-file blogging platforms](http://www.freshtechtips.com/2014/01/flat-file-blogging-software.html), but none quite fit the bill.

The solution I built yesterday is simple (every tool starts that way, doesn't it): put content in Markdown files, put some metadata about the entry in a YAML file, and generate the HTML and RSS files from that and Liquid templates. The editing can be done directly in GitHub. The publishing is done via FTP.

The code (not counting HTML templates or CSS) stays short by leveraging libraries like MarkdownDeep, YamlDotNet, DotLiquid and System.Net.FtpClient. Dynamic site functionality such as commenting and searching is outsourced, to Disqus and Google respectively. I intend to refresh the design and stylesheets for the site, as they are dating, but that will come later. 

The code for the tool and content for this blog are hosted on this [GitHub repo](https://github.com/dumky/blog).
