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
//      Mario Carrion <mcarrion@novell.com>
// 


using System;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using Mono.Unix;
using NUnit.Framework;
using Mono.UIAutomation.Winforms;
using Mono.UIAutomation.Winforms.Navigation;

namespace MonoTests.Mono.UIAutomation.Winforms
{
	public abstract class BaseProviderTest
	{
	
		#region Protected Fields
		
		protected MockBridge bridge;
		
		#endregion
		
		#region Private Fields
		
		private Form form;
		private FormProvider formProvider;
		
		#endregion
		
		#region Setup/Teardown

		[SetUp]
		public virtual void SetUp ()
		{			
			bridge = TestHelper.SetUpEnvironment ();
			form = new Form ();
			form.Show ();
			formProvider = (FormProvider) ProviderFactory.GetProvider (form);
		}
		
		[TearDown]
		public virtual void TearDown ()
		{
			TestHelper.TearDownEnvironment ();
			
			form.Close ();
			formProvider = null;
		}

		#endregion
		
		#region Basic Tests
		
		[Test]
		public virtual void IsEnabledPropertyTest ()
		{
			Control control = GetControlInstance ();
			if (control == null)
				return;
			
			IRawElementProviderSimple provider = ProviderFactory.GetProvider (control);
			
			bridge.ResetEventLists ();
			
			object initialVal =
				provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id);
			Assert.IsNotNull (initialVal, "Property returns null");
			Assert.IsTrue ((bool)initialVal, "Should initialize to true");
			
			control.Enabled = false;

			Assert.IsFalse ((bool)provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id),
			                "Toggle to false");

			AutomationPropertyChangedEventTuple tuple 
				= bridge.GetAutomationPropertyEventFrom (provider,
				                                         AutomationElementIdentifiers.IsEnabledProperty.Id);
			
			Assert.IsNotNull (tuple, "Tuple missing");
			Assert.AreEqual (initialVal,
			                 tuple.e.OldValue,
			                 string.Format ("1st. Old value should be true: '{0}'", tuple.e.OldValue));
			Assert.AreEqual (false,
			                 tuple.e.NewValue,
			                 string.Format ("1st. New value should be true: '{0}'", tuple.e.NewValue));
			
			bridge.ResetEventLists ();
			
			control.Enabled = true;
			Assert.IsTrue ((bool)provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id),
			               "Toggle to true");
			
			tuple 
				= bridge.GetAutomationPropertyEventFrom (provider,
				                                         AutomationElementIdentifiers.IsEnabledProperty.Id);
			Assert.IsNotNull (tuple, "Tuple missing");
			Assert.AreEqual (false,
			                 tuple.e.OldValue,
			                 string.Format ("2nd. Old value should be false: '{0}'", tuple.e.OldValue));
			Assert.AreEqual (true,
			                 tuple.e.NewValue,
			                 string.Format ("2nd. New value should be true: '{0}'", tuple.e.NewValue));
		}

		protected virtual bool IsContentElement {
			get { return true; }
		}

		[Test]
		public virtual void IsContentElementPropertyTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();
			
			TestProperty (provider,
			              AutomationElementIdentifiers.IsContentElementProperty,
			              IsContentElement);
		}

		protected virtual bool IsControlElement {
			get { return true; }
		}

		[Test]
		public virtual void IsControlElementPropertyTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();
			
			TestProperty (provider,
			              AutomationElementIdentifiers.IsControlElementProperty,
			              IsControlElement);
		}
		
		[Test]
		public virtual void AutomationIdPropertyTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();

			Assert.IsNotNull (provider.GetPropertyValue (AutomationElementIdentifiers.AutomationIdProperty.Id),
			                  "AutomationIdProperty should not be null.");
		}
		
		[Test]
		public virtual void BoundingRectanglePropertyTest ()
		{
			Control control = GetControlInstance ();
			if (control == null)
				return;

			IRawElementProviderSimple provider = GetProviderFromControl (control);
			
			try {
				Form.Show ();
				Form.Location = new System.Drawing.Point (0, 0);
				
				control.SetBounds (5, 6, 7, 8);
				System.Drawing.Rectangle screenRect =
					Form.RectangleToScreen (control.Bounds);
				Rect rect = new Rect (screenRect.X,
				                      screenRect.Y,
				                      7,
				                      8);
				
				TestProperty (provider,
				              AutomationElementIdentifiers.BoundingRectangleProperty,
				              rect);
			} catch (Exception) {}
		}
		
		[Test]
		public virtual void LabeledByAndNamePropertyTest ()
		{
			TestLabeledByAndName (true, true);
		}
		
		[Test]
		public virtual void ClickablePointPropertyTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();

			bool offscreen 
				= (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsOffscreenProperty.Id);
			object clickablePointObj 
				= provider.GetPropertyValue (AutomationElementIdentifiers.ClickablePointProperty.Id);

			Point clickablePoint = new Point (0, 0);

			// Clickable point should be either null or Rect.Empty
			if (offscreen) {
				if (clickablePointObj != null) {
					try {
						clickablePoint = (Point) clickablePointObj;
						Assert.IsTrue (clickablePoint.X >= 0,
						               string.Format ("X is negative, your provider should be OffScreen: {0}", clickablePoint.X));
						Assert.IsTrue (clickablePoint.Y >= 0,
						               string.Format ("Y is negative, your provider should be OffScreen: {0}", clickablePoint.Y));

					} catch (InvalidCastException) {
						Assert.Fail (string.Format ("You are not returning System.Windows.Point in ClickablePointProperty: {0}",
						                            clickablePoint,GetType ()));
					}
				}
			// Clickable point should intersect bounding rectangle...
			} else {
				if (clickablePointObj == null) // ...unless you are not clickable at all
					return;

				Assert.IsNotNull (clickablePoint, 
				                  "Your ClickablePointProperty should not be null.");

				try {
					clickablePoint = (Point) clickablePointObj;
					Assert.IsTrue (clickablePoint.X >= 0,
					               string.Format ("X is negative, your provider should be OffScreen: {0}", clickablePoint.X));
					Assert.IsTrue (clickablePoint.Y >= 0,
					               string.Format ("Y is negative, your provider should be OffScreen: {0}", clickablePoint.Y));

				} catch (InvalidCastException) {
					Assert.Fail (string.Format ("You are not returning System.Windows.Point in ClickablePointProperty: {0}",
					                            clickablePoint.GetType ()));
				}

				object boundingRectangle
					= provider.GetPropertyValue (AutomationElementIdentifiers.BoundingRectangleProperty.Id);
				Assert.IsNotNull (boundingRectangle, 
				                  "You need to return BoundingRectangle if you return ClickablePointProperty.");

				try {
					Rect boundingRectangleRect = (Rect) boundingRectangle;
					Assert.AreNotEqual (Rect.Empty, 
					                    boundingRectangleRect,
					                    "BoundingRectangle SHOULD NOT be Rect.Empty");

					Rect clickablePointRect = new Rect (clickablePoint.X, clickablePoint.Y, 1, 1);

					Assert.IsTrue (boundingRectangleRect.Contains (clickablePointRect),
					               string.Format ("ClickablePoint ({0}) SHOULD Intersect with BoundingRectangle: {1}",
					                              clickablePointRect.ToString (), 
					                              boundingRectangleRect.ToString ()));
					
				} catch (InvalidCastException) {
					Assert.Fail (string.Format ("You are not returning System.Windows.Rect in BoundingRectangle: {0}",
					                            boundingRectangle.GetType ()));
				}
			}
		}
		
		[Test]
		public virtual void IsKeyboardFocusablePropertyTest ()
		{
			Control control = GetControlInstance ();
			IRawElementProviderSimple provider = ProviderFactory.GetProvider (control);
			if (provider == null) {
				// This can be triggered by controls which
				// don't actually register themselves with
				// ProviderFactory (typically when they're
				// "fake" controls, e.g., they don't map to a
				// widget)
				return;
			}
			
			TestProperty (provider,
			              AutomationElementIdentifiers.IsKeyboardFocusableProperty,
			              control.CanFocus);
		}
		
		[Test]
		public virtual void FragmentRootAsParentTest ()
		{
			IRawElementProviderFragment fragment 
				= GetProvider () as IRawElementProviderFragment;
			if (fragment != null) {
				IRawElementProviderFragment parent = fragment.Navigate (NavigateDirection.Parent);
				
				//http://msdn.microsoft.com/en-us/library/system.windows.automation.provider.irawelementproviderfragment.fragmentroot.aspx
				if (parent == null)
					Assert.AreEqual (fragment, fragment.FragmentRoot, "FragmentRoot != Fragment");
				else
					Assert.AreEqual (parent, fragment.FragmentRoot, "FragmentRoot != Parent");
			}
		}

		#region Navigation Tests

		[Test]
		public void DefaultNavigationTest ()
		{
			IRawElementProviderFragment provider = GetProvider () as IRawElementProviderFragment;
			if (provider != null) {
				IRawElementProviderFragment child = provider.Navigate (NavigateDirection.FirstChild);
				while (child != null) {
					Assert.AreEqual (provider, 
					                 child.Navigate (NavigateDirection.Parent),
					                 "Child.Parent != provider");
					child = child.Navigate (NavigateDirection.NextSibling);
				}
			}
		}

		#endregion

		#region Pattern Tests

		[Test]
		public virtual void ButtonPatternsTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Button.Id)
				return;

			TestButtonPatterns (provider);
		}

		[Test]
		public virtual void CalendarPatternsTest ()
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Calendar.Id)
				return;

			TestCalendarPatterns (provider);
		}

		[Test]
		public virtual void CheckBoxPatternsTest () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.CheckBox.Id)
				return;

			TestCheckBoxPatterns (provider);
		}

		[Test]
		public virtual void ComboBoxPatternsTest () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ComboBox.Id)
				return;

			TestComboBoxPatterns (provider);
		}

		[Test]
		public virtual void PatternsDataGridTest () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.DataGrid.Id)
				return;
			
			TestDataGridPatterns (provider);
		}

		[Test]
		public virtual void PatternsDataItemTest () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.DataItem.Id)
				return;

			TestDataItemPatterns (provider);
		}

		[Test]
		public virtual void PatternsDocumentTest () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.DataItem.Id)
				return;

			TestDocumentPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsEdit () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Edit.Id)
				return;

			TestEditPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsGroup () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Group.Id)
				return;

			TestGroupPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsHeader () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Header.Id)
				return;

			TestHeaderPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsHeaderItem () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.HeaderItem.Id)
				return;

			TestHeaderItemPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsHyperlink () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Hyperlink.Id)
				return;

			TestHyperlinkPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsImage () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Hyperlink.Id)
				return;

			TestHyperlinkPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsList () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.List.Id)
				return;

			TestListPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsListItem () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ListItem.Id)
				return;

			TestListItemPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsMenu () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Menu.Id)
				return;

			TestMenuPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsMenuBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.MenuBar.Id)
				return;

			TestMenuBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsMenuItem ()
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.MenuItem.Id)
				return;

			TestMenuItemPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsPane () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Pane.Id)
				return;

			TestPanePatterns (provider);
		}

		[Test]
		public virtual void TestPatternsProgressBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ProgressBar.Id)
				return;

			TestProgressBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsRadioButton () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.RadioButton.Id)
				return;

			TestRadioButtonPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsScrollBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ScrollBar.Id)
				return;

			TestScrollBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsSeparator () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Separator.Id)
				return;

			TestSeparatorPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsSlider () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Slider.Id)
				return;

			TestSliderPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsSpinner () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Spinner.Id)
				return;

			TestSpinnerPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsSplitButton () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.SplitButton.Id)
				return;

			TestSplitButtonPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsStatusBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.StatusBar.Id)
				return;

			TestStatusBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTab () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Tab.Id)
				return;

			TestTabPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTabItem () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.TabItem.Id)
				return;

			TestTabItemPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTable () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Table.Id)
				return;

			TestTablePatterns (provider);
		}

		[Test]
		public virtual void TestPatternsText () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Text.Id)
				return;

			TestTextPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsThumb () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Thumb.Id)
				return;

			TestThumbPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTitleBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.TitleBar.Id)
				return;

			TestTitleBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsToolBar () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ToolBar.Id)
				return;

			TestToolBarPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsToolTip () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.ToolTip.Id)
				return;

			TestToolTipPatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTree () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Tree.Id)
				return;

			TestTreePatterns (provider);
		}

		[Test]
		public virtual void TestPatternsTreeItem () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.TreeItem.Id)
				return;

			TestTreeItemPatterns (provider);
		}


		[Test]
		public virtual void TestPatternsWindow () 
		{
			IRawElementProviderSimple provider = GetProvider ();

			if ((int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id)
			    != ControlType.Window.Id)
				return;

			TestWindowPatterns (provider);
		}

		#endregion
		
		#endregion
		
		#region Abstract Members
	
		protected abstract Control GetControlInstance ();

		#endregion
	
		#region Protected Helper Methods
		
		protected void TestProperty (IRawElementProviderSimple provider,
		                             AutomationProperty property,
		                             object expectedValue)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			TestHelper.TestAutomationProperty (provider, property, expectedValue);
		}
		
		internal FormProvider FormProvider {
			get { return formProvider; }
		}
		
		protected Form Form {
			get { return form; }
		}
		
		protected IRawElementProviderFragment GetProviderFromControl (Control control)
		{
			if (form.Contains (control) == false && form != control 
			    && control != null && ((control as Form) == null))
				form.Controls.Add (control);
			form.Size = new System.Drawing.Size (400, 400);

			return (IRawElementProviderFragment) ProviderFactory.GetProvider (control);
		}

		protected void TestLabeledByAndName (bool expectNonNullLabel, bool expectNameFromLabel)
		{
			TestLabeledByAndName (expectNonNullLabel, expectNameFromLabel, true, false, false);
		}
		
		/// <summary>
		/// Simple test of expected relationship between LabeledBy
		/// property and Name property.  Does not handle change events.
		/// </summary>
		/// <param name="expectNonNullLabel">
		/// A <see cref="System.Boolean"/> indicating whether or not a
		/// non-null value should be expected when getting LabeledBy.
		/// </param>
		/// <param name="expectNameFromLabel">
		/// A <see cref="System.Boolean"/> indicating whether or not the
		/// Name property should come from the provider returned by
		/// LabeledBy. Ignored if expectNonNullLabel is false.
		/// </param>
		/// <param name="expectNonNullName">
		/// A <see cref="System.Boolean"/> indicating whether or not a
		/// non-null value should be expected when getting Name. Ignored
		/// if expectNameFromLabel is true.
		/// </param>
		/// <param name="expectNameFromText">
		/// A <see cref="System.Boolean"/> indicating whether or not the
		/// Name property should come from Control.Text (a common case).
		/// Ignored if expectNameFromLabel is true.
		/// </param>
		protected void TestLabeledByAndName (bool expectNonNullLabel,
		                                     bool expectNameFromLabel, 
		                                     bool expectNonNullName, 
		                                     bool expectNameFromText, 
		                                     bool expectNameFromAccessible)
		{
			Control control = GetControlInstance ();
			if (control == null)
				return;
			
			Form f = control as Form;
			if (f != null)
				return;
			
			using (f = new Form ()) {
				f.Controls.Add (control);
				
				Label l = new Label ();
				l.Text = "my label";
				f.Controls.Add (l);				
				f.Show ();
			
				Type formListenerType = typeof (FormListener);
				MethodInfo onFormAddedMethod =
					formListenerType.GetMethod ("OnFormAdded", BindingFlags.Static | BindingFlags.NonPublic);
				onFormAddedMethod.Invoke (null, new object [] {f, null});
				
				IRawElementProviderSimple controlProvider =
					ProviderFactory.GetProvider (control);
				IRawElementProviderSimple labelProvider =
					ProviderFactory.GetProvider (l);
				
				object labeledBy = controlProvider.GetPropertyValue (AutomationElementIdentifiers.LabeledByProperty.Id);
				Assert.AreEqual (expectNonNullLabel ? labelProvider : null,
				                 labeledBy);

				if (expectNonNullLabel && expectNameFromLabel)
					Assert.AreEqual (labelProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id) as string,
					                 controlProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id) as string,
					                 "Control provider name should derive from label name.");
				else if (expectNonNullLabel && !expectNameFromLabel)
					Assert.IsTrue (labelProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id) as string ==
					               controlProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id) as string,
					               "Control provider name should not derive from label name.");
				
				if (!expectNameFromLabel && expectNonNullName && expectNameFromText && !expectNameFromAccessible)
					Assert.AreEqual (control.Text,
					                 controlProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id),
					                 "Control provider name should derive from control text.");
				else if (!expectNameFromLabel && expectNonNullName && !expectNameFromText && expectNameFromAccessible)
					Assert.AreEqual (control.AccessibleName,
					                 controlProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id),
					                 "Control provider name should derive from control AccessibleName.");
				else if (!expectNameFromLabel && !expectNonNullName)
					Assert.IsNull (controlProvider.GetPropertyValue (AutomationElementIdentifiers.NameProperty.Id),
					               "Control provider name should be null.");
				
				f.Close ();
			}
		}

		protected virtual IRawElementProviderSimple GetProvider ()
		{
			Control control = GetControlInstance ();
			if (control == null) {
				Assert.Fail ("You need to override GetProvider because you are testing a Component-based provider.");
				return null;
			} else
				return GetProviderFromControl (control);
		}

		#endregion

		#region Patterns Tests

		protected virtual void TestButtonPatterns (IRawElementProviderSimple provider)
		{
			// http://msdn.microsoft.com/en-us/library/ms742153.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id)
			               || (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTogglePatternAvailableProperty.Id),
			               "Button ControlType must support IInvokeProvider or IToggleProvider");
			
			Assert.AreEqual (Catalog.GetString ("button"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
			
			TestInvokePattern_InvokedEvent (provider);
		}
		
		protected virtual void TestCalendarPatterns (IRawElementProviderSimple provider)
		{
			// http://msdn.microsoft.com/en-us/library/ms753925.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsGridPatternAvailableProperty.Id),
			               "Calendar ControlType must support IGridProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id),
			               "Calendar ControlType must support ISelectionProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id),
			               "Calendar ControlType must support ITableProvider");
			
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsValuePatternAvailableProperty.Id),
			               "Calendar ControlType must NOT support IValueProvider");

			Assert.AreEqual (Catalog.GetString ("calendar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");

			// DEPENDS: IScrollProvider
		}

		protected virtual void TestCheckBoxPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms751693.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTogglePatternAvailableProperty.Id),
			               "CheckBox ControlType must support IToggleProvider");

			Assert.AreEqual (Catalog.GetString ("check box"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");

			//A control must cycle through its ToggleState in the following order: On, Off and, if supported, Indeterminate.
			TestTogglePattern_ToggleStatePropertyEvent (provider);
		}
		
		protected virtual void TestComboBoxPatterns (IRawElementProviderSimple provider) 
		{
			//http://msdn.microsoft.com/en-us/library/ms752305.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsExpandCollapsePatternAvailableProperty.Id),
			               "ComboBox ControlType must support IExpandCollapseProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id),
			               "ComboBox ControlType must support ISelectionProvider");
			
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsScrollPatternAvailableProperty.Id),
			                "ComboBox ControlType must NOT support IScrollProvider");

			Assert.AreEqual (Catalog.GetString ("combo box"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");

			// DEPENDS: IValueProvider
		}

		protected virtual void TestDataGridPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms752044.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsGridPatternAvailableProperty.Id),
			               "DataGrid ControlType must support IGridProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id),
			               "DataGrid ControlType must support ISelectionProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id),
			               "DataGrid ControlType must support ITableProvider");

			Assert.AreEqual (Catalog.GetString ("data grid"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");

			// DEPENDS: IScrollProvider
		}

		protected virtual void TestDataItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742561.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
			               "DataItem ControlType must support ISelectionItemProvider");

			// ITableItemProvider: (DEPENDS)
			// If the data item is contained within a Data Grid control type then it will support this pattern.
			IRawElementProviderFragment fragment = provider as IRawElementProviderFragment;
			if (fragment != null) {
				IRawElementProviderFragment parent = fragment.Navigate (NavigateDirection.Parent);
				if (parent != null) {
					if (ControlType.DataGrid.Id
					    == (int) parent.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id))
						Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTableItemPatternAvailableProperty.Id),
						               "DataItem ControlType must support ITableItemProvider when parent is DataGrid");
				}
			}

			Assert.AreEqual (Catalog.GetString ("data item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");

			// DEPENDS: IExpandCollapseProvider
			// DEPENDS: IGridItemProvider
			// DEPENDS: IScrollItemProvider
			// DEPENDS: IToggleProvider
			// DEPENDS: IValueProvider

			TestSelectionPatternChild (provider);
			TestGridPatternChild (provider);
			TestTablePatternChild (provider);
		}

		protected virtual void TestDocumentPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms752058.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTextPatternAvailableProperty.Id),
			               "Document ControlType must support ITextProvider");

			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsValuePatternAvailableProperty.Id),
			                "Document ControlType must NOT support IValueProvider");

			Assert.AreEqual (Catalog.GetString ("document"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
			
			// DEPENDS: IScrollProvider
		}

		protected virtual void TestEditPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms748367.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTextPatternAvailableProperty.Id),
			               "Edit ControlType must support ITextProvider");
			// DEPENDS: IValueProvider
			// DEPENDS: IRangeValueProvider

			Assert.AreEqual (Catalog.GetString ("edit"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestGroupPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742689.aspx
			// DEPENDS: IExpandCollapseProvider

			Assert.AreEqual (Catalog.GetString ("group"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestHeaderPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms753110.aspx
			// DEPENDS: ITransformProvider

			Assert.AreEqual (Catalog.GetString ("header"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestHeaderItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742202.aspx
			// DEPENDS: ITransformProvider
			// DEPENDS: IInvokeProvider

			Assert.AreEqual (Catalog.GetString ("header item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestHyperlinkPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742530.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id),
			               "Hyperlink ControlType must support IInvokeProvider");

			Assert.AreEqual (Catalog.GetString ("hyperlink"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestImagePatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms746603.aspx
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id),
			               "Image ControlType must NOT support IInvokeProvider");
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
			               "Image ControlType must NOT support ISelectionItemProvider");

			// DEPENDS: ITableItemProvider
			// DEPENDS: IGridItemProvider

			Assert.AreEqual (Catalog.GetString ("image"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestListPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742462.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id),
			               "List ControlType must support ISelectionProvider");

			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id),
			                "List ControlType must NOT support ITableProvider");

			// DEPENDS: IScrollProvider
			// DEPENDS: IGridProvider
			// DEPENDS: IMultipleViewProvider

			Assert.AreEqual (Catalog.GetString ("list"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestListItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms744765.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
			               "ListItem ControlType must support ISelectionItemProvider");

			// DEPENDS: IScrollItemProvider
			// DEPENDS: IToggleProvider
			// DEPENDS: IExpandCollapseProvider
			// DEPENDS: IValueProvider
			// DEPENDS: IGridItemProvider
			// DEPENDS: IInvokeProvider

			Assert.AreEqual (Catalog.GetString ("list item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
			
			TestSelectionPatternChild (provider);
			TestGridPatternChild (provider);
			TestTablePatternChild (provider);
		}

		protected virtual void TestMenuPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms741841.aspx
			
			// NO PATTERNS REQUIRED
		}

		protected virtual void TestMenuBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms752322.aspx
			
			// DEPENDS: IExpandCollapseProvider
			// DEPENDS: IDockProvider
			// DEPENDS: ITransformProvider

			Assert.AreEqual (Catalog.GetString ("menu bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestMenuItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms746680.aspx
			
			// DEPENDS: IExpandCollapseProvider
			// DEPENDS: IInvokeProvider
			// DEPENDS: IToggleProvider
			// DEPENDS: ISelectionItemProvider

			Assert.AreEqual (Catalog.GetString ("menu item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestPanePatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms749129.aspx
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsWindowPatternAvailableProperty.Id),
			                "Pane ControlType must NOT support IWindowProvider");
			
			// DEPENDS: ITransformProvider
			// DEPENDS: IDockProvider

			Assert.AreEqual (Catalog.GetString ("pane"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestProgressBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms743681.aspx

			// DEPENDS: IValueProvider
			// DEPENDS: IRangeValueProvider

			Assert.AreEqual (Catalog.GetString ("progress bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestRadioButtonPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms750484.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
			                "RadioButton ControlType must support ISelectionItemProvider");
			
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTogglePatternAvailableProperty.Id),
			                "RadioButton ControlType must support IToggleProvider");

			Assert.AreEqual (Catalog.GetString ("radio button"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestScrollBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms743712.aspx
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsScrollPatternAvailableProperty.Id),
			                "ScrollBar ControlType must support IScrollProvider");

			// DEPENDS: IRangeValueProvider

			Assert.AreEqual (Catalog.GetString ("scroll bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestSeparatorPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms750550.aspx

			// NO PATTERNS REQUIRED

			Assert.AreEqual (Catalog.GetString ("separator"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");			
		}

		protected virtual void TestSliderPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742106.aspx

			// DEPENDS: ISelectionProvider
			// DEPENDS: IRangeValueProvider

			Assert.AreEqual (Catalog.GetString ("slider"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestSpinnerPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms744847.aspx

			// DEPENDS: ISelectionProvider
			// DEPENDS: IRangeValueProvider
			// DEPENDS: IValueProvider

			Assert.AreEqual (Catalog.GetString ("spinner"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestSplitButtonPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742545.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsExpandCollapsePatternAvailableProperty.Id),
			                "SplitButton ControlType must support IExpandCollapseProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id),
			                "SplitButton ControlType must support IInvokeProvider");

			Assert.AreEqual (Catalog.GetString ("split button"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestStatusBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms745809.aspx

			// DEPENDS: IGridProvider

			Assert.AreEqual (Catalog.GetString ("status bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");			
		}

		protected virtual void TestTabPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms754207.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id),
			                "Tab ControlType must support ISelectionProvider");

			// DEPENDS: IScrollProvider

			Assert.AreEqual (Catalog.GetString ("tab"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestTabItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms751611.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
			                "TabItem ControlType must support ISelectionItemProvider");
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id),
			                "TabItem ControlType must support IInvokeProvider");

			Assert.AreEqual (Catalog.GetString ("tab item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestTablePatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms750608.aspx
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsGridPatternAvailableProperty.Id),
			                "Table ControlType must support IGridProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id),
			                "Table ControlType must support ITableProvider");

			// DEPENDS: ITableItemProvider

			Assert.AreEqual (Catalog.GetString ("table"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestTextPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms752073.aspx
			Assert.IsFalse ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsValuePatternAvailableProperty.Id),
			                "Text ControlType must NOT support IValueProvider");

			// DEPENDS: ITextProvider
			// DEPENDS: ITableItemProvider
			// DEPENDS: IRangeValueProvider

			Assert.AreEqual (Catalog.GetString ("text"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestThumbPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms742539.aspx

			// DEPENDS: IRangeValueProvider
			// DEPENDS: ITransformProvider

			Assert.AreEqual (Catalog.GetString ("thumb"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}


		protected virtual void TestTitleBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms744939.aspx

			// NO REQUIRED PATTERNS

			Assert.AreEqual (Catalog.GetString ("title bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestToolBarPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms750425.aspx

			// DEPENDS: IExpandCollapsePattern
			// DEPENDS: IDockPattern
			// DEPENDS: TransformPattern

			Assert.AreEqual (Catalog.GetString ("tool bar"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestToolTipPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms752335.aspx

			// DEPENDS: IWindowProvider
			// DEPENDS: ITextProvider

			Assert.AreEqual (Catalog.GetString ("tool tip"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestTreePatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms743706.aspx

			// DEPENDS: ISelectionProvider
			// DEPENDS: IScrollProvider

			Assert.AreEqual (Catalog.GetString ("tree"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestTreeItemPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms743384.aspx

			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsExpandCollapsePatternAvailableProperty.Id),
			               "TreeItem ControlType must support IExpandCollapseProvider");

			// DEPENDS: IInvokeProvider
			// DEPENDS: IScrollItemProvider
			// DEPENDS: ISelectionItemProvider
			// DEPENDS: IToggleProvider
			
			Assert.AreEqual (Catalog.GetString ("tree item"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestWindowPatterns (IRawElementProviderSimple provider) 
		{
			// http://msdn.microsoft.com/en-us/library/ms746673.aspx

			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsTransformPatternAvailableProperty.Id),
			               "Window ControlType must support ITransformProvider");
			Assert.IsTrue ((bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsWindowPatternAvailableProperty.Id),
			               "Window ControlType must support IWindowProvider");

			// DEPENDS: IDockProvider

			Assert.AreEqual (Catalog.GetString ("window"),
			                 provider.GetPropertyValue (AutomationElementIdentifiers.LocalizedControlTypeProperty.Id),
			                 "LocalizedControlTypeProperty");
		}

		protected virtual void TestGridPatternChild (IRawElementProviderSimple provider)
		{
			IRawElementProviderFragment child = provider as IRawElementProviderFragment;
			if (child == null)
				return;

			IRawElementProviderFragment parent = child.Navigate (NavigateDirection.Parent);
			if (parent == null)
				return;
			
			if ((bool) parent.GetPropertyValue (AutomationElementIdentifiers.IsGridPatternAvailableProperty.Id)) {
				Assert.IsTrue ((bool) child.GetPropertyValue (AutomationElementIdentifiers.IsGridItemPatternAvailableProperty.Id),
				               string.Format ("{0} must support IGridItemProvider", child.GetType ()));
				child = child.Navigate (NavigateDirection.NextSibling);
			}
		}

		protected void TestSelectionPatternChild (IRawElementProviderSimple provider)
		{
			IRawElementProviderFragment child = provider as IRawElementProviderFragment;
			if (child == null)
				return;

			IRawElementProviderFragment parent = child.Navigate (NavigateDirection.Parent);
			if (parent == null)
				return;
			
			if ((bool) parent.GetPropertyValue (AutomationElementIdentifiers.IsSelectionPatternAvailableProperty.Id)) {
				Assert.IsTrue ((bool) child.GetPropertyValue (AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id),
				               string.Format ("{0} must support ISelectionItemProvider", child.GetType ()));
				child = child.Navigate (NavigateDirection.NextSibling);
			}
		}

		protected void TestTablePatternChild (IRawElementProviderSimple provider)
		{
			IRawElementProviderFragment child = provider as IRawElementProviderFragment;
			if (child == null)
				return;

			IRawElementProviderFragment parent = child.Navigate (NavigateDirection.Parent);
			if (parent == null)
				return;
			
			if ((bool) parent.GetPropertyValue (AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id)) {
				Assert.IsTrue ((bool) child.GetPropertyValue (AutomationElementIdentifiers.IsTableItemPatternAvailableProperty.Id),
				               string.Format ("{0} must support ITableItemProvider", child.GetType ()));
				child = child.Navigate (NavigateDirection.NextSibling);
			}
		}

		protected void TestPatterns (IRawElementProviderSimple provider)
		{
			int ctype = (int) provider.GetPropertyValue (AutomationElementIdentifiers.ControlTypeProperty.Id);
			if (ctype == ControlType.Button.Id)
				TestButtonPatterns (provider);
			else if (ctype == ControlType.Calendar.Id)
				TestCalendarPatterns (provider);
			else if (ctype == ControlType.CheckBox.Id)
				TestCheckBoxPatterns (provider);
			else if (ctype == ControlType.ComboBox.Id)
				TestComboBoxPatterns (provider);
			else if (ctype == ControlType.DataGrid.Id)
				TestDataGridPatterns (provider);
			else if (ctype == ControlType.DataItem.Id)
				TestDataItemPatterns (provider);
			else if (ctype == ControlType.DataItem.Id)
				TestDocumentPatterns (provider);
			else if (ctype == ControlType.Edit.Id)
				TestEditPatterns (provider);
			else if (ctype == ControlType.Group.Id)
				TestGroupPatterns (provider);
			else if (ctype == ControlType.Header.Id)
				TestHeaderPatterns (provider);
			else if (ctype == ControlType.HeaderItem.Id)
				TestHeaderItemPatterns (provider);
			else if (ctype == ControlType.Hyperlink.Id)
				TestHyperlinkPatterns (provider);
			else if (ctype == ControlType.Hyperlink.Id)
				TestHyperlinkPatterns (provider);
			else if (ctype == ControlType.List.Id)
				TestListPatterns (provider);
			else if (ctype == ControlType.ListItem.Id)
				TestListItemPatterns (provider);
			else if (ctype == ControlType.Menu.Id)
				TestMenuPatterns (provider);
			else if (ctype == ControlType.MenuBar.Id)
				TestMenuBarPatterns (provider);
			else if (ctype == ControlType.MenuItem.Id)
				TestMenuItemPatterns (provider);
			else if (ctype == ControlType.Pane.Id)
				TestPanePatterns (provider);
			else if (ctype == ControlType.ProgressBar.Id)
				TestProgressBarPatterns (provider);
			else if (ctype == ControlType.RadioButton.Id)
				TestRadioButtonPatterns (provider);
			else if (ctype == ControlType.ScrollBar.Id)
				TestScrollBarPatterns (provider);
			else if (ctype == ControlType.Separator.Id)
				TestSeparatorPatterns (provider);
			else if (ctype == ControlType.Slider.Id)
				TestSliderPatterns (provider);
			else if (ctype == ControlType.Spinner.Id)
				TestSpinnerPatterns (provider);
			else if (ctype == ControlType.SplitButton.Id)
				TestSplitButtonPatterns (provider);
			else if (ctype == ControlType.StatusBar.Id)
				TestStatusBarPatterns (provider);
			else if (ctype == ControlType.Tab.Id)
				TestTabPatterns (provider);
			else if (ctype == ControlType.TabItem.Id)
				TestTabItemPatterns (provider);
			else if (ctype == ControlType.Table.Id)
				TestTablePatterns (provider);
			else if (ctype == ControlType.Text.Id)
				TestTextPatterns (provider);
			else if (ctype == ControlType.Thumb.Id)
				TestThumbPatterns (provider);
			else if (ctype == ControlType.TitleBar.Id)
				TestTitleBarPatterns (provider);
			else if (ctype == ControlType.ToolBar.Id)
				TestToolBarPatterns (provider);
			else if (ctype == ControlType.ToolTip.Id)
				TestToolTipPatterns (provider);
			else if (ctype == ControlType.Tree.Id)
				TestTreePatterns (provider);
			else if (ctype == ControlType.TreeItem.Id)
				TestTreeItemPatterns (provider);
			else if (ctype == ControlType.Window.Id)
				TestWindowPatterns (provider);
		}

		protected void TestChildPatterns (IRawElementProviderFragment root)
		{
			IRawElementProviderFragment provider = root.Navigate (NavigateDirection.FirstChild);
			while (provider != null) {
				TestPatterns (provider);
				provider = provider.Navigate (NavigateDirection.NextSibling);
			}
		}

		#endregion

		#region Test Events for each patterns

		protected virtual void TestInvokePattern_InvokedEvent (IRawElementProviderSimple provider)
		{
			IInvokeProvider invokeProvider 
				= provider.GetPatternProvider (InvokePatternIdentifiers.Pattern.Id) as IInvokeProvider;
			if (invokeProvider == null)
				Assert.Fail ("Provider {0} is not implementing IInvokeProvider", provider.GetType ());

			bridge.ResetEventLists ();

			bool enabled 
				= (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id);
			try {
				invokeProvider.Invoke ();
				Assert.IsNotNull (bridge.GetAutomationEventFrom (provider, InvokePatternIdentifiers.InvokedEvent.Id),
				                  "IInvokeProvider.Invoke didn't raise any event.");
			} catch (ElementNotEnabledException) {
				// According to http://msdn.microsoft.com/en-us/library/system.windows.automation.provider.iinvokeprovider.invoke.aspx
				if (!enabled)
					Assert.Fail ("Your provider is disabled but didn't throw ElementNotEnabledException.");
			}
		}

		protected virtual void TestTogglePattern_ToggleStatePropertyEvent (IRawElementProviderSimple provider)
		{
			IToggleProvider toggleProvider
				= provider.GetPatternProvider (TogglePatternIdentifiers.Pattern.Id) as IToggleProvider;
			if (toggleProvider == null)
				Assert.Fail ("Provider {0} is not implementing IToggleProvider", provider.GetType ());


			// A control must cycle through its toggle states in this order:
			// On, Off, Indeterminate
			bool enabled 
				= (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id);
			ToggleState toggleState = toggleProvider.ToggleState;
			ToggleState firstState = toggleState;

			try {
				bridge.ResetEventLists ();				
				do {
					bridge.ResetEventLists ();
					toggleProvider.Toggle ();
					Assert.IsNotNull (bridge.GetAutomationPropertyEventFrom (provider, TogglePatternIdentifiers.ToggleStateProperty.Id),
					                  string.Format ("toggleProvider.Toggle didn't raise any event. Old {0} Current: {1}", 
					                                 toggleState,
					                                 toggleProvider.ToggleState));
					ValidateToggleState  (toggleState, toggleProvider.ToggleState);
					toggleState = toggleProvider.ToggleState;
				} while (firstState != toggleState);
			} catch (ElementNotEnabledException) {
				if (!enabled)
					Assert.Fail ("Your provider is disabled but didn't throw ElementNotEnabledException.");
			}			
		}

		protected virtual void TestValuePattern_ValuePropertyEvent (IRawElementProviderSimple provider)
		{
			IValueProvider valueProvider 
				= provider.GetPatternProvider (ValuePatternIdentifiers.Pattern.Id) as IValueProvider;
			if (valueProvider == null)
				Assert.Fail ("Provider {0} is not implementing IValueProvider", provider.GetType ());

			bool enabled 
				= (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id);
			Assert.AreEqual (!enabled,
			                 valueProvider.IsReadOnly, 
			                 "!Enabled and IValueProvider.IsReadOnly should be equal");
			try {
				string newValue = "I'm your new value!";
				bridge.ResetEventLists ();
				valueProvider.SetValue (newValue);
				Assert.IsNotNull (bridge.GetAutomationPropertyEventFrom (provider, ValuePatternIdentifiers.ValueProperty.Id),
				                  "IValueProvider.Value didn't raise any event.");
				Assert.AreEqual (newValue,
				                 valueProvider.Value, 
				                 "Different value even when Value event was raised");
			} catch (ElementNotEnabledException) {
				// According to http://msdn.microsoft.com/en-us/library/system.windows.automation.provider.ivalueprovider.setvalue.aspx
				if (!enabled)
					Assert.Fail ("Your provider is disabled but didn't throw ElementNotEnabledException.");
			}
		}

		protected virtual void TestValuePattern_IsReadOnlyPropertyEvent (IRawElementProviderSimple provider)
		{
			IValueProvider valueProvider 
				= provider.GetPatternProvider (ValuePatternIdentifiers.Pattern.Id) as IValueProvider;
			if (valueProvider == null)
				Assert.Fail ("Provider {0} is not implementing IValueProvider", provider.GetType ());

			bool enabled 
				= (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id);
			Assert.AreEqual (!enabled,
			                 valueProvider.IsReadOnly, 
			                 "!Enabled and IValueProvider.IsReadOnly should be equal");
			
			bool newValue = enabled;

			bridge.ResetEventLists ();
			TestValuePattern_ChangeReadOnly (provider, newValue);
			AutomationPropertyChangedEventTuple tuple
				= bridge.GetAutomationPropertyEventFrom (provider, ValuePatternIdentifiers.IsReadOnlyProperty.Id);
			Assert.IsNotNull (tuple,
			                  "IValueProvider.IsReadOnly didn't raise any event.");
			Assert.AreEqual (newValue, tuple.e.NewValue, "Value is invalid in event.");
			Assert.AreEqual (!newValue, tuple.e.OldValue, "Value is invalid in event.");

			Assert.AreEqual (newValue,
			                 valueProvider.IsReadOnly, 
			                 "IsReadOnly value not changed.");
			Assert.AreEqual (!newValue,
			                 (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id), 
			                 "Enabled value not changed.");

			// Changing again
			newValue = !newValue;
			
			bridge.ResetEventLists ();
			TestValuePattern_ChangeReadOnly (provider, newValue);
			tuple = bridge.GetAutomationPropertyEventFrom (provider, ValuePatternIdentifiers.IsReadOnlyProperty.Id);
			Assert.IsNotNull (tuple,
			                  "IValueProvider.IsReadOnly didn't raise any event.");
			Assert.AreEqual (newValue, tuple.e.NewValue, "Value is invalid in event.");
			Assert.AreEqual (!newValue, tuple.e.OldValue, "Value is invalid in event.");

			Assert.AreEqual (newValue,
			                 valueProvider.IsReadOnly, 
			                 "IsReadOnly value not changed.");
			Assert.AreEqual (!newValue,
			                 (bool) provider.GetPropertyValue (AutomationElementIdentifiers.IsEnabledProperty.Id), 
			                 "Enabled value not changed.");
		}

		protected virtual void TestValuePattern_ChangeReadOnly (IRawElementProviderSimple provider, 
		                                                        bool newValue)
		{
			// This must be overridden by providers to actually change the value!
		}

		private void ValidateToggleState (ToggleState old, ToggleState got)
		{
			if (old == ToggleState.On)
				Assert.AreEqual (ToggleState.Off, got, "On -> Off");
			else if (old == ToggleState.Off) {
				if (got != ToggleState.Indeterminate 
				    && got != ToggleState.On)
					Assert.Fail ("Off -> Indeterminate or On");
			} else if (old == ToggleState.Indeterminate)
				Assert.AreEqual (ToggleState.On, got, "Indeterminate -> On");
		}

		#endregion
		
	}
	
}
