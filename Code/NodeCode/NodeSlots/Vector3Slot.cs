using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class Vector3Slot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _xPath;
		private SpinBox _x;
		[Export] public NodePath _yPath;
		private SpinBox _y;
		[Export] public NodePath _zPath;
		private SpinBox _z;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_x = this.GetNodeFromPath<SpinBox>( _xPath );
			_y = this.GetNodeFromPath<SpinBox>( _yPath );
			_z = this.GetNodeFromPath<SpinBox>( _zPath );

			_x.Connect("value_changed",this,nameof(OnChanged));
			_y.Connect("value_changed",this,nameof(OnChanged));
			_z.Connect("value_changed",this,nameof(OnChanged));
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;

			template.GetValue( "defaultValue", out Vector3 vector );
			_x.Value = vector.x;
			_y.Value = vector.y;
			_z.Value = vector.z;

			_parentModel.AddValue( _label.Text, vector );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			LibT.Maths.Vector3 vector = new Vector3((float)_x.Value,(float)_y.Value,(float)_z.Value);
			objData.AddValue( _label.Text, vector );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out Vector3 vector );

			_x.Value = vector.x;
			_y.Value = vector.y;
			_z.Value = vector.z;
		}
		
		private void OnChanged(float value)
		{
			LibT.Maths.Vector3 vector = new Vector3((float)_x.Value,(float)_y.Value,(float)_z.Value);
			_parentModel.AddValue( _label.Text, vector );
			OnValueUpdated?.Invoke();
		}
	}
}