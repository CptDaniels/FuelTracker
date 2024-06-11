using Camera.MAUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiOcr.Services;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SkiaSharp;
using SkiaSharp.Views;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace MauiOcr.ViewModel
{
    public partial class SelectViewModel : ObservableObject,INotifyPropertyChanged
    {
        private readonly IImagePass _imagePass;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CameraInfo camera = null;
        public CameraInfo Camera 
        {
            get => camera;
            set
            {
                camera = value;
                OnPropertyChanged(nameof(Camera));
                AutoStartPreview = false;
                OnPropertyChanged(nameof(AutoStartPreview));
                AutoStartPreview = true;
                OnPropertyChanged(nameof(AutoStartPreview));
            }
        }
        [ObservableProperty]
        ObservableCollection<CameraInfo> cameras = new();

        public bool AutoSnapShotAsImageSource{get;set;} = false;
        public ImageSource SnapSource {get;set;}

        [ObservableProperty]
        Stream snapStream;

        [ObservableProperty]
        SKBitmap bitmap;
        [ObservableProperty]
        byte[] bytes;
        public int NumCameras
        {
            set
            {
                if (value > 0)
                    Camera = Cameras.First();
            }
        }

        
        public bool AutoStartPreview { get; set; } = false;
        public float SnapshotSeconds { get; set; } = 0f;

        private bool takeSnapshot = false;
        public bool TakeSnapshot
        {
            get => takeSnapshot;
            set
            {
                takeSnapshot = value;
                OnPropertyChanged(nameof(TakeSnapshot));
            }
        }
        private CameraView self;
        public CameraView Self
        {
            get => self;
            set
            {

                self = value;
                OnPropertyChanged(nameof(Self));
            }
        }
        public SelectViewModel(IImagePass imagePass)
        {
            _imagePass = imagePass;
        }
        

        [RelayCommand]
        private async Task ExecuteTakeSnapshot()
            {
            TakeSnapshot = false;
            TakeSnapshot = true;
            OnPropertyChanged(nameof(TakeSnapshot));
            Bitmap = await GetBitmapFromImageSource(SnapSource);
            var subsetImageData = await GetImageSubsetAsync(Bitmap);     
            SetImageBytes(subsetImageData);
        }
        
        public async Task<byte[]> GetImageSubsetAsync(SKBitmap bitmap)
        {

            {
                if (bitmap != null)
                {

                    int originalWidth = bitmap.Width;
                    int originalHeight = bitmap.Height;


                    int startX = (int)(originalWidth * 0.5) - 100;
                    int startY = (int)(originalHeight * 0.6) - 25;
                    int boxWidth = 200;
                    int boxHeight = 50;

                    SKRectI subsetRect = new SKRectI(startX, startY, startX + boxWidth, startY + boxHeight);
                    SKBitmap subsetBitmap = new SKBitmap(boxWidth, boxHeight);

                    SKCanvas canvas = new SKCanvas(subsetBitmap);
                    
                    canvas.DrawBitmap(bitmap, subsetRect, new SKRect(0, 0, boxWidth, boxHeight));
                    

                    SKBitmap grayscaleBitmap = await Task.Run(() => ConvertToGrayscale(subsetBitmap));
                    SKBitmap blurredBitmap = await Task.Run(() => ApplyMedianBlur(grayscaleBitmap));
                    //SKBitmap thresholdedBitmap = await Task.Run(() => ApplyThresholding(blurredBitmap));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        grayscaleBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                        byte[] subsetImageData = ms.ToArray();
                        return subsetImageData;
                    }
                }
                else
                {
                    throw new Exception("Failed to de" +
                        "code the image stream.");
                }
            }
        }
        public void SetImageBytes(byte[] bytes)
        {
            _imagePass.ImageBytes = bytes;
        }

        private static async Task<SKBitmap> GetBitmapFromImageSource(ImageSource imageSource)
        {
            if (imageSource == null)
            {
                throw new ArgumentNullException(nameof(imageSource));
            }

            Stream imageStream = null;
            SKBitmap bitmap = null;

            try
            {
                imageStream = await ((StreamImageSource)imageSource).Stream(CancellationToken.None);

                if (imageStream == null)
                {
                    throw new Exception("Failed to get the stream from the ImageSource.");
                }

                if (imageStream.CanSeek)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);
                }

                // Debugging: Check the length of the stream
                Debug.WriteLine($"Stream length: {imageStream.Length}");

                // Debugging: Optionally save the stream to a file to inspect it
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    //File.WriteAllBytes("/path/to/save/stream_content.png", memoryStream.ToArray());
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitmap = SKBitmap.Decode(memoryStream);
                }

                if (bitmap == null)
                {
                    throw new Exception("Failed to decode the bitmap from the stream.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetBitmapFromImageSource: {ex.Message}");
            }
            finally
            {
                imageStream?.Dispose();
            }

            return bitmap;
        }
        private static async Task<byte[]> ImageSourceToByteArray(ImageSource imgsrc)
        {
            Stream stream = await ((StreamImageSource)imgsrc).Stream(CancellationToken.None);
            byte[] bytesAvailable = new byte[stream.Length];
            stream.Read(bytesAvailable, 0, bytesAvailable.Length);

            return bytesAvailable;
        }

        private SKBitmap ConvertToGrayscale(SKBitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            SKBitmap grayscaleBitmap = new SKBitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SKColor color = bitmap.GetPixel(x, y);
                    byte gray = (byte)(0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue);
                    grayscaleBitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
                }
            }
            return grayscaleBitmap;
        }

        private SKBitmap ApplyMedianBlur(SKBitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            SKBitmap blurredBitmap = new SKBitmap(width, height);

            // Apply a 3x3 median filter
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    List<byte> neighbors = new List<byte>();
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            neighbors.Add(bitmap.GetPixel(x + kx, y + ky).Red);
                        }
                    }
                    neighbors.Sort();
                    byte median = neighbors[4];
                    blurredBitmap.SetPixel(x, y, new SKColor(median, median, median));
                }
            }
            return blurredBitmap;
        }

        private SKBitmap ApplyThresholding(SKBitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            SKBitmap thresholdedBitmap = new SKBitmap(width, height);

            // Apply a fixed threshold
            byte threshold = 64;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte gray = bitmap.GetPixel(x, y).Red;
                    byte binary = gray >= threshold ? (byte)255 : (byte)0;
                    thresholdedBitmap.SetPixel(x, y, new SKColor(binary, binary, binary));
                }
            }
            return thresholdedBitmap;
        }
    }
}
