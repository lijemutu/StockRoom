# StockRoom
## StockRoom is a real time chatroom that provides stock data

This is a blazor application developed using .Net 7 with a WebApi service to fetch stock data from [https://stooq.com/q/?s=nflx.us](Stooq.com) in real time

## Features

* Real time chatroom, using SignalR Hubs and through websockets we connect client and blazor server
* Authentication via google, with google cloud system and credentials I implement a login system
* Live data via Stooq.com, a separate service handles the requests to Stooq API making it suitable for scaling if needed
* Built-in command in chatroom, using /stock={stock-code} the bot will answer with the stock data

## Deploy

Follow the steps for deploying to a server both applications:

* Publish the application from Visual Studio (right-click on the project and select publish)
* Choose deployment as Folder and select the folder will be published
* Configure deployment settings, with .net 7 framework
* Click the publish button
