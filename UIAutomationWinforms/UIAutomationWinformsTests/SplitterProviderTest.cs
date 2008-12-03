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
using System.Windows.Forms;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using Mono.UIAutomation.Winforms;
using NUnit.Framework;

namespace MonoTests.Mono.UIAutomation.Winforms
{
	[TestFixture]
	public class SplitterProviderTest : BaseProviderTest
	{
		#region Test
		
		[Test]
		public void BasicPropertiesTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider =
				ProviderFactory.GetProvider (splitter);
			
			TestProperty (provider,
			              AutomationElementIdentifiers.ControlTypeProperty,
			              ControlType.Pane.Id);
			
			TestProperty (provider,
			              AutomationElementIdentifiers.LocalizedControlTypeProperty,
			              "pane");
		}
		
		[Test]
		public void ProviderPatternTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			object transformProvider =
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			Assert.IsTrue (transformProvider is ITransformProvider,
			               "Not returning TransformPatternIdentifiers.");
			
			object dockProvider =
				provider.GetPatternProvider (DockPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (dockProvider,
			                  "Not returning DockPatternIdentifiers.");
			Assert.IsTrue (dockProvider is IDockProvider,
			               "Not returning DockPatternIdentifiers.");
		}
		
		#endregion
		
		#region ITransformProvider Test
		
		[Test]
		public void ITransformProviderCanMoveTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			Assert.IsFalse (transformProvider.CanMove,
			               "Splitter can't be moved.");
		}
		
		[Test]
		public void ITransformProviderCanResizeTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			Assert.IsTrue (transformProvider.CanResize,
			               "Splitter can be resized.");
		}
		
		[Test]
		public void ITransformProviderCanRotateTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			Assert.IsFalse (transformProvider.CanRotate,
			                "Splitter can't be rotated.");
		}
		
		[Test]
		public void ITransformProviderMoveTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			try {
				double x = 10, y = 10;
				transformProvider.Move (x, y);
				Assert.Fail ("InvalidOperationException not thrown");
			} catch (InvalidOperationException) { }
		}
		
		[Test]
		public void ITransformProviderResizeTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			double width = 50, heitht = 50;
			transformProvider.Resize (width, heitht);
			Assert.AreEqual (width, splitter.Size.Width, "Width");
			Assert.AreEqual (heitht, splitter.Size.Height, "Height");
			
			splitter.Dock = DockStyle.Right;
			transformProvider.Resize (width, heitht);
			Assert.AreEqual (width, splitter.Size.Width, "Width");
			Assert.AreEqual (heitht, splitter.Size.Height, "Height");
			
			splitter.Dock = DockStyle.Bottom;
			transformProvider.Resize (width, heitht);
			Assert.AreEqual (width, splitter.Size.Width, "Width");
			Assert.AreEqual (heitht, splitter.Size.Height, "Height");
			
			splitter.Dock = DockStyle.Top;
			transformProvider.Resize (width, heitht);
			Assert.AreEqual (width, splitter.Size.Width, "Width");
			Assert.AreEqual (heitht, splitter.Size.Height, "Height");
		}
		
		[Test]
		public void ITransformProviderRotateTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			ITransformProvider transformProvider = (ITransformProvider)
				provider.GetPatternProvider (TransformPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (transformProvider,
			                  "Not returning TransformPatternIdentifiers.");
			
			try {
				double degrees = 50;
				transformProvider.Rotate (degrees);
				Assert.Fail ("InvalidOperationException not thrown");
			} catch (InvalidOperationException) { }
		}
		
		#endregion
		
		#region IDockProvider Test
		
		[Test]
		public void IDockProviderDockPositionTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			IDockProvider dockProvider = (IDockProvider)
				provider.GetPatternProvider (DockPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (dockProvider,
			                  "Not returning DockPatternIdentifiers.");
			
			Assert.AreEqual ((int) splitter.Dock,
			                 (int) dockProvider.DockPosition,
			                 "Splitter is Left DockStyle by default.");
		}
		
		[Test]
		public void IDockProviderSetDockPositionTest ()
		{
			Splitter splitter = new Splitter ();
			IRawElementProviderSimple provider = 
				ProviderFactory.GetProvider (splitter);
			
			IDockProvider dockProvider = (IDockProvider)
				provider.GetPatternProvider (DockPatternIdentifiers.Pattern.Id);
			Assert.IsNotNull (dockProvider,
			                  "Not returning DockPatternIdentifiers.");
			
			dockProvider.SetDockPosition (DockPosition.Right);
			Assert.AreEqual ((int) splitter.Dock,
			                 (int) dockProvider.DockPosition,
			                 "Splitter should be Right DockStyle.");
			
			try {
				dockProvider.SetDockPosition ((DockPosition) DockStyle.None);
				Assert.Fail ("InvalidOperationException not thrown");
			} catch (InvalidOperationException) { }
			
			try {
				dockProvider.SetDockPosition ((DockPosition) DockStyle.Fill);
				Assert.Fail ("InvalidOperationException not thrown");
			} catch (InvalidOperationException) { }
		}
		
		#endregion
		
		#region BaseProviderTest Overrides
		
		protected override Control GetControlInstance ()
		{
			return new Splitter ();
		}
		
		#endregion
	}
}