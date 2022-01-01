using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class LongSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private long _value;
		private bool _allowNegative;
		private GenericDataObject<string> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
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
			
			_model.CopyFrom(GenericDataObject.CreateGdo(_value));
			OnValueUpdated?.Invoke();
		}
		
		private void OnTextEnter( string text )
		{
			_field.Text = _value.ToString();
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;
				model.GetValue(out _value);
				_field.Text = _value.ToString();
			}
			else
			{
				template.GetValue( "defaultValue", out _value );
				_model = parentModel.TryAddValue(_label.Text, _value) as GenericDataObject<string>;
				_field.Text = _value.ToString();
			}
			
			template.GetValue( "allowNegative", out _allowNegative );
		}
	}
}
