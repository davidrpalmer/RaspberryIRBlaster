# Raspberry IR Blaster
This is an ASP.NET Core REST server that runs on a Raspberry Pi to allow a remote client to blast out pre-defined infrared messages by making HTTP requests.

There is no client application in this project, however there is a client library ([RaspberryIRBlaster.Client](RaspberryIRBlaster.Client)) which may be used to help implement your own client application in either .NET Core or .NET Framework.

## Prerequisites
 * This application uses the [RaspberryIRDotNet](https://github.com/davidrpalmer/RaspberryIRDotNet) library so all the prerequisites of that project also apply here.
 * [ASP.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on the Pi

## Components
 * `RaspberryIRBlaster.Server` is the ASP.NET server. It can run on the console or as a systemd service.
 * `RaspberryIRBlaster.RemoteBuilder` is a command line tool for recording IR signals and managing config files for the server to use.
 * `RaspberryIRBlaster.Client` is an optional .NET Standard class library that can be used to help write your own client application to control the IR blaster server.

## Getting Started

In this application you will see the term "unit". This describes the base unit of time the infrared LED is on or off for (a PULSE or a SPACE). For example a remote may encode a binary bit 0 as one unit on, one unit off and a binary bit 1 as three units on one unit off. If the unit duration is 100μs then bit 0 would be 100μs PULSE, followed by 100μs SPACE and bit 1 would be 300μs PULSE followed by 100μs SPACE.

See the [Installation](Installation) directory for install instructions and template files.

Once installed you need to record some IR signals from your remote control so you can play them back later. To do this run the RemoteBuilder app (the paths may vary depending on where you have installed it to).
```
/usr/local/bin/RaspberryIRBlaster/RemoteBuilder/RaspberryIRBlaster.RemoteBuilder --config=/etc/RaspberryIRBlaster/
```
First it's a good idea to check your IR receiver is working. Select the "View Raw IR" option, then press buttons on your remote and confirm they are visible on screen.

Next you need to create a profile for your remote. Once done the remote has been saved to a JSON file, use the "List remotes" option to see exactly where. You may modify this JSON file manually or using the RemoteBuilder tool. You can now use the RemoteBuilder tool to modify the remote profile to record IR signals into it. Once you have your remote profile setup how you want you must either restart the IR Blaster server or clear the cache using the option from RemoteBuilder's main menu.

With the remote profiles setup and the IR Blaster server running you are now ready to blast out some IR signals. An example of how to send signals with curl:
```
curl -i -H "Content-Type: application/json" -X POST --data "{""Items"":[ {""Type"":""SendMessage"", ""Data"": ""PanasonicTV.ONE""}, {""Type"":""Sleep"", ""Data"": 750},{""Type"":""SendMessage"", ""Data"": ""PanasonicTV.TWO""}]}" http://ip-of-your-pi:8000/IRTX/SendBatch
```
The above will send button "ONE" of remote "PanasonicTV", wait for 750ms, then send button TWO.
The HTTP request won't complete until the IR message has been sent. If you want to abort a long running batch of IR messages then use:
```
curl -i -H "Content-Type: application/json" -X POST --data "" http://ip-of-your-pi:8000/IRTX/Abort
```

Once you have IR signals set up you need a client to control the IR Blaster server. You can continue to use curl as the client if you want, or you may prefer to write your own app. If your own app will be written in .NET then there is the RaspberryIRBlaster.Client to aid you.
