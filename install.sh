#!/usr/bin/env bash

curl -fsSL -O https://github.com/jonlabelle/secure-file/archive/master.zip
unzip -q -o secure-file.zip -d appveyor-tools
chmod +x ./appveyor-tools/secure-file
rm secure-file.zip
