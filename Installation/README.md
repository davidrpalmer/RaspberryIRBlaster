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
 * For `ListenAtUrl` you can use values like `http://*:8000` to listen on all interfaces on port 8000 or you can use `http://localhost:8000` to only listen on the IPv4 and IPv6 loopback addresses.
 * The `IRRXDevice` and `IRTXDevice` parameters only need to be specified if you have more than one IR device of a particular type. So if you have one TX and one RX device you can leave these at null and the application will auto select the device. If however you have multiple transmitters and/or multiple receivers you will need to use a value like `/dev/lirc5`.

Create the file `/etc/systemd/system/irblaster.service` from one of the templates in this folder. Run `sudo systemctl daemon-reload` to have systemd read it.
Run `sudo systemctl enable irblaster.service` to make the new service start at boot (optional).

