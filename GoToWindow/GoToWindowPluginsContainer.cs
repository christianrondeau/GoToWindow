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

                if(pluginsContainer.Plugins == null || !pluginsContainer.Plugins.Any())
                    throw new InstanceNotFoundException("No plug-ins found");

				pluginsContainer.Plugins = pluginsContainer.Plugins.OrderBy(plugin => plugin.Sequence).ToList();
				return pluginsContainer;
            }
            catch (InstanceNotFoundException exc)
            {
                HandleError(exc, ExitCodes.NoPluginsFound, "No plug-ins found. Check that at least GoToWindow.Plugins.Core.dll can be found in the Plugins directory and restart GoToWindow.");
                return null;
            }
            catch (Exception exc)
            {
                HandleError(exc, ExitCodes.ErrorLoadingPlugins, "An error occured while loading plug-ins. Try updating or removing plugins other than GoToWindow.Plugins.Core.dll from the Plugins directory and restart GoToWindow.");
                return null;
            }
		}

        private static void HandleError(Exception exc, ExitCodes exitCode, string message)
	    {
			var typeLoadExc = exc as ReflectionTypeLoadException;

			if (typeLoadExc != null)
				Log.Error(string.Join("; ", typeLoadExc.LoaderExceptions.Select(e => e.Message)), typeLoadExc);
			else
				Log.Error(exc);

	        MessageBox.Show(message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown((int)exitCode);
	    }
	}
}
