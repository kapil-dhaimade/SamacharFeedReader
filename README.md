# Samachar Feed Reader

Samachar Feed Reader is a simple, free and open-source RSS and Atom feed reader created in .NET Framework. It is a Windows desktop application which runs as a system tray app and shows dialog to notify of new feeds.

Software Requirements:
	1. Microsoft .NET Framework 4.5.1

Installation Steps: 
	1. Just place this folder to the location from where you want to run the application. 
	2. Run the SamacharFeedReader.exe present in this folder.
	
Managing Feeds:
	1. Feed sources are maintain in an XML file named 'subscribedFeeds.txt' which is present in the same folder as the application EXE.
	2. Open the file in a text editor and add/remove a feed source. Application may need some time to start showing feeds from a newly added feed source. If you want to fetch the new feeds instantly, quit the application from system tray and restart it.
	3. The same file 'subscribedFeeds.txt' may also be accessed by the 'Manage Feeds...' menu item shown in the application's tray menu.
	
Settings:
	1. Feed fetch interval: By default, the application fetches new feeds every 15 minutes. You may change this by updating the 'feedFetchIntervalMins' in the 'SamacharFeedReader.exe.config' file present alongside the application EXE. It's recommended to restart the application if you edit the settings.
	
Known Issues:
	1. The 'Show All' window showing the feeds may sometimes be hidden behind other application windows and may not come to front. 
	
Website: https://github.com/kapil-dhaimade/SamacharFeedReader
