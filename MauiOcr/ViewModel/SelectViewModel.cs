using Camera.MAUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiOcr.Services;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SkiaSharp;
using SkiaSharp.Views;
using System.Diagnostics;

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

        [ObservableProperty]
        public ImageSource snapShot;

        [ObservableProperty]
        public bool autoSnapShotAsImageSource = true;

        public Stream SnapShotStream;

        //[ObservableProperty]
        public Command TakeSnapshotCmd { get; set; }
        public int NumCameras
        {
            set
            {
                if (value > 0)
                    Camera = Cameras.First();
            }
        }
        public bool AutoStartPreview { get; set; } = false;
        public float AutoSnapshotSeconds { get; set; } = 0f;
        public string Seconds
        {
            get => AutoSnapshotSeconds.ToString();
            set
            {
                if (float.TryParse(value, out float seconds))
                {
                    AutoSnapshotSeconds = seconds;
                    OnPropertyChanged(nameof(AutoSnapshotSeconds));
                }
            }
        }


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
            //SnapShot = snapShot;
            _imagePass = imagePass;
            //TakeSnapshotCmd = new Command( async() =>
            //{
            //    TakeSnapshot = false;
            //    TakeSnapshot = true;
            //    await Permissions.RequestAsync<Permissions.Camera>();
            //    await ExecuteTakeSnapshot();
            //});
            //OnPropertyChanged(nameof(TakeSnapshotCmd));
        }


        [RelayCommand]
        private async Task ExecuteTakeSnapshot()
            {
            TakeSnapshot = false;
            TakeSnapshot = true;
            await Permissions.RequestAsync<Permissions.Camera>();
            var stream = await Self.TakePhotoAsync();
            ImageSource imageSource = Self.GetSnapShot();
            SnapShot = imageSource;
            //if (stream != null)
            //{
            //    var result = ImageSource.FromStream(() => stream);
            //    SnapShot = result;
            //}
            //stream?.Seek(0, SeekOrigin.Begin);
            
            var subsetImageData = await GetImageSubsetAsync(SnapShot);
            SetImageBytes(subsetImageData);
        }
        
        public async Task<byte[]> GetImageSubsetAsync(ImageSource imageSource)
        {
            if (imageSource == null)
            {
                throw new ArgumentNullException(nameof(imageSource));
            }

            Stream imageStream = await GetStreamFromImageSource(imageSource);

            if (imageStream.CanSeek)
            {
                imageStream.Seek(0, SeekOrigin.Begin);
            }

            using (SKBitmap bitmap = SKBitmap.Decode(imageStream))
            {
                if (bitmap != null)
                {
                    // Original image dimensions
                    int originalWidth = bitmap.Width;
                    int originalHeight = bitmap.Height;

                    // Coordinates for the 200x50 box
                    int startX = (int)(originalWidth * 0.5) - 100;
                    int startY = (int)(originalHeight * 0.6) - 25;
                    int boxWidth = 200;
                    int boxHeight = 50;

                    SKRectI subsetRect = new SKRectI(startX, startY, startX + boxWidth, startY + boxHeight);
                    SKBitmap subsetBitmap = new SKBitmap(boxWidth, boxHeight);

                    using (SKCanvas canvas = new SKCanvas(subsetBitmap))
                    {
                        canvas.DrawBitmap(bitmap, subsetRect, new SKRect(0, 0, boxWidth, boxHeight));
                    }

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
                    throw new Exception("Failed to decode the image stream.");
                }
            }
        }
        public void SetImageBytes(byte[] bytes)
        {
            _imagePass.ImageBytes = bytes;
        }

        private async Task<Stream> GetStreamFromImageSource(ImageSource imageSource)
        {
            if (imageSource is StreamImageSource streamImageSource)
            {
                return await streamImageSource.Stream(CancellationToken.None);
            }
            else if (imageSource is FileImageSource fileImageSource)
            {
                string filePath = fileImageSource.File;
                return File.OpenRead(filePath);
            }
            else if (imageSource is UriImageSource uriImageSource)
            {
                using (var client = new HttpClient())
                {
                    return await client.GetStreamAsync(uriImageSource.Uri);
                }
            }
            else
            {
                throw new NotSupportedException("ImageSource type not supported.");
            }
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
                    byte median = neighbors[4]; // Median value in the sorted list of 9 neighbors
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
