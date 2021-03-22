using Godot;
using LibT.Serialization;

namespace LibT
{
	public class LongSlot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private LineEdit _field;
		private long _value;
		private bool _allowNegative;
		
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
		}
		
		private void OnTextEnter( string text )
		{
			_field.Text = _value.ToString();
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "allowNegative", out _allowNegative );
			
			data.GetValue( "defaultValue", out _value );
			_field.Text = _value.ToString();
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _value );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out _value );
			_field.Text = _value.ToString();
		}

		public string GetString()
		{
			return _value.ToString();
		}
	}
}
