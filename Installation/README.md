# Raspberry IR Blaster Installation

These steps will store the config in `/etc/RaspberryIRBlaster/`, the binary files in `/usr/local/bin/RaspberryIRBlaster/` and the systemd service will be called `irblaster.service`. None of these names are fixed, any of them can be changed without re-building the application, so long as they are changed consistently for all steps in this file.

First build the server and RemoteBuilder applications. There are publish profiles for building with Visual Studio targeting the Raspberry Pi.
After building the applications copy them to `/usr/local/bin/RaspberryIRBlaster/RemoteBuilder` and `/usr/local/bin/RaspberryIRBlaster/Server/` on the Pi.

Make the applications executable.
```
sudo chmod 755 /usr/local/bin/RaspberryIRBlaster/Server/RaspberryIRBlaster.Server
sudo chmod 755 /usr/local/bin/RaspberryIRBlaster/RemoteBuilder/RaspberryIRBlaster.RemoteBuilder
```

You can optionally add an entry to your `.bash_aliases` file to make calling the RemoteBuilder easier.
```
alias irbrb='/usr/local/bin/RaspberryIRBlaster/RemoteBuilder/RaspberryIRBlaster.RemoteBuilder --config=/etc/RaspberryIRBlaster/'
```

Create the config directory and allow your user (usually `pi`) to write to the remotes folder.
```
sudo mkdir /etc/RaspberryIRBlaster/Remotes
sudo chgrp pi /etc/RaspberryIRBlaster/Remotes
sudo chmod 775 /etc/RaspberryIRBlaster/Remotes
```
Upload the `General.json` file from this directory to `/etc/RaspberryIRBlaster/General.json`. Make any changes you want to make to this file.
 * For `ListenAtUrl` you can use values like `http://*:8000` to listen on all interfaces on port 8000 or you can use `http://localhost:8000` to only listen on the IPv4 and IPv6 loopback addresses. Note this is ignored by the server when using systemd socket activation, but you should still set it since the RemoteBuilder will still use it and so will the server if you start it at the console.
 * The `IRRXDevice` and `IRTXDevice` parameters only need to be specified if you have more than one IR device of a particular type. So if you have one TX and one RX device you can leave these at null and the application will auto select the device. If however you have multiple transmitters and/or multiple receivers you will need to use a value like `/dev/lirc5`.

## systemd

This section is optional since you can simply run the IR Blaster server at the console if you want, but it is often useful to have systemd run it in the background instead. **Choose either the socket activation method -OR- the standalone method.**

### systemd - Socket Activation

Create the file `/etc/systemd/system/irblaster.service` from one of the templates in this folder.

Create the file `/etc/systemd/system/irblaster.socket` from the template in this folder. Change the port number if you want, it should be the same port number as what is in `ListenAtUrl`.

Run `sudo systemctl daemon-reload` to have systemd read the new files.

Run `sudo systemctl enable irblaster.socket` to make the new socket start at boot (optional).

### systemd - Standalone

Create the file `/etc/systemd/system/irblaster.service` from one of the templates in this folder. Comment out the `PrivateNetwork=true` line (only in the sandboxed template) and uncomment the `WantedBy` line in the `Install` section.

Run `sudo systemctl daemon-reload` to have systemd read the new files.

Run `sudo systemctl enable irblaster.service` to make the new service start at boot (optional).
