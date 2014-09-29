using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;

namespace Automation
{
    class PlaceOrders
    {
        private int _ordersPerDrone;
        private int _drones;

        public PlaceOrders()
        {
            _drones = 5;
            _ordersPerDrone = 10;
        }
        [Test]
        public void PlaceOrder()
        {
            for (int i = 0; i < _drones; i++)
            {
                Task t = new Task(LaunchLoad);
                t.Start();
            }
        }

        public void LaunchLoad()
        {
            ChromeDriver cd = new ChromeDriver();
            cd.Navigate().GoToUrl("http://localhost:20233");
            Thread.Sleep(1000 * 20);

            for (int i = 0; i < _ordersPerDrone; i++)
            {
                cd.FindElementById("intro1").Click();
                cd.FindElementById("btnPlaceOrder").Click();
                Console.WriteLine("Click number " + i);
                Thread.Sleep(500);
            }

            cd.Quit();
        }
    }
}
