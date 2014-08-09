using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class InterceptAltTabTests
    {
        [TestMethod]
        public void CanInterceptAltTab()
        {
            var intercepted = false;
            using (new HotKey(Key.Tab, KeyModifier.Alt, key => intercepted = true))
            {
                KeyboardSend.KeyDown(KeyboardSend.LAlt);
                KeyboardSend.KeyPress(KeyboardSend.Tab);
                KeyboardSend.KeyUp(KeyboardSend.LAlt);
            }

            Assert.IsTrue(intercepted, "Alt + Tab was not intercepted by InterceptAltTab");
        }
    }
}
