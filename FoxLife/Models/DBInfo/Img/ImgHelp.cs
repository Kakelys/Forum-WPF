using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FoxLife.ViewModels;
using Microsoft.Win32;

namespace FoxLife.Models.DBInfo.Img
{
    internal static class ImgHelp
    {
        public static ImageSource LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static byte[]? OpenImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (dialog.ShowDialog() != true) return null;
            var path = dialog.FileName;
            //check file size
            FileInfo file = new FileInfo(path);
            long size = file.Length;
            if ((size / 1024) >= 256)
            {
                MainViewModel.Message("File size must be less then 256 Kb", MessageViewModel.MessageType.Error);
                return null;
            }
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            var img = br.ReadBytes((int)fs.Length);
            return img;
        }
    }
}
