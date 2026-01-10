public abstract partial class ViewModel(BaseServices services) : ObservableValidator
{
    protected BaseServices Services => services;
}


public abstract partial class ViewModel : IConnectivityEventHandler
{
    [ObservableProperty] bool isConnected;

    [MainThread]
    public Task Handle(ConnectivityChanged @event, IMediatorContext context, CancellationToken cancellationToken)
    {
        this.IsConnected = @event.Connected;
        return Task.CompletedTask;
    }
}