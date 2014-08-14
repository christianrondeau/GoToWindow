# GoToWindow - Plugins

GoToWindow is extensible. Even the core functionality is a plug-in, that can be replaced if you wish. You can also write your own!

## Built-in Plugins

* `GoToWindow.Plugins.Core`: Loads the core windows list, as shown by the native `Alt` + `Tab` screen. 

## Creating your own plugin

**Warning! This is an alpha, and the plug-in signature is subject to change.**

### GitHub Project Naming

Plugins for GoToWindow, when open sourced on GitHub, should be named `GoToWindow-Plugin-YourPlugin`. This will make it easier to discover plug-ins for other people. Also, let us know!

If you think your extension should be part of GoToWindow's core, or if you need additional control on how things are displayed or handled from within your plug-ins, please create an issue.

### Writing your first plugin

Create a `C# 4.0` project. We suggest the convention `GoToWindow.Plugins.YourPlugin` for the project name, to clearly see the difference between plug-ins and dependencies.

Next, Reference `System.ComponentModel.Composition` (from [MEF](http://msdn.microsoft.com/en-CA/library/dd460648(v=vs.110).aspx)) and `GoToWindow.Extensibility` (from GoToWindow's install directory).

Implement `IGoToWindowPlugin`, and add an `Export` attribute to your class so GoToWindow can discover it. Here is an example class:

    [Export(GoToWindowPluginConstants.GoToWindowPluginContractName, typeof(IGoToWindowPlugin))]
    public class MyPlugin : IGoToWindowPlugin
    {
        public IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList()
        {
            return new IGoToWindowSearchResult[] {
                new MySearchResult();
            };
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

### Debugging

If you have trouble debugging, you can use the `GoToWindow.log` files, which should be generated next to `GoToWindow.exe`. If you wish to do so, you can also reference `log4net.dll` in your project, and generate your own logs as needed.

Happy programming!