#!/bin/bash
if [ -z "$1" ]; then
  echo "Syntax: bass-download Lib [path] [sufix]"
  exit 1
fi
lib=$1
path="${2:-files}"
sufix="${3:-24}"
if [ "$sufix" == "." ]; then
  sufix=""
fi

archive="temp.zip"
RED='\033[0;31m' # Red Text
NC='\033[0m' # No Color

# Windows
url="https://www.un4seen.com/${path}/${lib}${sufix}.zip"
echo Windows: $url
if wget -q -O "$archive" "$url"; then
unzip -o -j "$archive" "$lib.dll" -d "win-x86"
unzip -o -j "$archive" "x64/$lib.dll" -d "win-x64"
else
echo -e "${RED}Url not found for ${lib}${NC}"
fi
echo

# Linux
url="https://www.un4seen.com/${path}/${lib}${sufix}-linux.zip"
echo Linux: $url
if wget -q -O "$archive" "$url"; then
unzip -o -j "$archive" "libs/x86_64/lib$lib.so" -d "linux-x64"
unzip -o -j "$archive" "libs/aarch64/lib$lib.so" -d "linux-arm64"
else
echo -e "${RED}Url not found for ${lib}${NC}"
fi
echo

# OSX
url="https://www.un4seen.com/${path}/${lib}${sufix}-osx.zip"
echo OSX: $url
if wget -q -O "$archive" "$url"; then
unzip -o -j "$archive" "lib$lib.dylib" -d "osx-x64"
else
echo -e "${RED}Url not found for ${lib}${NC}"
fi
echo

rm $archive
