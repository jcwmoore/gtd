#!/usr/bin/env sh

sudo systemctl stop gtd.service

cd /var/www/gtd
tar zxvf package.tgz -C .
chmod 770 ./Gtd.Web
chown -R www-data ./*
rm ./package.tgz

sudo systemctl start gtd.service