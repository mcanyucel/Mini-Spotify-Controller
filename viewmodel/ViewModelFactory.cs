using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MiniSpotifyController.viewmodel;

public class ViewModelFactory(IServiceProvider serviceProvider)
{
    public TViewModel Create<TViewModel>(Dictionary<string, object>? parameters = null) where TViewModel : IViewModel =>
        parameters == null
            ? ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider)
            : ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider,
                parameters);
}