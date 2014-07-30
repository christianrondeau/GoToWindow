using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;


namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class ProcessExtensionsTests
    {
        [TestMethod]
        public void CanFindOutTheExecutableNameOfAProcess()
        {
            using (var app = new GivenAnApp("GoToWindow.CanFindOutTheExecutableNameOfAProcess"))
            {
                Assert.AreEqual(app.Process.GetExecutablePath(), app.Executable);
            }
        }
    }
}
