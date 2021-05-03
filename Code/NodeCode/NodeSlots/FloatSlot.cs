using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class FloatSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private SpinBox _field;
		private GenericDataObject<float> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<SpinBox>( _fieldPath );

			_field.Connect("value_changed",this,nameof(OnChanged));
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "hasMax", out bool hasMax );
			template.GetValue( "hasMin", out bool hasMin );

			parentModel.TryGetValue(_label.Text, out GenericDataObject<float> model);
			if(model != null)
			{
				_model = model;
				_field.Value = _model.value;
			}
			else
			{
				template.GetValue( "defaultValue", out float value );
				_model = parentModel.TryAddValue(_label.Text, value) as GenericDataObject<float>;
				_field.Value = value;
			}

			_field.AllowGreater = !hasMax;
			if( hasMax )
			{
				template.GetValue( "max", out float max );
				_field.MaxValue = max;
			}

			_field.AllowLesser = !hasMin;
			if( hasMin )
			{
				template.GetValue( "min", out float min );
				_field.MinValue = min;
			}
			
			if( template.values.ContainsKey( "minStepSize" ) )
			{
				template.GetValue( "minStepSize", out double minStepSize );
				_field.Step = minStepSize;
			}
		}

		private void OnChanged(float value)
		{
			_model.value = value;
			OnValueUpdated?.Invoke();
		}
	}
}

