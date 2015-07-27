I came back from vacation with the intent to blog more regularly. As an efficient procrastinator, I decided this would involve moving away from MovableType first ;-)
I don't like setup and I don't like administering SQL instances. I prefer static files (easier to backup and migrate to new hosts) and enjoy the idea of managing my content in GitHub (versioning, access control, Markdown preview). 
There are a few [flat-file blogging platforms](http://www.freshtechtips.com/2014/01/flat-file-blogging-software.html), but none quite fit the bill.

The solution I built yesterday is simple: put content in Markdown files, put some metadata about the entry in a YAML file, generate the blog files from that and simple templates.
The code becomes really simple by leveraging libraries like MarkdownDeep, YamlDotNet, and DotLiquid (for templates). I also intend to refresh my templates and stylesheets, although that will be more work.

The code for the tool and content for this blog are hosted on this [GitHub repo](https://github.com/dumky/blog).
