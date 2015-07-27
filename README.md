# Curiosity is Bliss
Julien Couvreur's [blog](http://blog.monstuff.com)

Content is mastered here, processed through simple templates and published on my site as static files.

The BlogBuilder program works as follows:

1. You give it a folder, containing sub-folders for 'content', 'templates' and  'output'
2. It finds 'index.yml' in the 'content' folder. This is a YAML-formatted file with blog metadata and the list of entries.
3. For each entry in the index file, it finds a Markdown file which contains the body of a blog post.
4. Three DotLiquid templates from the 'templates' folders are used in addition to Markdown processing to generate the output files.
5. The output folder will hold an index.html and an index.rss files, as well as html files for each entry under the 'archives' folder (with sub-folders matching the original content structure where the Markdown file was found).
 

