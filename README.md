# WarframeMarketClient
The WarframeMarketClient allows you to use [Warframe.Market](http://warframe.market) without having to keep the browser open.

## Features
### What it can do

 - Automatically sets your status to ingame when you start warframe, and back offline close warframe 
 - sets your status to offline when you are afk (longer than 5 minutes)
 - Chat 
 - Change your orders, like mark them sold, change listings or add new listings

### What it can **not** do
 
 - Search for offers on the market

## Requirements

[.Net4.6] (https://www.microsoft.com/de-DE/download/details.aspx?id=48137) or later

## [Download] (https://github.com/Versalkul/WarframeMarketClient/releases)

## How to use the client
You need to install .Net 4.6 to use this program, it will not work without! 

Just start the Executable file, like a portable program.  
Before you can use the program you need to authenticate yourself.  
Because you wouldnt want to give a program your username and password (looking at the people using steam to authenticate), im using the session-cookie instead.  
The session-cookie is what your browser uses to "remember you".  

## How to get your session cookie



## How to Update

This program does **not** update itsself.  
If an update is available it will display a update button which will link to this page, where you can download the new version.


## How to invalidate your session token
The program will offer to invalidate your session-cookie when removing the files it created (button in settings tab). (anmerkung an mich: todo).
If your browser still has this cookie it will activate the same cookie when you log in again with the cookie.
If you want a new cookie delete your cookie in the browser.

## Licence
