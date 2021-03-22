using Godot;
using LibT.Serialization;

namespace LibT
{
	public class IntSlot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private SpinBox _field;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<SpinBox>( _fieldPath );
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "hasMax", out bool hasMax );
			data.GetValue( "hasMin", out bool hasMin );

			_field.AllowGreater = !hasMax;
			if( hasMax )
			{
				data.GetValue( "max", out int max );
				_field.MaxValue = max;
			}

			_field.AllowLesser = !hasMin;
			if( hasMin )
			{
				data.GetValue( "min", out int min );
				_field.MinValue = min;
			}
			
			if( data.values.ContainsKey( "minStepSize" ) )
			{
				data.GetValue( "minStepSize", out double minStepSize );
				_field.Step = minStepSize;
			}
			
			data.GetValue( "defaultValue", out int value );
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

		public string GetString()
		{
			return ((int)_field.Value).ToString();
		}
	}
}
