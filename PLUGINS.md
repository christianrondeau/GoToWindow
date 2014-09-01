# GoToWindow - Plugins

GoToWindow is extensible. Even the core functionality is a plug-in, that can be replaced if you wish. You can also write your own!

## Creating your own plugin

**Warning! This is an alpha, and the plug-in signature is subject to change.**

### GitHub Project Naming

Plugins for GoToWindow, when open sourced on GitHub, should be named `GoToWindow-Plugin-YourPlugin`. This will make it easier to discover plug-ins for other people. Also, let us know!

If you think your extension should be part of GoToWindow's core, or if you need additional control on how things are displayed or handled from within your plug-ins, please create an issue.

### Writing your first plugin

Create a `C# 4.0` project. We suggest the convention `GoToWindow.Plugins.YourPlugin` for the project name, to clearly see the difference between plug-ins and dependencies.

Next, Reference `System.ComponentModel.Composition` (from [MEF](http://msdn.microsoft.com/en-CA/library/dd460648(v=vs.110).aspx)) and `GoToWindow.Extensibility` (from GoToWindow's install directory).

Implement `IGoToWindowPlugin`, and add an `Export` attribute to your class so GoToWindow can discover it. Here is an example class:

    [Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
    public class MyPlugin : IGoToWindowPlugin
    {
		public string Id { get { return "MyAmazingPlugin"; } }

        public string Name { get { return "My Amazing Plugin"; } }

        public GoToWindowPluginSequence Sequence
        {
            get { return GoToWindowPluginSequence.AfterCore; }
        }

        public void BuildList(List<IGoToWindowSearchResult> list)
        {
            list.Add(new MySearchResult());
        }
    }

Note the `IGoToWindowSearchResult`. You must implement your own to define what user control to show, how to filter it and what to do when it is selected. Here is an example implementation:

    public class MySearchResult : IGoToWindowSearchResult
    {
        public UserControl View { get; set; }

        public MySearchResult()
        {
            View = new MyWpfUserControl();
        }

        public void Select()
        {
            // Do something!
        }

        public bool IsShown(string searchQuery)
        {
            return true; // Filter based on properties, or custom conditions
        }
    }

Finally, place the compiled plugin dll file into the `GoToWindow\Plugins` directory, and restart GoToWindow.

### Extending the Existing List

Since you have access to the previous plugins list in BuildList, you can modify it and replace entries by your own. You'll need to find entries that implement `IWindowSearchResult`, which provide everything you need to interact with the window.

### Reusing the existing views

If you want to add new entries, but still use GoToWindow's existing display, you can extend the basic search results. Note that to use the `BasicListEntry` view, your search result must implement `IBasicSearchResult`.


    public class CustomSearchResult : SearchResultBase, IBasicSearchResult, ISearchResult
    {
        private readonly IBasicSearchResult _item;

        public BitmapFrame Icon
        {
            get { return null; }
        }

        public string Title
        {
            get { return "My own entry!"; }
        }

        public string Process
        {
            get { return "Hello World"; }
        }

        public ChromeTabSearchResult()
            : base(() => new BasicListEntry())
        {
        }

        public void Select()
        {
            // Do Something
        }

        public bool IsShown(string searchQuery)
        {
            return IsShown(searchQuery, Process, Title);
        }
    }

### Debugging

If you have trouble debugging, you can use the `GoToWindow.log` files, which should be generated next to `GoToWindow.exe`. If you wish to do so, you can also reference `log4net.dll` in your project, and generate your own logs as needed.

Happy programming!