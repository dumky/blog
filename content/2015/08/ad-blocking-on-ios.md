
Screenshots

John LoVerso.  
http://www.schooner.com/~loverso/no-ads/

Updated using Google's DNS servers (8.8.8.8 port 53) as blackhole. This idea comes from the [FAQ of Weblock](https://www.weblockapp.com/faq/#question-7), an iOS app which generates PAC files). The FAQ offers a good explanation for this choice:

> 1. iOS requires dummy proxy to be a valid IP address accepting connections (so it's not possible to use local IP address of your device, since there is no open port to connect to). 
> 2. It's really responsive, fast and stable anywhere in the world. 
> 3. It's NOT ABLE to handle HTTP/HTTPS traffic, since it's a DNS server (it handles an entirely different protocol). It immediately closes the HTTP/HTTPS connection (which is perfect!). 
> 4. It's widely recognised and well known IP, so you don't have to be concerned about your privacy. We're quite sure Google is not logging all web connection attempts made while blocking content from your device, since this dummy proxy is actually a DNS server supporting a different kind of requests. 


http://blog.monstuff.com/ad-block-pac.js
