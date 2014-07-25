using System;
using System.Configuration;

namespace GoToWindow
{
    public class AppConfiguration
    {
        public static readonly AppConfiguration Current = new AppConfiguration();

        public bool HookAltTab { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool AlwaysShow { get; set; }

        public AppConfiguration()
        {
            HookAltTab = GetBooleanSetting("GoToWindow.HookAltTab", true);
            AlwaysOnTop = GetBooleanSetting("GoToWindow.AlwaysOnTop", true);
            AlwaysShow = GetBooleanSetting("GoToWindow.AlwaysShow", false);
        }
        
        private bool GetBooleanSetting(string key, bool defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];

            if(value == null)
                return defaultValue;

            return Boolean.Parse(value);
        }
    }
}
