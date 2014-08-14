using System;
using System.Windows;
using GoToWindow.Windows;
using GoToWindow.ViewModels;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility;
using System.ComponentModel.Composition.Hosting;
using log4net;
using System.Collections.Generic;

namespace GoToWindow
{
    public class GoToWindowPluginsContainer
    {
        public const string PluginsFolderName = "Plugins";

        private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowPluginsContainer).Assembly, "GoToWindow");

        [ImportMany(GoToWindowPluginConstants.GoToWindowPluginContractName)]
        public IEnumerable<IGoToWindowPlugin> Plugins { get; set; }

        public static GoToWindowPluginsContainer LoadPlugins()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(PluginsFolderName));
            var container = new CompositionContainer(catalog);

            try
            {
                var pluginsContainer = new GoToWindowPluginsContainer();
                container.ComposeParts(pluginsContainer);
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
