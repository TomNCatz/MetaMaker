using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class BooleanSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private CheckBox _field;
		
		private List<string> _matchTrue = new List<string>();
		private List<string> _invertTrue = new List<string>();
		private List<string> _matchFalse = new List<string>();
		private List<string> _invertFalse = new List<string>();

		private SlottedGraphNode _parent;
		public event System.Action OnValueUpdated;
		private bool _toggling;
		private GenericDataArray _parentModel;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_field = this.GetNodeFromPath<CheckBox>( _fieldPath );
			_field.Connect( "toggled", this, nameof(OnToggled) );
			
			_parent = GetParent<SlottedGraphNode>();
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;
			_parentModel.AddValue(_label.Text, _field.Pressed);
			
			template.GetValue( "defaultValue", out bool value );
			_field.Pressed = value;

			if( template.values.ContainsKey( "id" ) )
			{
				template.GetValue( "id", out string id );
				_parent.booleanLookup[id] = this;
			}
			
			if( template.values.ContainsKey( "matchTrue" ) )
			{
				template.GetValue( "matchTrue", out _matchTrue );
				template.GetValue( "invertTrue", out _invertTrue );
				template.GetValue( "matchFalse", out _matchFalse );
				template.GetValue( "invertFalse", out _invertFalse );
			}
		}

		public void GetObjectData( GenericDataArray objData )
		{
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out bool value );
			_field.Pressed = value;
		}

		private void OnToggled(bool buttonPressed)
		{
			if( _toggling ) return;

			_toggling = true;
				
			if( buttonPressed )
			{
				foreach( string key in _matchTrue )
				{
					if( _parent.booleanLookup.ContainsKey( key ) )
					{
						_parent.booleanLookup[key].OnToggled(true);
					}
				}
				foreach( string key in _invertTrue )
				{
					if( _parent.booleanLookup.ContainsKey( key ) )
					{
						_parent.booleanLookup[key].OnToggled(false);
					}
				}
			}
			else
			{
				foreach( string key in _matchFalse )
				{
					if( _parent.booleanLookup.ContainsKey( key ) )
					{
						_parent.booleanLookup[key].OnToggled(false);
					}
				}
				foreach( string key in _invertFalse )
				{
					if( _parent.booleanLookup.ContainsKey( key ) )
					{
						_parent.booleanLookup[key].OnToggled(true);
					}
				}
			}

			_field.Pressed = buttonPressed;
			_parentModel.AddValue(_label.Text, buttonPressed);
			_toggling = false;
			OnValueUpdated?.Invoke();
		}
	}
}