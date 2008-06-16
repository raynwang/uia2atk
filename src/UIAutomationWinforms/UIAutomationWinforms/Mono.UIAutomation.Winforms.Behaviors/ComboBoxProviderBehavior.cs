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
//	Mario Carrion <mcarrion@novell.com>
// 

using System.Windows.Forms;
using System.Windows.Automation;

namespace Mono.UIAutomation.Winforms.Behaviors
{

	public abstract class ComboBoxProviderBehavior : ProviderBehavior
	{
		
#region Constructors
		
		protected ComboBoxProviderBehavior (SimpleControlProvider provider)
			: base (provider) 
		{
		}

#endregion

#region Protected fields
		
		protected ComboBox combobox;
		
#endregion
		
#region IProviderBehavior Interface

		public override object GetPropertyValue (int propertyId)
		{
			if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
				return ControlType.ComboBox.Id;
			//FIXME: According the documentation this is valid, however you can
			//focus only when control.CanFocus, this doesn't make any sense.
			else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
				return true;
			else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
				return "combo box";
			else
				return null;
		}

		public override void Connect (Control control)
		{
			combobox = (ComboBox) control;
		}
		
		public override void Disconnect (Control control)
		{
		}
		
#endregion

	}
}
