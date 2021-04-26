using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class RelativePathSlot : Container, IField
	{
		private static Color good = new Color(1,1,1,1);
		private static Color bad = new Color(1f,0.4f,0.4f,1);

		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;

		private string _relativePath;
		private int _startOffset;
		private string _prefix;
		private string[] _extensions;
		private GenericDataArray _parentModel;
		private readonly ServiceInjection<MainView> _mainView = new ServiceInjection<MainView>();
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();

		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
			{
				_mainView.Get.FilePopup.Show( _extensions, false, "Select a File For a Path" )
				.Then( path =>
				{
					path = path.PathAbsoluteToRelative(_app.Get.SaveFilePath);
					if(path.Length > _startOffset)
					{
						path = path.Substring(_startOffset);
					}
					else
					{
						_app.Get.CatchException(new Exception($"Path({path}) was shorter than startOffest({_startOffset})"));
					}

					_relativePath = _prefix + path;
					UpdateDisplay();
					UpdateData();
				} )
				.Catch( _app.Get.CatchException );
			}
		}

		private void TestPath()
		{
			if(string.IsNullOrEmpty(_relativePath))
			{
				_field.Set("custom_colors/font_color", bad);
				return;
			}

			_field.Set("custom_colors/font_color", good);
		}

		private void UpdateDisplay()
		{
			if(string.IsNullOrEmpty(_relativePath))
			{
				_field.Text = "Path Not Set";
			}
			else
			{
				_field.Text = _relativePath;
			}

			TestPath();
		}

		private void UpdateData()
		{
			_parentModel.AddValue( _label.Text, _relativePath );
			OnValueUpdated?.Invoke();
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue( "startOffset", out _startOffset );
			template.GetValue( "prefix", out _prefix );
			template.GetValue( "extensions", out _extensions );

			_parentModel = parentModel;
			if(parentModel.values.ContainsKey(_label.Text))
			{
				parentModel.GetValue( _label.Text, out _relativePath );
				UpdateDisplay();
			}
			else
			{
				UpdateDisplay();
				UpdateData();
			}
		}
	}
}