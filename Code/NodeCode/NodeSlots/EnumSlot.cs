using System;
using System.Collections.Generic;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class EnumSlot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private OptionButton _field;

		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<OptionButton>( _fieldPath );
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "values", out List<string> values );
			foreach( string value in values )
			{
				_field.AddItem( value );
			}
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.GetItemText(_field.Selected) );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text,out string choice );

			for( int i = 0; i < _field.GetItemCount(); i++ )
			{
				if( _field.GetItemText( i ).Equals( choice ) )
				{
					_field.Select( i );
				}	
			}
		}

		public string GetString()
		{
			return _field.GetItemText( _field.Selected );
		}
	}
}