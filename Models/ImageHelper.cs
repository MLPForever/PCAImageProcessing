using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PCAImageProcessing.Models
{
    public static class ImageHelper
    {
        public static Mat[] LoadImages(string folderPath, int width = 35, int height = 40)
        {
            var files = Directory.GetFiles(folderPath, "*.jpg");
            Mat[] images = new Mat[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Mat img = Cv2.ImRead(files[i], ImreadModes.Grayscale);
                img = img.Resize(new OpenCvSharp.Size(width, height));
                img.ConvertTo(img, MatType.CV_64F);
                images[i] = img;
            }

            return images;
        }
        public static Mat BitmapImageToMat(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                using (var bmp = new System.Drawing.Bitmap(outStream))
                {
                    return OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
                }
            }
        }

        public static BitmapImage MatToBitmapImage(Mat mat)
        {
            Cv2.Normalize(mat, mat, 0, 255, NormTypes.MinMax);
            mat.ConvertTo(mat, MatType.CV_8UC1);

            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}
