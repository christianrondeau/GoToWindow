using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows;
using GoToWindow.Extensibility;
using log4net;
using System.Reflection;

namespace GoToWindow
{
	public interface IGoToWindowPluginsContainer
	{
		List<IGoToWindowPlugin> Plugins { get; set; }
	}

	public class GoToWindowPluginsContainer : IGoToWindowPluginsContainer
	{
		public const string PluginsFolderName = "Plugins";

		public const string PluginErrorMessage = "An error occured while loading plug-ins. Try updating or removing plugins other than GoToWindow.Plugins.Core.dll from the Plugins directory and restart GoToWindow.";

		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowPluginsContainer).Assembly, "GoToWindow");

		[ImportMany(GoToWindowConstants.PluginContractName)]
		public List<IGoToWindowPlugin> Plugins { get; set; }

		public static GoToWindowPluginsContainer LoadPlugins()
		{
			var catalog = new AggregateCatalog();
			try
			{
				catalog.Catalogs.Add(new DirectoryCatalog(PluginsFolderName));
			}
			catch (DirectoryNotFoundException exc)
			{
				HandleError(exc, ExitCodes.PluginDirectoryNotFound, "Plugins directory not found. Check that the Plugins directory is created and at least contains at least GoToWindow.Plugins.Core.dll can be found and restart GoToWindow.");
				return null;
			}

			var container = new CompositionContainer(catalog);

			try
			{
				var pluginsContainer = new GoToWindowPluginsContainer();
				container.ComposeParts(pluginsContainer);

				DiagnosticCatalogComposition(catalog);

				if(pluginsContainer.Plugins == null || !pluginsContainer.Plugins.Any())
				{
					HandleError(new Exception("No plugins were composed"), ExitCodes.NoPluginsFound, "No plug-ins found. Check that at least GoToWindow.Plugins.Core.dll can be found in the Plugins directory and restart GoToWindow.");
					return null;
				}

				pluginsContainer.Plugins = pluginsContainer.Plugins.OrderBy(plugin => plugin.Sequence).ToList();
				return pluginsContainer;
			}
			catch (InstanceNotFoundException exc)
			{
				HandleError(exc, ExitCodes.NoPluginsFound, "No plug-ins found. Check that at least GoToWindow.Plugins.Core.dll can be found in the Plugins directory and restart GoToWindow.");
				return null;
			}
			catch(ReflectionTypeLoadException exc)
			{
				HandleError(exc, ExitCodes.ErrorLoadingPluginsTypes, "An error occured while loading plugin types.", string.Join("; ", exc.LoaderExceptions.Select(e => e.Message)));
				return null;
			}
			catch (Exception exc)
			{
				HandleError(exc, ExitCodes.ErrorLoadingPlugins, "An error occured while loading plug-ins. Try updating or removing plugins other than GoToWindow.Plugins.Core.dll from the Plugins directory and restart GoToWindow.");
				return null;
			}
		}

		private static void DiagnosticCatalogComposition(AggregateCatalog catalog)
		{
			if (!Log.IsDebugEnabled) return;

			try
			{
				foreach (var c in catalog.Catalogs)
				{
					if (c is DirectoryCatalog directoryCatalog)
					{
						Log.Debug($"Loaded directory catalog from '{directoryCatalog.FullPath}', looking for '{directoryCatalog.SearchPattern}'. Loaded: {string.Join(", ", directoryCatalog.LoadedFiles)}");
					}
					else
					{
						Log.Debug($"Loaded catalog of type {c.GetType().FullName}");
					}
				}
			}
			catch (Exception exc)
			{
				Log.Warn("Failed generating catalog information", exc);
			}
		}

		private static void HandleError(Exception exc, ExitCodes exitCode, string message, string additionalInfo = null)
		{
			if (additionalInfo != null)
				Log.Error(additionalInfo, exc);
			else
				Log.Error(exc);
			


			MessageBox.Show(message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
			Application.Current.Shutdown((int)exitCode);
		}
	}
}
