using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace KokoroUpTime
{
    public static class ButtonExtensions
    {
        public static void PerformClick(this Button button)
        {
            if (button == null)
                throw new ArgumentNullException("button");

            var provider = new ButtonAutomationPeer(button) as IInvokeProvider;
            provider.Invoke();
        }
    }
}
