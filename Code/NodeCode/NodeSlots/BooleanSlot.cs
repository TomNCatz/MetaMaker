using System;
using System.Collections.Generic;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class BooleanSlot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private CheckBox _field;
		
		private List<string> _matchTrue = new List<string>();
		private List<string> _invertTrue = new List<string>();
		private List<string> _matchFalse = new List<string>();
		private List<string> _invertFalse = new List<string>();

		private SlottedGraphNode _parent;
		private bool _toggling;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_field = this.GetNodeFromPath<CheckBox>( _fieldPath );
			_field.Connect( "toggled", this, nameof(OnToggled) );
			
			_parent = GetParent<SlottedGraphNode>();
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "defaultValue", out bool value );
			_field.Pressed = value;

			if( data.values.ContainsKey( "id" ) )
			{
				data.GetValue( "id", out string id );
				_parent.booleanLookup[id] = this;
			}
			
			if( data.values.ContainsKey( "matchTrue" ) )
			{
				data.GetValue( "matchTrue", out _matchTrue );
				data.GetValue( "invertTrue", out _invertTrue );
				data.GetValue( "matchFalse", out _matchFalse );
				data.GetValue( "invertFalse", out _invertFalse );
			}
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Pressed );
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
			_toggling = false;
		}

		public string GetString()
		{
			return _field.Pressed.ToString();
		}
	}
}