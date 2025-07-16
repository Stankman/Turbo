using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Configuration;
using Turbo.Core.Plugins;

namespace Turbo.Plugins;

public class TurboPluginManager(
    ILogger<TurboPluginManager> _logger,
    IServiceProvider _serviceProvider,
    IEmulatorConfig _emulatorConfig) : IPluginManager
{
    private readonly HashSet<MethodInfo> _methods = new();
    private readonly HashSet<ITurboPlugin> _plugins = new();

    public void LoadPlugins()
    {
        _logger.LogInformation("{Context} -> Loading plugins...", nameof(TurboPluginManager));

        if (!Directory.Exists("plugins")) Directory.CreateDirectory("plugins");

        var plugins = Directory.GetFiles("plugins", "*.dll");

        var pluginOrder = _emulatorConfig.PluginOrder.ToArray();

        if (pluginOrder != null) plugins = [.. plugins.OrderBy(value => Array.IndexOf(pluginOrder, value))];

        foreach (var plugin in plugins)
            try
            {
                var assembly = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), plugin));

                if (assembly == null) return;

                // Get a list of all types in assembly that implement ITurboPlugin. 
                // Exclude interfaces, abstract and generic types.
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(ITurboPlugin).IsAssignableFrom(t))
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && !t.IsGenericType)
                    .ToList();

                if (pluginTypes.Any())
                    // Create instances
                    foreach (var pluginType in pluginTypes)
                        CreatePluginInstance(pluginType);
                else
                    _logger.LogError(
                        "{Context} -> {Plugin} can't be loaded because it doesn't implement {PluginInterface}!",
                        nameof(TurboPluginManager), plugin, nameof(ITurboPlugin));
            }

            catch (Exception ex)
            {
                _logger.LogError("{Context} -> {Plugin} not loaded : {ex}", nameof(TurboPluginManager), plugin, ex.StackTrace);
                Console.WriteLine($"{ex.StackTrace}");
            }

        _logger.LogInformation("{Context} -> {AmountOfPlugins} plugin(s) loaded!", nameof(TurboPluginManager),
            _plugins.Count);
    }

    private void CreatePluginInstance(Type pluginType)
    {
        try
        {
            var constructors = pluginType.GetConstructors();
            var firstConstructor = constructors.FirstOrDefault();

            if (firstConstructor == null)
            {
                _logger.LogError("No public constructor found for plugin type {PluginType} in assembly {Assembly}", pluginType.FullName, pluginType.Assembly.FullName);
                return;
            }

            var parameters = new List<object>();

            foreach (var param in firstConstructor.GetParameters())
            {
                var service = _serviceProvider.GetService(param.ParameterType);
                parameters.Add(service);
            }

            var pluginInstance = Activator.CreateInstance(pluginType, [.. parameters]) as ITurboPlugin;

            if (pluginInstance != null)
            {
                _plugins.Add(pluginInstance);
                _logger.LogInformation("{Context} -> Loaded {PluginName} by {PluginAuthor}", nameof(TurboPluginManager),
                    pluginInstance.PluginName, pluginInstance.PluginAuthor);
            }
            else
            {
                _logger.LogError("Failed to create instance of plugin type {PluginType}", pluginType.FullName);
            }
        }
        catch (ReflectionTypeLoadException rex)
        {
            foreach (var loaderException in rex.LoaderExceptions)
            {
                _logger.LogError("LoaderException: {Message}", loaderException.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception while creating plugin instance for {PluginType}: {Exception}", pluginType.FullName, ex);
            Console.WriteLine($"Stack Trace: {ex}");
        }
    }
}