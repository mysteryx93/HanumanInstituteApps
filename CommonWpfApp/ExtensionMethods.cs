using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HanumanInstitute.CommonWpfApp
{
    public static class WpfExtensionMethods
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null) { throw new ArgumentNullException(nameof(icon)); }

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            if (bitmap == null) { throw new ArgumentNullException(nameof(bitmap)); }

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
