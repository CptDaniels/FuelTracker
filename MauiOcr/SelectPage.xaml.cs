using MauiOcr.ViewModel;

namespace MauiOcr;

public partial class SelectPage : ContentPage
{

	public SelectPage(SelectViewModel vm)
	{
        InitializeComponent();
		BindingContext = vm;
	}


}