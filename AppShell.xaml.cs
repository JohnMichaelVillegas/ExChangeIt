namespace ExChangeIt
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RatesPage), typeof(RatesPage));
        }
    }
}