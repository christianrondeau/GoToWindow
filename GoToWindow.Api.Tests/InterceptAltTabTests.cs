using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class InterceptAltTabTests
    {
        [TestMethod]
        public void CanInterceptAltTab()
        {
            var intercepted = false;
            using(var interceptor = new InterceptAltTab(() => intercepted = true))
            {
                KeyboardSend.KeyDown(KeyboardSend.LAlt);
                KeyboardSend.KeyPress(KeyboardSend.Tab);
                KeyboardSend.KeyUp(KeyboardSend.LAlt);
            }

            Assert.IsTrue(intercepted, "Alt + Tab was not intercepted by InterceptAltTab");
        }
    }
}
