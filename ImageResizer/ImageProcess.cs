using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }
        }
        
        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var fileExtensions = new List<string>
                                 {
                                     "png"
                                   , "jpg"
                                   , "jpeg"
                                 };
            var tasks = fileExtensions.Select(fileExtension =>
                                                  Task.Run(() => ResizeImagesByFileExtensionAsync(sourcePath
                                                                                                , destPath
                                                                                                , scale
                                                                                                , fileExtension)))
                                      .ToArray();
            await Task.WhenAll(tasks);
        }

        private async Task ResizeImagesByFileExtensionAsync(string     sourcePath
                                                          , string     destPath
                                                          , double     scale
                                                          , string     fileExtension)
        {
            var allFiles = await FindImagesAsync(sourcePath, fileExtension);
            var fileTasks = allFiles.Select(file =>
                                                Task.Run(() => ResizeImageAsync(destPath
                                                                              , scale
                                                                              , file)));
            await Task.WhenAll(fileTasks);
        }

        private async Task<bool> ResizeImageAsync(string destPath , double scale , string file)
        {
            Image imgPhoto = Image.FromFile(file);
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destionatonWidth = (int)(sourceWidth * scale);
            int destionatonHeight = (int)(sourceHeight * scale);

            Bitmap processedImage = processBitmap((Bitmap)imgPhoto, sourceWidth, sourceHeight, destionatonWidth, destionatonHeight);

            string imgName = Path.GetFileNameWithoutExtension(file);
            string destFile = Path.Combine(destPath, imgName + ".jpg");
            processedImage.Save(destFile, ImageFormat.Jpeg);

            return true;
        }


        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        private async Task<string[]> FindImagesAsync(string srcPath, string fileExtension)
        {
            return await Task.Run(() => Directory.GetFiles(srcPath, $"*.{fileExtension}", SearchOption.AllDirectories));
        }
        
        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}
