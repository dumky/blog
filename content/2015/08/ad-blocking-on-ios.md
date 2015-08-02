
Mobile browsers don't support extensions like their desktop counterparts and most don't have an ad blocker built-in. But it turns out that iOS (and probably Android and Windows Phone) supports good old "proxy auto-configuration" (PAC).  
PAC is a mechanism by which the operating system uses a simple script file to choose when to use a proxy. The script receives the host and url of each web request and tells the operating system whether to connect directly (as normal) or instead use a proxy. The trick is to use a blackhole proxy (which returns no content) for urls that are recognized as advertisement, based on a list of known domains and url patterns.  

So I dug up an existing ad-blocking PAC file which seems somehow up-to-date (from [John LoVerso](http://www.schooner.com/~loverso/no-ads/)), configured the blackhole proxy to Google's DNS server (8.8.8.8 port 53), and updated my iOS wifi settings to point to it. I tested in Chrome on iPhone and iPad and this method seems to work. 

You can do the same and configure your mobile device to use the PAC file. 

TODO: can ads in apps be blocked too?  
TODO: can the PAC file be hosted inside an installed app?


http://blog.monstuff.com/ad-block-pac.js


# Security considerations

Configuring a PAC file into your operating system can be dangerous. If the PAC file is adversarial or was modified by a hacker, the attacker could send parts or all of your web traffic through a proxy of his choice.  
You could use your own copy of this file, but you'd have to host your copy securely.  

The solution I adopted is to host the PAC file on a CDN of static files. The CDN does not allow updating an existing file in-place, it only allows adding new version. The only downside is that you'll have to update your settings if you want to use a newer version of the file.    
Also, it is good to review the contents of the PAC file. It is pretty easy to read and it should be clear that it is using Google's DNS servers as blackhole proxies.

Btw, this idea comes from the [FAQ of Weblock](https://www.weblockapp.com/faq/#question-7), an iOS app which generates PAC files. The FAQ offers a good explanation for this choice:

> 1. iOS requires dummy proxy to be a valid IP address accepting connections (so it's not possible to use local IP address of your device, since there is no open port to connect to). 
> 2. It's really responsive, fast and stable anywhere in the world. 
> 3. It's NOT ABLE to handle HTTP/HTTPS traffic, since it's a DNS server (it handles an entirely different protocol). It immediately closes the HTTP/HTTPS connection (which is perfect!). 
> 4. It's widely recognised and well known IP, so you don't have to be concerned about your privacy. We're quite sure Google is not logging all web connection attempts made while blocking content from your device, since this dummy proxy is actually a DNS server supporting a different kind of requests. 
