﻿// Permission is hereby granted, free of charge, to any person obtaining 
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
using System.Drawing;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.Mono.UIAutomation.Winforms.Client
{
	//According to http://msdn.microsoft.com/en-us/library/ms743712.aspx
	[TestFixture]
	[Description ("Tests SWF.ScrollBar (SWF.HScrollBar/SWF.VScrollBar) as ControlType.ScrollBar")]
	public class ScrollBarTest : BaseTest
    {
		#region Properties

		[Test]
		[LameSpec, Description ("Value: Null | The scroll bar control does not have content elements and the NameProperty is not required to be set.")]
		public override void MsdnNamePropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (null,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.NameProperty, true),
				"NameProperty");
		}

		[Test]
		[LameSpec, Description ("Value: Not a number | Notes: The scroll bar control does not have clickable points. ")]
		public override void MsdnClickablePointPropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (double.NaN,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.ClickablePointProperty, true),
				"ClickablePointProperty");
		}

		[Test]
		[LameSpec, Description ("Value: null | Notes: Scroll bars do not have labels. ")]
		public override void MsdnLabeledByPropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (null,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.LabeledByProperty, true),
				"LabeledByProperty");
		}

		[Test]
		public override void MsdnLocalizedControlTypePropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual("scroll bar",
					child.GetCurrentPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty, true),
					"LocalizedControlTypeProperty");
		}

		[Test]
		public override void MsdnIsContentElementPropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (false,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.IsContentElementProperty, true),
				"IsContentElementProperty");
		}

		[Test]
		public override void MsdnIsControlElementPropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (true,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.IsControlElementProperty, true),
				"IsControlElementProperty");
		}

		[Test]
		[LameSpec, Description ("Value: True | Notes: The scroll bar control must always expose its horizontal or vertical orientation. ")]
		public override void MsdnOrientationPropertyTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			Assert.AreEqual (true,
				child.GetCurrentPropertyValue (AutomationElementIdentifiers.OrientationProperty, true),
				"OrientationProperty");
		}

		#endregion Properties

		#region Patterns

		[Test]
		[LameSpec, Description ("Support/Value: Depends | This functionality is required to be supported only if the Scroll control pattern is not supported on the container that has the scroll bar.")]
		public override void MsdnRangeValuePatternTest ()
		{
			AutomationElement child = GetScrollBarControl ();
			AutomationElement parent = TreeWalker.RawViewWalker.GetParent (child);

			bool parentSupportsScrollPattern = SupportsPattern (parent, ScrollPatternIdentifiers.Pattern);

			if (SupportsPattern (child, RangeValuePatternIdentifiers.Pattern) == true
				&& parentSupportsScrollPattern == true)
				Assert.Fail ("RangeValuePattern NOT SUPPORTED when parent supports ScrollPattern");
		}

		#endregion Patterns

		#region Protected Methods

		protected override AutomationElement GetAutomationElement ()
		{
			return GetScrollBarControl ();
		}

		#endregion

		#region Private Methods

		// This method gets ScrollBar as ControlType.ScrollBar
		private AutomationElement GetScrollBarControl ()
		{
			ListBox listbox = new ListBox ();
			listbox.Items.AddRange (new object [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
			listbox.Size = new Size (100, 100);
			listbox.Location = new Point (3, 3);

			AutomationElement listboxElement = GetAutomationElementFromControl (listbox);
			AutomationElement child = TreeWalker.RawViewWalker.GetFirstChild (listboxElement);
			AutomationElement scrollBarChild = null;

			Assert.IsNotNull (child, "ListBox should have children");
			while (child != null) {
				if (child.GetCurrentPropertyValue (AutomationElementIdentifiers.ControlTypeProperty, true)
					== ControlType.ScrollBar) {
					scrollBarChild = child;
					break;
				}
				child = TreeWalker.RawViewWalker.GetNextSibling (child);
			}

			Assert.IsNotNull (scrollBarChild, "We should have ScrollBar element");

			return scrollBarChild;
		}

		#endregion
	}
}
