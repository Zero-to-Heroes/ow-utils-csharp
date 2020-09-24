using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwUtils;

namespace OwUtilsTests
{
    [TestClass]
    public class TestOwUtils
    {
        [TestMethod]
        public void BasicTest()
        {
            new OwUtilsPlugin().captureWindow("Firestone - BattlegroundsWindow", "", null);
        }
    }
}
