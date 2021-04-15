using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class EnumSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private OptionButton _field;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
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
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text,out string choice );

			for( int i = 0; i < _field.GetItemCount(); i++ )
			{
				if( _field.GetItemText( i ).Equals( choice ) )
				{
					_field.Select( i );
					_parentModel.AddValue( _label.Text, GetSelection() );
				}	
			}
		}

		private void OnChanged(int value)
		{
			_parentModel.AddValue( _label.Text, GetSelection() );
			OnValueUpdated?.Invoke();
		}

		private string GetSelection()
		{
			if(_field.GetItemCount() == 0)
			{
				return string.Empty;
			}

			return _field.GetItemText(_field.Selected);
		}
	}
}