// using System;
// using System.Drawing;
// using System.Drawing.Imaging;
// using System.IO;
//

// namespace HanumanInstitute.CommonAvaloniaApp
// {
//     public static class WpfExtensionMethods
//     {
//         public static ImageSource ToImageSource(this Icon icon)
//         {
//             if (icon == null) { throw new ArgumentNullException(nameof(icon)); }
//
//             ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
//                 icon.Handle,
//                 Int32Rect.Empty,
//                 BitmapSizeOptions.FromEmptyOptions());
//
//             return imageSource;
//         }
//     }
// }
