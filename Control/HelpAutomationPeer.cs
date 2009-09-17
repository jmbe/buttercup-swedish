using System.Windows;
using System.Windows.Automation.Peers;

namespace Buttercup.Control
{
    public class HelpAutomationPeer : FrameworkElementAutomationPeer
    {
		#region Constructors (1) 

        public HelpAutomationPeer(FrameworkElement owner) : base(owner)
        {
        }

		#endregion Constructors 

		#region Methods (1) 

		// Protected Methods (1) 

        protected override bool IsContentElementCore()
        {
            return true;
        }

		#endregion Methods 
    }
}