namespace SpaghettiManager.App;

public partial class App : Application
{
    public App(AppShell mainPage)
    {
        this.InitializeComponent();
        this.MainPage = mainPage;
    }
}