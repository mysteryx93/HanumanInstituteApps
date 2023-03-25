#!/bin/bash

./bass-download.sh bassenc
./bass-download.sh bassenc_mp3
./bass-download.sh bassenc_flac
./bass-download.sh bassenc_ogg
./bass-download.sh bassenc_opus
./bass-download.sh bassenc_aac stuff .


archive="temp.zip"

lib="bassenc_aac"
wget -O "$archive" "https://www.un4seen.com/stuff/bassenc_aac-linux.zip"
unzip -o -j "$archive" "x64/lib$lib.so" -d "linux-x64"

wget -O "$archive" "https://www.un4seen.com/stuff/bassenc_aac-linux-arm.zip"
unzip -o -j "$archive" "aarch64/lib$lib.so" -d "linux-arm64"

rm $archive
