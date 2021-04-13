using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class IntSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private SpinBox _field;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<SpinBox>( _fieldPath );
			_field.Connect("value_changed",this,nameof(OnChanged));
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "hasMax", out bool hasMax );
			template.GetValue( "hasMin", out bool hasMin );

			_parentModel = parentModel;
			_parentModel.AddValue(_label.Text, (int)_field.Value);

			_field.AllowGreater = !hasMax;
			if( hasMax )
			{
				template.GetValue( "max", out int max );
				_field.MaxValue = max;
			}

			_field.AllowLesser = !hasMin;
			if( hasMin )
			{
				template.GetValue( "min", out int min );
				_field.MinValue = min;
			}
			
			if( template.values.ContainsKey( "minStepSize" ) )
			{
				template.GetValue( "minStepSize", out double minStepSize );
				_field.Step = minStepSize;
			}
			
			template.GetValue( "defaultValue", out int value );
			_field.Value = value;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, (int)_field.Value );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out int value );
			_field.Value = value;
		}

		private void OnChanged(float value)
		{
			_parentModel.AddValue(_label.Text, (int)value);
			OnValueUpdated?.Invoke();
		}
	}
}
