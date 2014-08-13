# Go To Window - Plugins

**Warning! This is an alpha, and the plug-in signature is subject to change.**

## GitHub Project Naming

Plugins for GoToWindow should be named `GoToWindow-Plugin-YourPlugin`. This will make it easier to find plug-ins in the future.

Also, if you think your extension should be part of GoToWindow's core, or if you need additional control from within your plug-ins, please create an issue.

## Creating your own plug-in

Create a `C#` project.

Reference `System.ComponentModel.Composition` and `GoToWindow.Extensibility`

You can now implement `IGoToWindowPlugin`, and mark you class as `Export` for GoToWindow to discover it. Here is an example class:

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

You will also need a `IGoToWindowSearchResult` implementation that will define the list entry to show, and what to do when it is selected. Here is an example implementation:

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

Finally, drop the compiled DLL into the `GoToWindow\Plugins` directory, and restart GoToWindow.