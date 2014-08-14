using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using GoToWindow.Extensibility;
using log4net;

namespace GoToWindow
{
	public interface IGoToWindowPluginsContainer
	{
		List<IGoToWindowPlugin> Plugins { get; set; }
	}

	public class GoToWindowPluginsContainer : IGoToWindowPluginsContainer
	{
		public const string PluginsFolderName = "Plugins";

		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowPluginsContainer).Assembly, "GoToWindow");

		[ImportMany(GoToWindowConstants.PluginContractName)]
		public List<IGoToWindowPlugin> Plugins { get; set; }

		public static GoToWindowPluginsContainer LoadPlugins()
		{
			var catalog = new AggregateCatalog();
			catalog.Catalogs.Add(new DirectoryCatalog(PluginsFolderName));
			var container = new CompositionContainer(catalog);

			try
			{
				var pluginsContainer = new GoToWindowPluginsContainer();
				container.ComposeParts(pluginsContainer);

				pluginsContainer.Plugins = pluginsContainer.Plugins.OrderBy(plugin => plugin.Sequence).ToList();
				return pluginsContainer;
			}
			catch (CompositionException compositionException)
			{
				Log.Error(compositionException);
				MessageBox.Show("An error occured while loading plug-ins. Try updating or removing plugins other than GoToWindow.Plugins.Core.dll from the Plugins directory and restart GoToWindow.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
				throw;
			}
		}
	}
}
