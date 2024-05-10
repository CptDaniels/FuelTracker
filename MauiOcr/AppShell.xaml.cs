namespace MauiOcr
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SelectPage), typeof(SelectPage));
        }
    }
}
