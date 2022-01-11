using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwUtils;

namespace OwUtilsTests
{
    [TestClass]
    public class TestOwUtils
    {
        [TestMethod]
        public void TestCapture()
        {
            new OwUtilsPlugin().captureWindow("Firestone - BattlegroundsWindow", "", false, null);
        }

        [TestMethod]
        public void TestFlash()
        {
            var openWindows = WindowUtils.GetOpenWindows();
            new OwUtilsPlugin().flashWindow("Hearthstone", null);
        }
    }
}
