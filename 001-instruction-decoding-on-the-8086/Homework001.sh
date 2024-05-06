#!/usr/bin/env sh

if [ "$#" -eq 1 ]; then
    ./Decoder/bin/x64/Release/net8.0/linux-x64/Homework.001.Decoder $1
elif [ "$#" -eq 2 ]; then
    ./Decoder/bin/x64/Release/net8.0/linux-x64/Homework.001.Decoder $1 $2
else
    echo "Illegal number of parameters"
fi
