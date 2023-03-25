#!/bin/bash
if [ -z "$1" ]; then
  echo "Syntax: bass-download Lib [path] [sufix]"
  exit 1
fi
lib=$1
path=$2
if [ -z "$2" ]; then
  path="files/"
fi
sufix=$3
if [ -z "$3" ]; then
  sufix="24"
fi
if [ $sufix="." ]; then
  sufix=""
fi

archive="temp.zip"

# Windows
wget -O "$archive" "https://www.un4seen.com/${path}/${lib}${sufix}.zip"
unzip -o -j "$archive" "$lib.dll" -d "win-x86"
unzip -o -j "$archive" "x64/$lib.dll" -d "win-x64"

# Linux
wget -O "$archive" "https://www.un4seen.com/${path}/${lib}${sufix}-linux.zip"
unzip -o -j "$archive" "libs/x86_64/lib$lib.so" -d "linux-x64"
unzip -o -j "$archive" "libs/aarch64/lib$lib.so" -d "linux-arm64"

# OSX
wget -O "$archive" "https://www.un4seen.com/${path}/${lib}${sufix}-osx.zip"
unzip -o -j "$archive" "lib$lib.dylib" -d "osx-x64"

rm $archive
