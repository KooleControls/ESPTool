using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ExampleApp
{
    public static class Helpers
    {
        private delegate void SafeCallDelegate<T>(T c, Action action);
        public static void InvokeIfRequired<T>(this T c, Action action)  where T : Control
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new SafeCallDelegate<T>(InvokeIfRequired), new object[] { c, action });
            }
            else
            {
                action();
            }
        }
    }
}
