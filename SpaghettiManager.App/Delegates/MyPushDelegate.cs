using Shiny.Push;

namespace SpaghettiManager.App.Delegates;

public partial class MyPushDelegate : PushDelegate
{
    public override async Task OnEntry(PushNotification notification)
    {
    }

    public override async Task OnReceived(PushNotification notification)
    {
    }

    public override async Task OnNewToken(string token)
    {
    }

    public override async Task OnUnRegistered(string token)
    {
    }
}