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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
// 
// Authors: 
//	Neville Gao <nevillegao@gmail.com>
// 

using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using SWF = System.Windows.Forms;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Events;
using Mono.UIAutomation.Winforms.Events.UpDownBase;

namespace Mono.UIAutomation.Winforms.Behaviors.UpDownBase
{
	internal class ButtonInvokeProviderBehavior : ProviderBehavior, IInvokeProvider
	{
		#region Constructor
		
		public ButtonInvokeProviderBehavior (UpDownBaseProvider.UpDownBaseButtonProvider provider)
			: base (provider)
		{
			this.provider = provider;
		}
		
		#endregion
		
		#region IProviderBehavior Interface
		
		public override AutomationPattern ProviderPattern {
			get { return InvokePatternIdentifiers.Pattern; }
		}
		
		public override void Connect ()
		{
			Provider.SetEvent (ProviderEventType.InvokePatternInvokedEvent,
			                   new ButtonInvokePatternInvokedEvent (Provider));
		}
		
		public override void Disconnect ()
		{
			Provider.SetEvent (ProviderEventType.InvokePatternInvokedEvent,
			                   null);
		}
		
		#endregion
		
		#region IInvokeProvider Members
		
		public void Invoke ()
		{
			if (provider.UpDownBase.InvokeRequired) {
				provider.UpDownBase.BeginInvoke (new SWF.MethodInvoker (PerformInvoke));
				return;
			}

			PerformInvoke ();
		}

		private void PerformInvoke ()
		{
			if (provider.Orientation == UpDownBaseProvider.UpDownBaseButtonOrientation.Forward)
				provider.UpDownBase.UpButton ();
			else if (provider.Orientation == UpDownBaseProvider.UpDownBaseButtonOrientation.Backward)
				provider.UpDownBase.DownButton ();
		}
		
		#endregion
		
		#region Private Fields
		
		private UpDownBaseProvider.UpDownBaseButtonProvider provider;
		
		#endregion
	}
}