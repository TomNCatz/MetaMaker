using System;
using Godot;
using LibT;

namespace MetaMaker
{
	public class FlagItem : Container
	{
		[Export] public NodePath _titlePath;
		private Label _title;
		[Export] public NodePath _checkPath;
		private CheckBox _check;

		public event Action OnValueUpdated;

		public string Label 
		{
			set
			{
				_title.Text = value;
			}
		}

		public bool Value
		{
			get => _value;
			set
			{
				_value = value;
				_check.Pressed = value;
			}
		}
		private bool _value;

		public override void _Ready()
		{
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_check = this.GetNodeFromPath<CheckBox>( _checkPath );
			_check.Connect( "toggled", this, nameof(OnToggled) );
		}

		private void OnToggled(bool buttonPressed)
		{
			_value = buttonPressed;
			OnValueUpdated?.Invoke();
		}
	}
}