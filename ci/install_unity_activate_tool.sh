#!/usr/bin/env bash

curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.3/install.sh | bash
source ~/.nvm/nvm.sh
nvm install 16.19.0
npm install -g npm@latest
nvm -v
node -v
npm -v
npm install -g unity-activate
apt-get update
apt-get install -y libgbm-dev
unity-editor -batchmode -createManualActivationFile

ls | grep \.alf$ > out.txt


file=$(cat out.txt)

unity-activate --username $UNITY_USERNAME --password $UNITY_PASSWORD $file