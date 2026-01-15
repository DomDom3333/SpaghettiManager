namespace SpaghettiManager.App;

public partial class App : Application
{
    private SpaghettiDatabase db;
    public App(AppShell mainPage, SpaghettiDatabase database)
    {
        this.InitializeComponent();
        this.MainPage = mainPage;
        this.db = database;
    }

    protected async override void OnStart()
    {
        base.OnStart();
        await db.InitializeAsync();
    }
}