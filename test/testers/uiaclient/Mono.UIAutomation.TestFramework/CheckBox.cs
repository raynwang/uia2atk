// CheckBox.cs: CheckBox control class wrapper.
//
// This program is free software; you can redistribute it and/or modify it under
// the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
//
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
//
// Authors:
//	Ray Wang  (rawang@novell.com)
//	Felicia Mu  (fxmu@novell.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace Mono.UIAutomation.TestFramework
{
	public class CheckBox : Element
	{
		public static readonly ControlType UIAType = ControlType.CheckBox;
		
		//List the patterns that the control must support
		public override List<AutomationPattern> SupportedPatterns {
			get { return new List<AutomationPattern>() {TogglePattern.Pattern}; }
			
		}

		public CheckBox (AutomationElement elm)
			: base (elm)
		{
		}

		#region TogglePattern.
		public void Toggle ()
		{
			Toggle (true);
		}

		public void Toggle (bool log)
		{
			if (log)
				procedureLogger.Action (string.Format ("Toggle {0}.", this.NameAndType));

			TogglePattern tp = (TogglePattern) element.GetCurrentPattern (TogglePattern.Pattern);
			tp.Toggle ();
		}

		public ToggleState ToggleState {
			get { return (ToggleState) element.GetCurrentPropertyValue (TogglePattern.ToggleStateProperty, true); }
		}
		#endregion
	}
}