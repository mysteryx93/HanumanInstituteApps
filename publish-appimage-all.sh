if [ -z $1 ] ; then
  echo "You must specify version (eg: 2.1)" && exit 1;
fi

./publish-appimage -f Player432hz/publish-appimage.conf -y
cp Player432hz/bin/AppImages/Player432hz-x86_64.AppImage Publish/$1/Player432hz-$1_Linux_x64.AppImage

./publish-appimage -f Converter432hz/publish-appimage.conf -y
cp Converter432hz/bin/AppImages/Converter432hz-x86_64.AppImage Publish/$1/Converter432hz-$1_Linux_x64.AppImage

./publish-appimage -f PowerliminalsPlayer/publish-appimage.conf -y
cp PowerliminalsPlayer/bin/AppImages/PowerliminalsPlayer-x86_64.AppImage Publish/$1/PowerliminalsPlayer-$1_Linux_x64.AppImage
