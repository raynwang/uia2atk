// WindowPatternTests.cs: Tests for Window and Dock Patterns.
//
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
//	Ray Wang <rawang@novell.com>
//	Felicia Mu <fxmu@novell.com>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Mono.UIAutomation.TestFramework;
using NUnit.Framework;
using System.Windows.Automation;

namespace MonoTests.Mono.UIAutomation.UIAClientAPI.Winforms
{
	[TestFixture]
	public class WindowPatternTests : TestBase
	{
		Window window;
		Application app;

		protected override void LaunchSample ()
		{
			SingleInstance("WindowPattern and TransformPattern Test");
			app = new Application ("WindowAndTransformPatternProvider");
			app.Launch ("mono", "WindowAndTransformPatternProvider.exe");
		}

		protected override void OnSetup ()
		{
			base.OnSetup ();
			window = app.GetWindow ("WindowPattern and TransformPattern Test");
		}

		protected override void OnQuit ()
		{
			base.OnQuit ();
			procedureLogger.Save ();
			window.Close ();
		}

		[Test]
		public void RunTestCase106 ()
		{
			Run (TestCase106);
		}

		private void TestCase106 ()
		{
			//106.1 Maximize the window
			window.SetWindowVisualState (WindowVisualState.Maximized);
			Thread.Sleep (Config.Instance.MediumDelay);
			procedureLogger.ExpectedResult ("The window is Maximized.");
			Assert.AreEqual (WindowVisualState.Maximized, window.WindowVisualState);
			Assert.AreEqual (WindowInteractionState.Running, window.WindowInteractionState);
			Thread.Sleep (Config.Instance.ShortDelay);

			//106.2 Minimize the window
			window.SetWindowVisualState (WindowVisualState.Minimized);
			Thread.Sleep (Config.Instance.MediumDelay);
			procedureLogger.ExpectedResult ("The window is Minimized.");
			Assert.AreEqual (WindowVisualState.Minimized, window.WindowVisualState);
			Thread.Sleep (Config.Instance.ShortDelay);

			//106.3 Restore the window
			window.SetWindowVisualState (WindowVisualState.Normal);
			Thread.Sleep (Config.Instance.MediumDelay);
			procedureLogger.ExpectedResult ("The window would be Restored.");
			Assert.AreEqual (WindowVisualState.Normal, window.WindowVisualState);
			Thread.Sleep (Config.Instance.ShortDelay);

			//106.4 Rotate the control for a given degree
			//BUG574269 Cannot find pane control on Linux
			//BUG574242 Can't define custom provider by using the WM_GETOBJECT message
			//var pane = window.Find<Pane> ("WindowAndTransformPatternProviderControl, r:0");
			var pane = window.Find<Pane> ();
			pane.Rotate (90.0);
			procedureLogger.ExpectedResult ("The pane would be rotated for 90 degree.");
			Thread.Sleep (Config.Instance.ShortDelay);

			//106.5 Check the WindowPattern's property
			procedureLogger.Action ("Check the window can be maximized.");
			Assert.AreEqual (true, window.CanMaximize);
			procedureLogger.ExpectedResult ("The window can be maximized.");
			Thread.Sleep (Config.Instance.ShortDelay);

			procedureLogger.Action ("Check the window can be minimized.");
			Assert.AreEqual (true, window.CanMinimize);
			procedureLogger.ExpectedResult ("The window can be minimized.");
			Thread.Sleep (Config.Instance.ShortDelay);

			procedureLogger.Action ("Check the window is modal.");
			Assert.AreEqual (false, window.IsModal);
			procedureLogger.ExpectedResult ("The window is modal.");
			Thread.Sleep (Config.Instance.ShortDelay);

			procedureLogger.Action ("Check the window is topmost.");
			Assert.AreEqual (false, window.IsTopmost);
			procedureLogger.ExpectedResult ("The window is topmost.");
			Thread.Sleep (Config.Instance.ShortDelay);

			//106.6 Close the application
			window.Close ();
			procedureLogger.ExpectedResult ("The window closes.");
			Thread.Sleep (Config.Instance.ShortDelay);

			//Launch sample for the sake that NUnit could close the sample application.
			LaunchSample ();
			Thread.Sleep (Config.Instance.MediumDelay);
			OnSetup ();
		}
	}
}
