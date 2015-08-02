
Mobile browsers don't support extensions like their desktop counterparts and most don't have an ad blocker built-in. But it turns out that iOS (and probably Android and Windows Phone) supports good old "proxy auto-configuration" (PAC).  
PAC is a mechanism by which a simple script file can tell the operating system when to use a proxy. The script can look at the host and url of the request and tell the operating system to connect directly or instead use a proxy. The trick is to use a blackhole proxy, which returns no content, for urls that are recognized as advertisement.  

So I dug up an existing ad-blocking PAC files which seems somehow up-to-date (from [John LoVerso](http://www.schooner.com/~loverso/no-ads/)), configured the blackhole proxy to Google's DNS server (8.8.8.8 port 53), and updated my iOS wifi settings to point to the PAC file.  

You can do the same and configure your mobile device to use the PAC file. 


http://blog.monstuff.com/ad-block-pac.js


# Security considerations



This idea comes from the [FAQ of Weblock](https://www.weblockapp.com/faq/#question-7), an iOS app which generates PAC files). The FAQ offers a good explanation for this choice:

> 1. iOS requires dummy proxy to be a valid IP address accepting connections (so it's not possible to use local IP address of your device, since there is no open port to connect to). 
> 2. It's really responsive, fast and stable anywhere in the world. 
> 3. It's NOT ABLE to handle HTTP/HTTPS traffic, since it's a DNS server (it handles an entirely different protocol). It immediately closes the HTTP/HTTPS connection (which is perfect!). 
> 4. It's widely recognised and well known IP, so you don't have to be concerned about your privacy. We're quite sure Google is not logging all web connection attempts made while blocking content from your device, since this dummy proxy is actually a DNS server supporting a different kind of requests. 
