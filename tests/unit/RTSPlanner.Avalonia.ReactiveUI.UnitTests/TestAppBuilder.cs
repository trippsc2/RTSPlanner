using System;
using Avalonia;
using Avalonia.Headless;
using RTSPlanner.Avalonia.ReactiveUI.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace RTSPlanner.Avalonia.ReactiveUI.UnitTests;

public sealed class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        throw new NotImplementedException();
    }
}