// using System;
// using System.Drawing;
// using System.Drawing.Imaging;
// using System.Globalization;
// using System.IO;
// using Avalonia.Data.Converters;
//
// namespace HanumanInstitute.Common.Avalonia.App
// {
//     public class BitmapToImageSourceConverter : IValueConverter
//     {
//         public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
//         {
//             if (value is Bitmap bmp)
//             {
//                 using var memory = new MemoryStream();
//                 bmp.Save(memory, ImageFormat.Png);
//                 memory.Position = 0;
//
//                 var bitmapImage = new BitmapImage();
//                 bitmapImage.BeginInit();
//                 bitmapImage.StreamSource = memory;
//                 bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
//                 bitmapImage.EndInit();
//
//                 return bitmapImage;
//             }
//             return null;
//         }
//
//         public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }
