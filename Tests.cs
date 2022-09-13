using NUnit.Framework;
using System.Drawing;

namespace Игра
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void TestMapSize()
        {
            var map = new Map(new string[]
            {
                "   ",
                "   ",
                "   ",
            }, "hero");
            Assert.AreEqual(5, map.Rows);
        }
        [Test]
        public void TestEnemyCount()
        {
            var map = new Map(new string[]
            {
                "   ",
                "   ",
                "   ",
            }, "hero", 2);
            Assert.AreEqual(2, map.Enemies.Length);
        }
        [Test]
        public void TestPlayerPosition()
        {
            var map = new Map(new string[]
           {
                "S  ",
                "   ",
                "   ",
           }, "hero");
            Assert.AreEqual(new Point(0, 1), map.Player.InitialPosition);
        }
    }
}
