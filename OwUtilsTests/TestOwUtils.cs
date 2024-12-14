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

        [TestMethod]
        public void TestCopyFile()
        {
            new OwUtilsPlugin().copyFile(
                "E:\\Source\\bazaar\\bazaar-app\\dist\\apps\\bazaar-app\\Files\\game-libs\\BazaarTrackerInspector.dll",
                "e:/games/tempo launcher - beta/the bazaar game_64/bazaarwinprodlatest//BepInEx\\plugins",
                (a, b) => {
                    Console.WriteLine(a);
                    Console.WriteLine(b);
                });
        }
        [TestMethod]
        public void TestListFiles()
        {
            new OwUtilsPlugin().ListFilesInDirectory("E:\\Source\\bazaar\\bazaar-app\\dist\\apps\\bazaar-app\\game-libs\\UnitySpyBazaarPlugin.dll", (result) =>
            {
                Console.WriteLine(result);
            });
        }
    }
}
