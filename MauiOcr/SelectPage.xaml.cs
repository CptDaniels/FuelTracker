using MauiOcr.ViewModel;
using Camera.MAUI;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Storage;
using System.IO;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui;
using System.Reflection;
using System.Diagnostics;

namespace MauiOcr;

public partial class SelectPage : ContentPage
{
	public SelectPage(SelectViewModel vm)
	{
        InitializeComponent();
		BindingContext = vm;
        //InitializeBitmapAsync();
	}

    static public string FilePath;
    //static public byte[] Bitmap;

    private void PictureTaken(object sender, EventArgs e)
    {
        FilePath = Path.Combine(FileSystem.Current.CacheDirectory, $"temp.png");

        Image.Source = cameraView.GetSnapShot(Camera.MAUI.ImageFormat.PNG);
        var result = cameraView.SaveSnapShot(Camera.MAUI.ImageFormat.PNG, FilePath);
        Trace.WriteLine($"Cache dir: {FileSystem.Current.CacheDirectory} and result of snapshot {result}");
    }
    private void CameraView_CamerasLoaded(object sender, EventArgs e)
    {
        cameraView.Camera = cameraView.Cameras.First();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await cameraView.StopCameraAsync();
            var result = await cameraView.StartCameraAsync();
        });
    }
    private async void InitializeBitmapAsync()
    {
        //Bitmap = await ImageSourceToByteArrayAsync(FilePath);
    }


    public static async Task<byte[]> ImageSourceToByteArrayAsync(string filepath)
    {
        var assem = Assembly.GetExecutingAssembly();
        using var stream = assem.GetManifestResourceStream(filepath);
        byte[] bytesAvailable = new byte[stream.Length];
        stream.Read(bytesAvailable, 0, bytesAvailable.Length);
        return bytesAvailable;
    }
}