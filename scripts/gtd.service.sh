#!/usr/bin/env sh

# run this command manually once to initialize the setup the droplet to host the gtd site and enabling automatic deployment

sudo cp ./gtd.service /etc/systemd/system
sudo systemctl enable gtd.service

mkdir /var/www/gtd

