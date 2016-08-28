# WarframeMarketClient <img src="https://raw.githubusercontent.com/Versalkul/WarframeMarketClient/master/Icon.ico" width="50" alt="MarframeMarketClient Icon" title="Our current Icon">
The WarframeMarketClient allows you to use [Warframe.Market](http://warframe.market) without having to keep the browser open.

## Features
### What it can do

 - Automatically sets your status to ingame when you start warframe, and back offline when you close warframe 
 - Sets your status to offline when you are afk (longer than 5 minutes)
 - Chat 
 - Change your orders, like mark them sold, change listings or add new listings

### What it can **not** do
 - **Change the Offline/Online switch in your browser**
 - Search for offers on the market

## Requirements

[.Net4.6] (https://www.microsoft.com/en-us/download/details.aspx?id=48130) or later (it is preinstalled on Windows 10)

## [Download] (https://github.com/Versalkul/WarframeMarketClient/releases/latest)
The executable downloads are in the release section (no need to clone or compile yourself ;) )

## How to use the client
You need to install .Net 4.6 to use this program, it will not work without! 

Just start the Executable file, like a portable program.  
Before you can use the program you need to authenticate yourself.  
Because you wouldnt want to give a program your username and password (looking at the people using steam to authenticate), I'm using the session-cookie instead.  
The session-cookie is what the website uses to "remember you".  
You need to obtain your cookie from the browser and copy-paste it in the session cookie textbox in the settings-tab and press save (or enter)  
The session cookie will be only recognized as valid if the session the cookie is representing is logged in!    

## How to get your session cookie
Usually you can find the cookie in your browser. Search for a cookie named *'session'* on warframe.market and copy its content.  
[How to get your session cookie in firefox] (http://imgur.com/7tamenv)  
[How to get your session cookie in chrome] (http://imgur.com/NddsKgc)  

**Don't share this code with anyone as it allows full access to your account (as long as you are logged in/ until you invalidate the cookie (see below))!**

## How to Update
This program does **not** update itsself.  
If an update is available it will display a update button which will link to this page.  
Just download the new version and replace the executable.

## How to invalidate your session cookie
The program will offer to invalidate your session-cookie (log you out) when removing the files it created (button in settings tab).  
If your browser still has this cookie it will **reactivate the same cookie** when you log in again.
If you want a new cookie delete your cookie in the browser while you are logged out.

## License
MIT Licence
