using System;
using System.Collections.Generic;
using System.Windows;
using MiniSpotifyController.viewmodel;
using Serilog;

namespace MiniSpotifyController.window;

public class WindowFactory(IDictionary<Type, Type> viewModelWindowMappings, ILogger logger) 
{
    public Window CreateWindowForViewModel<TViewModel>() where TViewModel : IViewModel
    {
        var viewModelType = typeof(TViewModel);
        
        if (!viewModelWindowMappings.TryGetValue(viewModelType, out var windowType))
        {
            logger.Error($"No window found for view model {viewModelType.Name}");
            throw new ArgumentException($"No window found for view model {viewModelType.Name}");
        }

        if (!typeof(Window).IsAssignableFrom(windowType))
        {
            logger.Error($"Window type {windowType.Name} does not inherit from {nameof(Window)}");
            throw new ArgumentException($"Window type {windowType.Name} does not inherit from {nameof(Window)}");
        }

        if (Activator.CreateInstance(windowType) is Window window) return window;
        
        logger.Error($"Failed to create window for view model {viewModelType.Name}");
        throw new InvalidOperationException($"Failed to create window for view model {viewModelType.Name}");
    }
}