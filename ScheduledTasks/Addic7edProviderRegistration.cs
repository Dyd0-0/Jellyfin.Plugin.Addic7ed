using System;
using Jellyfin.Plugin.Addic7ed.Providers;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Subtitles;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Addic7ed.ScheduledTasks
{
    /// <summary>
    /// Class to register the Addic7ed subtitle provider with Jellyfin.
    /// </summary>
    public class Addic7edProviderRegistration : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISubtitleProvider, Addic7edSubtitleProvider>();
        }
    }
}
