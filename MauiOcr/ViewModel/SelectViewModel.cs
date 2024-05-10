using Camera.MAUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TesseractOcrMaui;
using MauiOcr.Services;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.IO;

namespace MauiOcr.ViewModel
{
    public partial class SelectViewModel : ObservableObject,INotifyPropertyChanged
    {
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
        ImageSource snapShot;

        [ObservableProperty]
        Stream snapShotStream;

        [ObservableProperty]
        byte[] byteArray;

        public int NumCameras
        {
            set
            {
                if (value > 0)
                    Camera = Cameras.First();
            }
        }
        public bool AutoStartPreview { get; set; } = false;
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

        [RelayCommand]
        private void ExecuteTakeSnapshot()
            {
            TakeSnapshot = false;
            TakeSnapshot = true;
        }

        public static async Task<SKBitmap> ConvertToGrayscaleAsync(SKBitmap originalBitmap)
        {
            // Create a new SKBitmap with the same dimensions as the original
            SKBitmap grayscaleBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

            // Iterate through each pixel of the original bitmap
            for (int x = 0; x < originalBitmap.Width; x++)
            {
                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    // Get the color of the pixel
                    SKColor pixelColor = originalBitmap.GetPixel(x, y);

                    // Calculate the grayscale value using luminosity method
                    byte grayValue = (byte)(0.299 * pixelColor.Red + 0.587 * pixelColor.Green + 0.114 * pixelColor.Blue);

                    // Create a new SKColor with the grayscale value
                    SKColor grayColor = new SKColor(grayValue, grayValue, grayValue);

                    // Set the pixel color in the grayscale bitmap
                    grayscaleBitmap.SetPixel(x, y, grayColor);
                }
            }

            return grayscaleBitmap;
        }
        public async Task BitmapToByteArrayAsync()
        {
            using (Stream imageStream = snapShotStream)
            {
                SKBitmap originalBitmap = SKBitmap.Decode(imageStream);
                SKBitmap grayscaleBitmap = await ConvertToGrayscaleAsync(originalBitmap);

                using (SKImage image = SKImage.FromBitmap(grayscaleBitmap))
                using (SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    byteArray = encoded.ToArray();
                }
            }
        }
    }
}
