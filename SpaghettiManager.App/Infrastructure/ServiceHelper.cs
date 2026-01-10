using System;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace SpaghettiManager.App.Infrastructure;

public static class ServiceHelper
{
    public static IServiceProvider Services => MauiApplication.Current.Services;

    public static T GetRequiredService<T>() where T : notnull
        => Services.GetRequiredService<T>();
}

