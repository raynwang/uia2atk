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

using System;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using System.Windows;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Events;

namespace Mono.UIAutomation.Winforms.Behaviors
{
	
	internal class ComboBoxItemProviderBehavior 
		: ProviderBehavior, ISelectionItemProvider
	{

#region Constructor
		
		public ComboBoxItemProviderBehavior (ComboBoxItemProvider provider)
			: base (provider)
		{
		}
		
#endregion
		
#region IProviderBehavior Interface
		
		public override void Disconnect (Control control)
		{
			Provider.SetEvent (ProviderEventType.SelectionItemElementSelectedEvent, 
			                   null);
		}

		public override object GetPropertyValue (int propertyId)
		{
			if (propertyId == SelectionItemPatternIdentifiers.IsSelectedProperty.Id)
				return IsSelected;
			else if (propertyId == SelectionItemPatternIdentifiers.SelectionContainerProperty.Id)
				return SelectionContainer;
			else
				return base.GetPropertyValue (propertyId);
		}

		public override void Connect (Control control)
		{
			Provider.SetEvent (ProviderEventType.SelectionItemElementSelectedEvent, 
			                   new ComboBoxItemElementSelectedEvent (Provider));
		}
		
		public override AutomationPattern ProviderPattern { 
			get { return SelectionItemPatternIdentifiers.Pattern; }
		}
		
#endregion

#region ISelectionItemProvider Interface

		public void AddToSelection ()
		{
			if (IsSelected)
				return;
			
			IRawElementProviderSimple comboboxProvider
				= ((ComboBoxItemProvider) Provider).ComboBoxProvider;
			IRawElementProviderSimple[] selection 
				= (IRawElementProviderSimple[]) comboboxProvider.GetPropertyValue (SelectionPatternIdentifiers.SelectionProperty.Id);
			
			if (selection == null)
				Select ();
			else 
				throw new InvalidOperationException ();
		}

		public void RemoveFromSelection ()
		{
			IRawElementProviderSimple comboboxProvider
				= ((ComboBoxItemProvider) Provider).ComboBoxProvider;
			IRawElementProviderSimple[] selection 
				= (IRawElementProviderSimple[]) comboboxProvider.GetPropertyValue (SelectionPatternIdentifiers.SelectionProperty.Id);
			
			if (selection == null)
				return;
			else
				throw new InvalidOperationException ();			
		}

		public void Select ()
		{
			if (IsSelected)
				return;
			
			((ComboBoxItemProvider) Provider).ComboBoxControl.SelectedIndex 
				= ((ComboBoxItemProvider) Provider).Index;
		}

		public bool IsSelected {
			get { 
				return ((ComboBoxItemProvider) Provider).ComboBoxControl.SelectedIndex 
					== ((ComboBoxItemProvider) Provider).Index; 
			}
		}

		public IRawElementProviderSimple SelectionContainer {
			get { return ((ComboBoxItemProvider) Provider).ComboBoxProvider; }
		}

#endregion 

	}
}
