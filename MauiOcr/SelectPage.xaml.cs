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
	}


}