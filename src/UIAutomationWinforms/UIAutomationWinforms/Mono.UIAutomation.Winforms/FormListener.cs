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
//      Sandy Armstrong <sanfordarmstrong@gmail.com>
// 

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace Mono.UIAutomation.Winforms
{
	public class FormListener
	{
#region Private Static Members
		
		static bool initialized = false;
		static Dictionary<Form, WindowProvider> formProviders =
			new Dictionary<Form, WindowProvider> ();
		
#endregion
		
#region Public Static Methods
		
		/// <summary>
		/// Set up the FormListener class to listen to winforms
		/// events that will allow the correct UIA providers to be
		/// created and the correct UIA events to be fired.
		/// 
		/// This method is called via reflection from the
		/// System.Windows.Forms.Application class.
		/// </summary>
		public static void Initialize ()
		{
			if (initialized)
				return;
			
			Console.WriteLine ("FormListener Initialized");
			
			Type appType = typeof (Application);
			// NOTE: FormAdded is fired too frequently (such as
			//       when the form comes into focus).  A different
			//       event is probably more appropriate.
			EventInfo formAddedEvent =
				appType.GetEvent("FormAdded",
				                 BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo formAddedEventAddMethod =
				formAddedEvent.GetAddMethod(true);
			formAddedEventAddMethod.Invoke(null,
			                               new object[]{new EventHandler(OnFormAdded)});
			
			// OnRun
			EventInfo preRunEvent =
				appType.GetEvent("PreRun",
				                 BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo preRunEventAddMethod =
				preRunEvent.GetAddMethod(true);
			preRunEventAddMethod.Invoke(null,
			                               new object[]{new EventHandler(OnPreRun)});
		}
		
#endregion
		
#region Static Event Handlers
		
		/// <summary>
		/// Start GLib mainloop in its own thread just before
		/// winforms mainloop starts
		/// </summary>
		static void OnPreRun (object sender, EventArgs args)
		{
			Console.WriteLine ("PreRun fired");
			
			// TODO: Change this temporary hack to pass on the PreRun event
			AutomationInteropProvider.RaiseAutomationEvent (null, null, null);
		}
		
		static void OnFormAdded (object sender, EventArgs args)
		{
			Console.WriteLine ("Form added!");
			Form f = (Form) sender;
			if (formProviders.ContainsKey (f))
				return;
			WindowProvider provider = new WindowProvider (f);
			formProviders [f] = provider;
			
			// TODO: Fill in rest of eventargs
			AutomationInteropProvider.RaiseStructureChangedEvent (
			  provider,
			  new StructureChangedEventArgs (StructureChangeType.ChildrenBulkAdded,
			                                 new int [] {0}));
			
			// HACK: This is just to make sure control providers
			//       aren't sent to bridge until the parent's already
			//       there.  There are about 100 ways to do this
			//       better.
			foreach (IRawElementProviderSimple control in provider.controlProviders.Values) {
				// TODO: Fill in rest of eventargs
				if (control == null)
					break;
				AutomationInteropProvider.RaiseStructureChangedEvent (
				  control,
				  new StructureChangedEventArgs (StructureChangeType.ChildrenBulkAdded,
				                                 new int [] {0}));
			}
		}
		
#endregion
	}
}
