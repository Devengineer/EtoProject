using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using Eto.Forms;
using Eto.Drawing;

namespace EtoProject
{
	public class MyCommand : Command
	{
		public MyCommand ()
		{
			MenuText = "C&lick Me, Command";
			ToolBarText = "Click Me";
			ToolTip = "This shows a dialog for no reason";
			//Image = Icon.FromResource ("MyResourceName.ico");
			//Image = Bitmap.FromResource ("MyResourceName.png");
			Shortcut = Application.Instance.CommonModifier | Keys.M;  // control+M or cmd+M
		}

		protected override void OnExecuted (EventArgs e)
		{
			base.OnExecuted(e);

			MessageBox.Show(Application.Instance.MainForm, "You clicked me!", "Wow!", MessageBoxButtons.OK);
		}
	}

		
	public class MyForm : Form
	{
		public MyForm ()
		{
			// Set ClientSize instead of Size, as each platform has different window border sizes
			ClientSize = new Size (600, 400);

			// Title to show in the title bar
			Title = "Menus and Toolbars";

			// Create menu
			Menu = new MenuBar
			{
				Items =
				{
					new ButtonMenuItem
					{
						Text = "&File",
						Items =
						{
							// You can add commands or menu items
							new MyCommand (),

							// Another menu item, not based off a Command
							new ButtonMenuItem { Text = "Click Me, MenuItem" }
						}
					}
				},

				// Quit item (goes in Application menu on OS X, File menu for others)
				QuitItem = new Command((sender, e) => Application.Instance.Quit())
				{
					MenuText = "Quit",
					Shortcut = Application.Instance.CommonModifier | Keys.Q
				},

				// About command (goes in Application menu on OS X, Help menu for others)
				AboutItem = new Command((sender, e) => new Dialog
				{
					Content = new Label { Text = "About my app..." },
					ClientSize = new Size (200, 200)
				}.ShowModal(this))
				{
					MenuText = "About my app"
				}
			};

			// Create toolbar
			ToolBar = new ToolBar {
				Items = {
					new MyCommand (),
					new SeparatorToolItem (),
					new ButtonToolItem { Text = "Click Me, ToolItem" }
				}
			};


			// ViewModel
			var model = new ViewModel { String = "Hello!", Check = false, BackgroundColor = Color.Parse ("white") };

			var textBox = new TextBox ();
			textBox.TextBinding.Bind (model, m => m.String);	// Working
			//textBox.Bind(c => c.Text, model, (ViewModel m) => m.String);	// Perfect TwoWay bind
			textBox.Bind(c => c.BackgroundColor, model, (ViewModel m) => m.BackgroundColor);	// Working

			var check = new CheckBox ();
			//check.CheckedBinding.BindDataContext((ViewModel m) => m.Check);	//Working Context
			check.CheckedBinding.Bind (model, m => m.Check);	// Working


			// The main layout mechanism for Eto.Forms is a TableLayout.
			// This is recommended to allow controls to keep their natural platform-specific size.
			// You can layout your controls declaratively using rows and columns as below, or add to the TableLayout.Rows and TableRow.Cell directly.
			Content =  new TableLayout
			{
				Spacing = new Size (5, 5),	// Space between each cell
				Padding = new Padding (10, 10, 10, 10),	// Space around the table's sides
				Rows =
				{
					new TableRow (
						new TableCell (new Label { Text = "First Column" }, true),
						new TableCell (new Label { Text = "Second Column" }, true),
						new Label { Text = "Third Column" }
					),
					new TableRow (
						new TextBox { Text = "Some Text", BackgroundColor = Color.Parse ("Green"), TextColor = Color.Parse ("Red") },	// Changed color of text and bg
						new DropDown { Items = { "Item 1", "Item 2", "Item 3" } },
						//new CheckBox { Text = "A checkbox" }
						new CheckBox { Text = "A checkbox", Checked = true }
					),
					new TableRow (
						textBox,
						check
					),
					new TableRow (
						new Slider { Value = 50, TickFrequency = 20 },
						new ProgressBar { Value = 50 }
					),

					// By default, the last row & column will get scaled. This adds a row at the end to take the extra space of the form.
					// Otherwise, the above row will get scaled and stretch the TextBox/ComboBox/CheckBox to fill the remaining height.
					null	//new TableRow { ScaleHeight = true }
				}
			};
			
			// Set data context so it propegates to all child controls
			/*
			var model = new ViewModel { String = "Hello!", Check = false, BackgroundColor = Color.Parse ("white") };
			DataContext = model;
			*/

			// This creates the following layout:
			//  --------------------------------
			// |First     |Second    |Third     |
			//  --------------------------------
			// |<TextBox> |<ComboBox>|<CheckBox>|
			//  --------------------------------
			// |          |          |          |
			// |          |          |          |
			//  --------------------------------
			//
			// Some notes:
			//  1. When scaling the width of a cell, it applies to all cells in the same column.
			//  2. When scaling the height of a row, it applies to the entire row.
			//  3. Scaling a row/column makes it share all remaining space with other scaled rows/columns.
			//  4. If a row/column is not scaled, it will be the size of the largest control in that row/column.
			//  5. A Control can be implicitly converted to a TableCell or TableRow to make the layout more concise.
		}
	}


	// Typically implemented view model
	public class ViewModel : INotifyPropertyChanged
	{
		string _string;
		bool _isChecked;
		Color _color;

		public string String
		{
			get { return _string; }
			set
			{
				if (_string != value)
				{
					SetField(ref _string, value, "String");
					Debug.WriteLine(string.Format("Set TextProperty to {0}", value));
					Color.TryParse(_string, out _color);
					try
					{
						Application.Instance.MainForm.UpdateBindings(BindingUpdateMode.Destination);
					}
					catch (Exception e)
					{
						Debug.WriteLine(string.Format("Exception: {0}", e));
					}
				}
			}
		}
			
		public bool Check
		{
			get { return _isChecked; }
			set
			{
				if (_isChecked != value)
				{
					SetField(ref _isChecked, value, "Check");
					Debug.WriteLine(string.Format("isChecked: {0}", value));
                    if (_isChecked)
                    {
						BackgroundColor = Color.Parse("green");
						String = "True";
                    }
                    else
                    {
						BackgroundColor = Color.Parse("red");
						String = "False";
                    }

					// Updating bindings manually
					Application.Instance.MainForm.UpdateBindings(BindingUpdateMode.Destination);
				}
			}
		}

		public Color BackgroundColor
		{
			get { return _color; }
			set
			{
				if (_color != value)
				{
					SetField(ref _color, value, "Color");
					Debug.WriteLine(string.Format("Set bg color to {0}", value));
				}
			}
		}

		// C# 6.0 makes the implementation easier
		/*
		void OnPropertyChanged([CallerMemberName] string memberName = null)
		{
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberName));
		}
		*/

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetField<T>(ref T field, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}

