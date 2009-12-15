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
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com) 
// 
// Authors: 
//  Matt Guo <matt@mattguo.com>
// 

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using Mono.UIAutomation.Services;
using Mono.UIAutomation.Source;
using DC = Mono.UIAutomation.UiaDbus;
using DCI = Mono.UIAutomation.UiaDbus.Interfaces;

namespace Mono.UIAutomation.UiaDbusSource
{
	public class UiaDbusWindowPattern : IWindowPattern
	{
		private DCI.IWindowPattern pattern;

		public UiaDbusWindowPattern (DCI.IWindowPattern pattern)
		{
			this.pattern = pattern;
		}

		public void Close ()
		{
			try {
				pattern.Close ();
			} catch (Exception ex) {
				throw DbusExceptionTranslator.Translate (ex);
			}
		}

		public void SetWindowVisualState (WindowVisualState state)
		{
			try {
				pattern.SetVisualState (state);
			} catch (Exception ex) {
				throw DbusExceptionTranslator.Translate (ex);
			}
		}

		public bool WaitForInputIdle (int milliseconds)
		{
			try {
				return pattern.WaitForInputIdle (milliseconds);
			} catch (Exception ex) {
				throw DbusExceptionTranslator.Translate (ex);
			}
		}

		public bool CanMaximize {
			get {
				try {
					return pattern.Maximizable;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}

		public bool CanMinimize {
			get {
				try {
					return pattern.Minimizable;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}

		public bool IsModal {
			get {
				try {
					return pattern.IsModal;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}

		public bool IsTopmost {
			get {
				try {
					return pattern.IsTopmost;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}

		public WindowInteractionState WindowInteractionState {
			get {
				try {
					return pattern.InteractionState;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}

		public WindowVisualState WindowVisualState {
			get {
				try {
					return pattern.VisualState;
				} catch (Exception ex) {
					throw DbusExceptionTranslator.Translate (ex);
				}
			}
		}
	}
}
