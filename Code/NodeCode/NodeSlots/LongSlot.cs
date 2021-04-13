using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class LongSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private long _value;
		private bool _allowNegative;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
			_field.Connect( "text_changed", this, nameof(OnTextChange) );
			_field.Connect( "text_entered", this, nameof(OnTextEnter) );
		}

		private void OnTextChange( string text )
		{
			if( string.IsNullOrEmpty( text ) )
			{
				_value = 0;
			}
			else if( text == "-")
			{
				_value = 0;
			}
			else if( long.TryParse( text, out long temp ) )
			{
				if( !_allowNegative && temp < 0 )
				{
					temp *= -1;
					_value = temp;
					_field.Text = _value.ToString();
				}
				_value = temp;
			}
			else
			{
				_field.Text = _value.ToString();
			}
			
			_parentModel.AddValue(_label.Text, _value);
			OnValueUpdated?.Invoke();
		}
		
		private void OnTextEnter( string text )
		{
			_field.Text = _value.ToString();
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			_parentModel = parentModel;
			_parentModel.AddValue(_label.Text, _value);
			
			template.GetValue( "allowNegative", out _allowNegative );
			
			template.GetValue( "defaultValue", out _value );
			_field.Text = _value.ToString();
			OnTextChange( _field.Text );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _value );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out _value );
			_field.Text = _value.ToString();
			OnTextChange( _field.Text );
		}
	}
}
