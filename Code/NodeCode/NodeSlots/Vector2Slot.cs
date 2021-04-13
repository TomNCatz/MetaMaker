using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class Vector2Slot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _xPath;
		private SpinBox _x;
		[Export] public NodePath _yPath;
		private SpinBox _y;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_x = this.GetNodeFromPath<SpinBox>( _xPath );
			_y = this.GetNodeFromPath<SpinBox>( _yPath );
			
			_x.Connect("value_changed",this,nameof(OnChanged));
			_y.Connect("value_changed",this,nameof(OnChanged));
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;

			template.GetValue( "defaultValue", out Vector2 vector );
			_x.Value = vector.x;
			_y.Value = vector.y;

			_parentModel.AddValue( _label.Text, vector );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			LibT.Maths.Vector2 vector = new Vector2((float)_x.Value,(float)_y.Value);
			objData.AddValue( _label.Text, vector );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out Vector2 vector );

			_x.Value = vector.x;
			_y.Value = vector.y;
		}
		
		private void OnChanged(float value)
		{
			LibT.Maths.Vector2 vector = new Vector2((float)_x.Value,(float)_y.Value);
			_parentModel.AddValue( _label.Text, vector );
			OnValueUpdated?.Invoke();
		}
	}
}