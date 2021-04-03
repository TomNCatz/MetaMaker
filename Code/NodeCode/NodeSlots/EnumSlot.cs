using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class EnumSlot : Container, IField, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _fieldPath;
		private OptionButton _field;
		private GenericDataArray _parentModel;

		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<OptionButton>( _fieldPath );
			_field.Connect("item_selected",this,nameof(OnChanged));
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			_parentModel = parentModel;

			template.GetValue( "values", out List<string> values );
			foreach( string value in values )
			{
				_field.AddItem( value );
			}
			_parentModel.AddValue( _label.Text, GetSelection() );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, GetSelection() );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text,out string choice );

			_field.Clear();
			for( int i = 0; i < _field.GetItemCount(); i++ )
			{
				if( _field.GetItemText( i ).Equals( choice ) )
				{
					_field.Select( i );
				}	
			}
		}

		private void OnChanged(int value)
		{
			_parentModel.AddValue( _label.Text, GetSelection() );
		}

		private string GetSelection()
		{
			if(_field.GetItemCount() == 0)
			{
				return string.Empty;
			}

			return _field.GetItemText(_field.Selected);
		}

		public string GetString()
		{
			return _field.GetItemText( _field.Selected );
		}
	}
}