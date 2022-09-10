using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FoxLife.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Img
{
    internal class ImgContext : ForumDbContext,IDbContextConnection
    {
        public static ImageSource GetImage(int imgId)
        {
            using var db = new ImgContext();
            try
            {
                var data = db.Img.AsNoTracking().First(x => x.Id == imgId).Img;
                return ImgHelp.LoadImage(data);
            }
            catch
            {
                return null;
            }
        }

        #region DebugThings
        public static void ChangeDefault()
        {
            string path = @"C:\papka\programms\Git\Forum\FoxLife\FoxLife\icons\fox.png";

            FileInfo file = new FileInfo(path);
            long size = file.Length;
            if ((size / 1024) >= 256)
            {
                MainViewModel.Message("File size must be less then 256 Kb", MessageViewModel.MessageType.Error);
                return;
            }
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            //var img = br.ReadBytes((int)fs.Length);
            

            using var db = new ImgContext();
            var img = db.Img.AsParallel().Select(x => x).First(x => x.Id == -1);
            img.Img = br.ReadBytes((int)fs.Length);


            db.SaveChanges();
        }
        #endregion
    }
}
