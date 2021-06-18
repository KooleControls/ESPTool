using ESPTool.Com;
using ESPTool.Loaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace TestProject
{
    [TestClass]
    public class ESP32Test
    {
        
        [TestMethod]
        public async void TestEnterBootloader()
        {
            Communicator communicator = new Communicator();
            communicator.OpenSerial("COM30", 115200);
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            bool result = await communicator.EnterBootloader(cts.Token);
            Assert.IsTrue(result);
        }




    }
}
