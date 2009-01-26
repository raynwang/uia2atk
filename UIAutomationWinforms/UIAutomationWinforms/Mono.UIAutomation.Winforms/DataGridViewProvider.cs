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
//	Mario Carrion <mcarrion@novell.com>
//
using Mono.Unix;
using System;
using System.ComponentModel;
using SD = System.Drawing;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using SWF = System.Windows.Forms;
using Mono.UIAutomation.Winforms.Behaviors.DataGridView;

namespace Mono.UIAutomation.Winforms
{
		
	[MapsComponent (typeof (SWF.DataGridView))]
	internal class DataGridViewProvider : ListProvider
	{
		#region Constructors
		
		public DataGridViewProvider (SWF.DataGridView datagridview) 
			: base (datagridview)
		{
			this.datagridview = datagridview;
		}

		#endregion

		#region Overridden Methods

		// NOTE:
		//       SWF.DataGridView.VerticalScrollBar and 
		//       SWF.DataGridView.HorizontalScrollBar are protected 
		//       properties part of the public API.
		protected override SWF.ScrollBar HorizontalScrollBar { 
			get {
				return Helper.GetPrivateProperty<SWF.DataGridView, SWF.ScrollBar> (typeof (SWF.DataGridView),
				                                                                   datagridview,
				                                                                   "HorizontalScrollBar");
			}
		}

		protected override SWF.ScrollBar VerticalScrollBar { 
			get {
				return Helper.GetPrivateProperty<SWF.DataGridView, SWF.ScrollBar> (typeof (SWF.DataGridView),
				                                                                   datagridview,
				                                                                   "VerticalScrollBar");
			}
		}

		protected override object GetProviderPropertyValue (int propertyId)
		{
			if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
				return ControlType.DataGrid.Id;
			else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
				return Catalog.GetString ("data grid");
			else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
				return true;
			else
				return base.GetProviderPropertyValue (propertyId);
		}

		public override void UnselectItem (ListItemProvider item)
		{
			if (!ContainsItem (item))
				return;
			
			DataGridDataItemProvider dataItem = (DataGridDataItemProvider) item;
			dataItem.Row.Selected = false;
		}

		public override void SelectItem (ListItemProvider item)
		{
			if (!ContainsItem (item))
				return;

			((DataGridDataItemProvider) item).Row.Selected = true;
		}

		public override void ScrollItemIntoView (ListItemProvider item)
		{
			if (!ContainsItem (item))
				return;

			DataGridDataItemProvider dataItem = (DataGridDataItemProvider) item;
			datagridview.FirstDisplayedCell = dataItem.Row.Cells [0];
		}

		public override ListItemProvider[] GetSelectedItems ()
		{
			List<ListItemProvider> items = new List<ListItemProvider> ();
			
			foreach (SWF.DataGridViewRow row in datagridview.SelectedRows)
				items.Add (GetItemProviderFrom (this, row, false));

			return items.ToArray ();
		}
		
		public override int SelectedItemsCount {
			get { return datagridview.SelectedRows.Count; }
		}

		public override bool IsItemSelected (ListItemProvider item)
		{
			if (!ContainsItem (item))
				return false;

			return ((DataGridDataItemProvider) item).Row.Selected;
		}
		
		public override int ItemsCount {
			get { return datagridview.Rows.Count; }
		}

		public override int IndexOfObjectItem (object objectItem)
		{
			SWF.DataGridViewRow row = objectItem as SWF.DataGridViewRow;
			if (row == null)
				return -1;

			return datagridview.Rows.IndexOf (row);
		}

		public override object GetItemPropertyValue (ListItemProvider item, int propertyId)
		{
			DataGridDataItemProvider provider = (DataGridDataItemProvider) item;
			
			if (propertyId == AutomationElementIdentifiers.NameProperty.Id)
				return provider.Row.Cells [0].Value as string;
			else if (propertyId == AutomationElementIdentifiers.HasKeyboardFocusProperty.Id)
				return false;
			else if (propertyId == AutomationElementIdentifiers.BoundingRectangleProperty.Id) {
				SD.Rectangle rectangle = datagridview.GetRowDisplayRectangle (provider.Row.Index, false);
				if (datagridview.RowHeadersVisible)
					rectangle.X += datagridview.RowHeadersWidth;

				return Helper.GetControlScreenBounds (rectangle, 
				                                      datagridview,
				                                      true);
			} else if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id)
				return Helper.IsListItemOffScreen (item.BoundingRectangle,
				                                   datagridview, 
				                                   datagridview.ColumnHeadersVisible,
				                                   header.Size,
				                                   ScrollBehaviorObserver);
			else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
				return true; // FIXME: Is this always OK?
			else
				return null;
		}

		internal override Behaviors.IProviderBehavior GetBehaviorRealization (AutomationPattern behavior)
		{
			if (behavior == SelectionPatternIdentifiers.Pattern)
				return new SelectionProviderBehavior (this);
			// FIXME: Implement ScrollProviderBehavior
			else
				return null;
		}

		public override void InitializeChildControlStructure ()
		{
			base.InitializeChildControlStructure ();

			header = new DataGridViewHeaderProvider (this);
			header.Initialize ();
			OnNavigationChildAdded (false, header);

			datagridview.Rows.CollectionChanged += OnCollectionChanged;
			foreach (SWF.DataGridViewRow row in datagridview.Rows) {
				ListItemProvider itemProvider = GetItemProviderFrom (this, row);
				OnNavigationChildAdded (false, itemProvider);
			}
		}

		public override void FinalizeChildControlStructure ()
		{
			base.FinalizeChildControlStructure ();
			
			datagridview.Rows.CollectionChanged -= OnCollectionChanged;
		}

		protected override ListItemProvider GetNewItemProvider (FragmentRootControlProvider rootProvider,
		                                                        ListProvider provider,
		                                                        SWF.Control control,
		                                                        object objectItem)
		{
			return new DataGridDataItemProvider (this, 
			                                     datagridview, 
			                                     (SWF.DataGridViewRow) objectItem);
		}

		#endregion

		#region Public Properties

		public DataGridViewHeaderProvider Header {
			get { return header; }
		}

		public SWF.DataGridView DataGridView {
			get { return datagridview; }
		}

		#endregion

		#region Private Fields

		private SWF.DataGridView datagridview;
		private DataGridViewHeaderProvider header;

		#endregion

		#region Internal Class: Header Provider 
		
		internal class DataGridViewHeaderProvider : FragmentRootControlProvider
		{
			public DataGridViewHeaderProvider (DataGridViewProvider provider) : base (null)
			{
				viewProvider = provider;
				headers = new Dictionary<SWF.DataGridViewColumn, DataGridViewHeaderItemProvider> ();
			}

			public override IRawElementProviderFragmentRoot FragmentRoot {
				get {  return viewProvider; }
			}

			public SWF.DataGridView DataGridView {
				get { return viewProvider.DataGridView; }
			}

			public SD.Rectangle Size {
				get {
					if (!DataGridView.ColumnHeadersVisible)
						return new SD.Rectangle (0, 0, 0, 0);
					else {
						SD.Rectangle bounds = SD.Rectangle.Empty;
						bounds.Height = DataGridView.ColumnHeadersHeight;
						bounds.Width = DataGridView.RowHeadersWidth;
						for (int index = 0; index < DataGridView.Columns.Count; index++)
							bounds.Width += DataGridView.Columns [index].Width;
						
						return bounds;
					}
				}
			}

			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.Header.Id;
				else if (propertyId == AutomationElementIdentifiers.NameProperty.Id)
					return "Header";
				else if (propertyId == AutomationElementIdentifiers.LabeledByProperty.Id)
					return null;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("header");
				else if (propertyId == AutomationElementIdentifiers.OrientationProperty.Id)
					return OrientationType.Horizontal;
				else if (propertyId == AutomationElementIdentifiers.IsContentElementProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsEnabledProperty.Id)
					return true;
				else if (propertyId == AutomationElementIdentifiers.BoundingRectangleProperty.Id)
					return Helper.GetControlScreenBounds (Size, DataGridView, true);
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			public override void InitializeChildControlStructure ()
			{
				DataGridView.Columns.CollectionChanged += OnColumnsCollectionChanged;
				
				foreach (SWF.DataGridViewColumn column in DataGridView.Columns)
					UpdateCollection (column, CollectionChangeAction.Add, false);
			}

			public override void FinalizeChildControlStructure ()
			{
				base.FinalizeChildControlStructure ();

				foreach (DataGridViewHeaderItemProvider item in headers.Values)
					item.Terminate ();

				headers.Clear ();
			}

			private void OnColumnsCollectionChanged (object sender,
			                                         CollectionChangeEventArgs args)
			{
				UpdateCollection ((SWF.DataGridViewColumn) args.Element,
				                  args.Action,
				                  true);
			}

			private void UpdateCollection (SWF.DataGridViewColumn column, 
			                               CollectionChangeAction change,
			                               bool raiseEvent)
			{
				if (change == CollectionChangeAction.Remove) {
					DataGridViewHeaderItemProvider headerItem = headers [column];
					OnNavigationChildRemoved (raiseEvent, headerItem);
					headers.Remove (column);
				} else if (change == CollectionChangeAction.Add) {
					DataGridViewHeaderItemProvider headerItem 
						= new DataGridViewHeaderItemProvider (this, column);
					headerItem.Initialize ();
					OnNavigationChildAdded (raiseEvent, headerItem);
					headers [column] = headerItem;
				}
			}

			private DataGridViewProvider viewProvider;
			private Dictionary<SWF.DataGridViewColumn, DataGridViewHeaderItemProvider> headers;
		}

		#endregion

		#region Internal Class: Header Item Provider

		internal class DataGridViewHeaderItemProvider : FragmentControlProvider
		{
			public DataGridViewHeaderItemProvider (DataGridViewHeaderProvider headerProvider,
			                                       SWF.DataGridViewColumn column)
				: base (null)
			{
				this.headerProvider = headerProvider;
				this.column = column;
			}

			public override IRawElementProviderFragmentRoot FragmentRoot {
				get { return headerProvider; }
			}

			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.HeaderItem.Id;
				else if (propertyId == AutomationElementIdentifiers.NameProperty.Id)
					return column.HeaderText;
				else if (propertyId == AutomationElementIdentifiers.LabeledByProperty.Id)
					return null;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("header item");
				else if (propertyId == AutomationElementIdentifiers.OrientationProperty.Id)
					return OrientationType.Horizontal;
				else if (propertyId == AutomationElementIdentifiers.IsContentElementProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.HasKeyboardFocusProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsEnabledProperty.Id)
					return true;
				else if (propertyId == AutomationElementIdentifiers.HelpTextProperty.Id)
					return column.ToolTipText;
				else if (propertyId == AutomationElementIdentifiers.BoundingRectangleProperty.Id) {
					if (column == null || column.Index < 0)
						return Rect.Empty;

					Rect headerBounds
						= (Rect) headerProvider.GetPropertyValue (AutomationElementIdentifiers.BoundingRectangleProperty.Id);
					for (int index = 0; index < column.Index; index++)
						headerBounds.X += headerProvider.DataGridView.Columns [index].Width;

					headerBounds.X += headerProvider.DataGridView.RowHeadersWidth;
					headerBounds.Width = headerProvider.DataGridView.Columns [column.Index].Width;
					
					return headerBounds;
				} else if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id) {
					Rect bounds 
						= (Rect) GetPropertyValue (AutomationElementIdentifiers.BoundingRectangleProperty.Id);
					return Helper.IsOffScreen (bounds, headerProvider.DataGridView, true);
				} else if (propertyId == AutomationElementIdentifiers.ClickablePointProperty.Id)
					return Helper.GetClickablePoint (this);
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			private SWF.DataGridViewColumn column;
			private DataGridViewHeaderProvider headerProvider;
		}

		#endregion

		#region Internal Class: Data Item Provider

		internal class DataGridDataItemProvider : ListItemProvider
		{
			public DataGridDataItemProvider (DataGridViewProvider datagridProvider,
			                                 SWF.DataGridView datagridview,
		                                     SWF.DataGridViewRow row) 
				: base (datagridProvider, datagridProvider, datagridview, row)
			{
				this.datagridview = datagridview;
				this.row = row;

				columns = new Dictionary<SWF.DataGridViewColumn, DataGridViewDataItemChildProvider> ();
			}

			public SWF.DataGridView DataGridView {
				get { return datagridview; }
			}

			public SWF.DataGridViewRow Row {
				get { return row; }
			}

			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.DataItem.Id;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("data item");
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			public override void InitializeChildControlStructure ()
			{
				foreach (SWF.DataGridViewColumn column in datagridview.Columns)
					UpdateCollection (column, CollectionChangeAction.Add, false);

				datagridview.Columns.CollectionChanged += OnColumnsCollectionChanged;
			}

			public override void FinalizeChildControlStructure ()
			{
				base.FinalizeChildControlStructure ();

				datagridview.Columns.CollectionChanged -= OnColumnsCollectionChanged;
			}

			private void OnColumnsCollectionChanged (object sender, 
			                                         CollectionChangeEventArgs args)
			{
				UpdateCollection ((SWF.DataGridViewColumn) args.Element,
				                  args.Action, 
				                  true);
			}

			private void UpdateCollection (SWF.DataGridViewColumn column, 
			                               CollectionChangeAction change,
			                               bool raiseEvent)
			{
				if (change == CollectionChangeAction.Remove) {
					DataGridViewDataItemChildProvider child = columns [column];
					OnNavigationChildRemoved (raiseEvent, child);
					child.Terminate ();
					columns.Remove (child.Column);
				} else if (change == CollectionChangeAction.Add) {
					DataGridViewDataItemChildProvider child;

					if ((column as SWF.DataGridViewButtonColumn) != null)
						child = new DataGridViewDataItemButtonProvider (this, column);
					else if ((column as SWF.DataGridViewCheckBoxColumn) != null)
						child = new DataGridViewDataItemCheckBoxProvider (this, column);
					else if ((column as SWF.DataGridViewLinkColumn) != null)
						child = new DataGridViewDataItemLinkProvider (this, column);
					else
						child = new DataGridViewDataItemChildProvider (this, column);

					child.Initialize ();
					OnNavigationChildAdded (raiseEvent, child);
					columns [child.Column] = child;
				}
			}

			private Dictionary<SWF.DataGridViewColumn, DataGridViewDataItemChildProvider> columns;
			private SWF.DataGridView datagridview;
			private SWF.DataGridViewRow row;
		}

		#endregion

		#region Internal Class: Data Item Child Provider

		internal class DataGridViewDataItemChildProvider : FragmentControlProvider
		{
			public DataGridViewDataItemChildProvider (DataGridDataItemProvider itemProvider,
			                                          SWF.DataGridViewColumn column) : base (null)
			{
				this.itemProvider = itemProvider;
				this.column = column;

				cell = itemProvider.Row.Cells [itemProvider.DataGridView.Columns.IndexOf (column)];
				gridProvider = (DataGridViewProvider) itemProvider.ListProvider;
			}

			public SWF.DataGridViewCell Cell {
				get { return cell; }
			}

			public SWF.DataGridViewColumn Column {
				get { return column; }
			}

			public DataGridDataItemProvider ItemProvider {
				get { return itemProvider; }
			}

			public override IRawElementProviderFragmentRoot FragmentRoot {
				get { return itemProvider; }
			}

			public override void Initialize ()
			{
				base.Initialize ();

				//LAMESPEC: Vista does not support Text Pattern.
				//http://msdn.microsoft.com/en-us/library/ms748367.aspx

//				SetBehavior (GridItemPatternIdentifiers.Pattern,
//				             new DataItemChildGridItemProviderBehavior (this));
//				SetBehavior (TableItemPatternIdentifiers.Pattern,
//				             new DataItemChildTableItemProviderBehavior (this));
				SetBehavior (ValuePatternIdentifiers.Pattern,
				             new DataItemChildValueProviderBehavior (this));

//				// Automation Events
//				SetEvent (ProviderEventType.AutomationElementIsOffscreenProperty,
//				          new ListItemEditAutomationIsOffscreenPropertyEvent (this));
			}

			protected override object GetProviderPropertyValue (int propertyId)
			{
				// FIXME: Generalize?
				
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id) {
					if (typeof (SWF.DataGridViewComboBoxColumn) == column.GetType ())
						return ControlType.ComboBox.Id;
					else if (typeof (SWF.DataGridViewImageColumn) == column.GetType ())
						return ControlType.Image.Id;
					else // SWF.DataGridViewTextBoxColumn or something else
						return ControlType.Edit.Id;
				} else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id) {
					if (typeof (SWF.DataGridViewComboBoxColumn) == column.GetType ())
						return Catalog.GetString ("combobox");
					else if (typeof (SWF.DataGridViewImageColumn) == column.GetType ())
						return Catalog.GetString ("image");
					else // SWF.DataGridViewTextBoxColumn or something else
						return Catalog.GetString ("edit");
				} else if (propertyId == AutomationElementIdentifiers.NameProperty.Id) {
					return cell.Value as string;
				} else if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.HasKeyboardFocusProperty.Id)
					return false;
				else if (propertyId == AutomationElementIdentifiers.IsEnabledProperty.Id)
					return true;
				else if (propertyId == AutomationElementIdentifiers.LabeledByProperty.Id)
					return null;
				else if (propertyId == AutomationElementIdentifiers.HelpTextProperty.Id)
					return cell.ToolTipText;
				else if (propertyId == AutomationElementIdentifiers.BoundingRectangleProperty.Id) {
					Rect itemBounds = itemProvider.BoundingRectangle;

					for (int index = 0; index < cell.ColumnIndex; index++)
						itemBounds.X += itemProvider.DataGridView.Columns [index].Width;

					itemBounds.Width = itemProvider.DataGridView.Columns [cell.ColumnIndex].Width;

					return itemBounds;
				} else if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id)
					return Helper.IsListItemOffScreen (BoundingRectangle,
					                                   itemProvider.DataGridView, 
					                                   itemProvider.DataGridView.ColumnHeadersVisible,
					                                   gridProvider.Header.Size,
					                                   gridProvider.ScrollBehaviorObserver);
				else if (propertyId == AutomationElementIdentifiers.ClickablePointProperty.Id)
					return Helper.GetClickablePoint (this);
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			private SWF.DataGridViewCell cell;
			private SWF.DataGridViewColumn column;
			private DataGridDataItemProvider itemProvider;
			private DataGridViewProvider gridProvider;
		}

		#endregion

		#region Internal Class: Data Item Button Provider

		internal class DataGridViewDataItemButtonProvider : DataGridViewDataItemChildProvider
		{
			public DataGridViewDataItemButtonProvider (DataGridDataItemProvider itemProvider,
			                                           SWF.DataGridViewColumn column)
				: base (itemProvider, column)
			{
				buttonCell = (SWF.DataGridViewButtonCell) Cell;
			}

			public override void Initialize ()
			{
				base.Initialize ();
	
				SetBehavior (InvokePatternIdentifiers.Pattern, 
				             new DataItemChildInvokeProviderBehavior (this));
			}

			public SWF.DataGridViewButtonCell ButtonCell {
				get { return buttonCell; }
			}
			
			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.Button.Id;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("button");
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			private SWF.DataGridViewButtonCell buttonCell;
		}

		#endregion

		#region Internal Class: Data Item CheckBox Provider

		internal class DataGridViewDataItemCheckBoxProvider : DataGridViewDataItemChildProvider
		{
			public DataGridViewDataItemCheckBoxProvider (DataGridDataItemProvider itemProvider,
			                                             SWF.DataGridViewColumn column)
				: base (itemProvider, column)
			{
				checkBoxCell = (SWF.DataGridViewCheckBoxCell) Cell;
			}

			public override void Initialize ()
			{
				base.Initialize ();
	
				SetBehavior (TogglePatternIdentifiers.Pattern,
				             new DataItemChildToggleProviderBehavior (this));
			}

			public SWF.DataGridViewCheckBoxCell CheckBoxCell {
				get { return checkBoxCell; }
			}
			
			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.CheckBox.Id;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("checkbox");
				else
					return base.GetProviderPropertyValue (propertyId);
			}

			private SWF.DataGridViewCheckBoxCell checkBoxCell;
		}

		#endregion

		#region Internal Class: Data Item Link Provider

		internal class DataGridViewDataItemLinkProvider : DataGridViewDataItemChildProvider
		{
			public DataGridViewDataItemLinkProvider (DataGridDataItemProvider itemProvider,
			                                         SWF.DataGridViewColumn column)
				: base (itemProvider, column)
			{
			}

			public override void Initialize ()
			{
				base.Initialize ();
	
				SetBehavior (InvokePatternIdentifiers.Pattern, 
				             new DataItemChildInvokeProviderBehavior (this));
			}
			
			protected override object GetProviderPropertyValue (int propertyId)
			{
				if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id)
					return ControlType.Hyperlink.Id;
				else if (propertyId == AutomationElementIdentifiers.LocalizedControlTypeProperty.Id)
					return Catalog.GetString ("hyperlink");
				else
					return base.GetProviderPropertyValue (propertyId);
			}
		}

		#endregion
	}
}