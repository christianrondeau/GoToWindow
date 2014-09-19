using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using log4net.Config;

[assembly: AssemblyTitle("Go To Window")]
[assembly: AssemblyDescription("The keyboard-maniac alt-tab alternative")]

[assembly: ComVisible(false)]

[assembly: ThemeInfo(
	ResourceDictionaryLocation.None,
	ResourceDictionaryLocation.SourceAssembly
)]

[assembly: XmlConfigurator(ConfigFile = "log4net.config")]

[assembly: AssemblyMetadata("SquirrelAwareVersion", "1")]
