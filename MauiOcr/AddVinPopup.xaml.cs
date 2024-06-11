using CommunityToolkit.Maui.Views;
using MauiOcr.ViewModel;

namespace MauiOcr;

public partial class AddVinPopup : Popup
{
	public AddVinPopup(AddVinPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}