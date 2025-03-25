using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoreView.App.ViewModels;

/// <summary>
/// Base view model for metric monitoring
/// </summary>
public abstract partial class BaseMetricViewModel : ObservableObject, IDisposable
{
    protected bool _disposed;

    [ObservableProperty]
    private DateTime _lastUpdated;

    [RelayCommand]
    protected abstract void RefreshData();

    protected virtual void UpdateLastUpdated()
    {
        LastUpdated = DateTime.Now;
    }

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}