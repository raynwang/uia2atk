// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 
//  
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// 
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com) 
// 
// Authors: 
//	Matt Guo <matt@mattguo.com>
// 

using System;
using SD = System.Drawing;
using System.Windows.Automation;
using SWF = System.Windows.Forms;
using System.Windows.Automation.Provider;

using Mono.UIAutomation.Bridge;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Events;
using Mono.UIAutomation.Winforms.Events.ToolStripSplitButton;

namespace Mono.UIAutomation.Winforms.Behaviors.ToolStripSplitButton
{
	internal class InvokeProviderBehavior
		: ProviderBehavior, IInvokeProvider
	{
#region Constructor
		public InvokeProviderBehavior (ToolStripSplitButtonProvider provider)
			: base (provider)
		{
			button = (SWF.ToolStripSplitButton) provider.Component;
		}
#endregion

#region IProviderBehavior Interface
		public override void Connect ()
		{
			Provider.SetEvent (ProviderEventType.InvokePatternInvokedEvent,
			                   new InvokePatternInvokedEvent (Provider));
		}

		public override void Disconnect ()
		{
			Provider.SetEvent (ProviderEventType.InvokePatternInvokedEvent,
			                   null);
		}

		public override AutomationPattern ProviderPattern {
			get { return InvokePatternIdentifiers.Pattern; }
		}
#endregion

#region IInvokeProvider Members
		public virtual void Invoke ()
		{
			if (!button.Enabled)
				throw new ElementNotEnabledException ();

			PerformButtonClick ();
		}

		private void PerformButtonClick ()
		{
			SWF.ToolStrip toolstrip = button.Owner;
			if (toolstrip.InvokeRequired) {
				toolstrip.BeginInvoke (new SWF.MethodInvoker (PerformButtonClick));
				return;
			}
			button.PerformButtonClick ();
		}
#endregion

#region Private Fields
		private SWF.ToolStripSplitButton button;
#endregion
	}
}
