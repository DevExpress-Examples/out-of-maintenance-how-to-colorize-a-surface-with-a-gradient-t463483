using DevExpress.Utils;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace GradientHeightMap {
    public class HeightMapViewModel {
        public static Uri HeightMapUri { get { return AssemblyHelper.GetResourceUri(typeof(HeightMapViewModel).Assembly, "Data/Heightmap.jpg"); } }
        public ImageData ImageData { get; set; }
        public HeightMapViewModel() {
            GenerateMap(HeightMapUri);
        }
        void GenerateMap(Uri uri) {
            ImageDataLoader imageDataLoader = new ImageDataLoader(uri);
            PixelColor[,] pixels = imageDataLoader.GetPixels();
            int countX = pixels.GetLength(0);
            int countZ = pixels.GetLength(1);
            int startX = 0;
            int startZ = 0;
            int gridStep = 100;
            double minY = -300;
            double maxY = 3000;
            ImageData imageData = new ImageData(
                new DoubleCollection(countX),
                new DoubleCollection(countZ),
                new DoubleCollection(countX * countZ)
                );
            bool fullZ = false;
            for (int i = 0; i < countX; i++) {
                imageData.XArguments.Add(startX + i * gridStep);
                for (int j = 0; j < countZ; j++) {
                    if (!fullZ)
                        imageData.YArguments.Add(startZ + j * gridStep);
                    double value = GetValue(pixels[i, j], minY, maxY);
                    imageData.Values.Add(value);
                }
                fullZ = true;
            }
            ImageData = imageData;
        }
        double GetValue(PixelColor color, double min, double max) {
            double normalizedValue = (double)color.Gray / 255.0;
            return min + normalizedValue * (max - min);
        }
    }
    public class ImageDataLoader {
        readonly StreamResourceInfo streamResourceInfo;
        public ImageDataLoader(Uri uri) {
            this.streamResourceInfo = Application.GetResourceStream(uri);
        }
        PixelColor[,] GetPixels(BitmapSource source) {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            PixelColor[,] result = new PixelColor[source.PixelWidth, source.PixelHeight];
            int stride = (int)source.PixelWidth * (source.Format.BitsPerPixel / 8);
            CopyPixels(source, result, stride, 0);
            return result;
        }
        void CopyPixels(BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[x + x0, y + y0] = new PixelColor {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
        }
        void DoEvents() {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        public PixelColor[,] GetPixels() {
            PixelColor[,] pixels = new PixelColor[0, 0];
            try {
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = this.streamResourceInfo.Stream;
                source.EndInit();
                while (source.IsDownloading) {
                    DoEvents();
                }
                pixels = GetPixels(source);
            }
            catch {
            }
            return pixels;
        }
    }
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
        public byte Gray { get { return (byte)(((int)Blue + (int)Green + (int)Red) / 3); } }
    }
}