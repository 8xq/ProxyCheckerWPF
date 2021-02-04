# ProxyCheckerWPF
A very basic C# proxy checker that will filter the working proxies from the dead proxies , if the proxy is valid it will also attempt to return the "ISP" and "region" of the proxy. As you will see below the design is very basic and its just the default XAML controls available for use within WPF , the reason for this was to learn more about Xaml and learn some of the differences.  

![Alt text](https://i.imgur.com/92lhbP0.png "GUI")



# Setup (project source)
```
* Download the project files and open in your IDE
* Add dependencies if they are missing (Leaf xnet , Newtonsoft)
* Now build the source :)
```

# Proxy checker info
```
* Checks both http/s , socks4 & socks5 proxy types
* Customizable timeout (MS) (within project source)
* Customizable thread count (within project source)
* Save proxies to a text file and remove all of the dead proxies
* See proxies in a listbox + server information
```

# Future updates
```
1 - Tidy up the code a little (simplify the multi threading)
2 - Add option to test proxy against a specific site
3 - Add option to save proxies with ISP information or filter
4 - Write working proxies to file instantly 
5 - Add options.json for settings
```

# Options
```
* Threads (1-2000) (reocmmended is 250 threads)
* Timeout (1,000 MS = 1 seconds)
```

![Alt text](https://i.imgur.com/9UUTaC2.png "Settings")

# Dependencies
[Leaf Xnet](https://github.com/csharp-leaf/Leaf.xNet) \
[Newtonsoft](https://www.newtonsoft.com/)

```
Admin@hvh.site
```
