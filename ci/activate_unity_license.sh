#!/usr/bin/env bash

unity-editor -batchmode -createManualActivationFile

ls | grep \.alf$ > out.txt


file=$(cat out.txt)

unity-activate --username $UNITY_USERNAME --password $UNITY_PASSWORD $file.alf