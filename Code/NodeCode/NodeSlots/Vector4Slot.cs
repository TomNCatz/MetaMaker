using Godot;
using LibT;
using LibT.Maths;
using LibT.Serialization;

namespace MetaMaker
{
	public class Vector4Slot : Container, IField, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _xPath;
		private SpinBox _x;
		[Export] private readonly NodePath _yPath;
		private SpinBox _y;
		[Export] private readonly NodePath _zPath;
		private SpinBox _z;
		[Export] private readonly NodePath _wPath;
		private SpinBox _w;
		private GenericDataArray _parentModel;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_x = this.GetNodeFromPath<SpinBox>( _xPath );
			_y = this.GetNodeFromPath<SpinBox>( _yPath );
			_z = this.GetNodeFromPath<SpinBox>( _zPath );
			_w = this.GetNodeFromPath<SpinBox>( _wPath );

			_x.Connect("value_changed",this,nameof(OnChanged));
			_y.Connect("value_changed",this,nameof(OnChanged));
			_z.Connect("value_changed",this,nameof(OnChanged));
			_w.Connect("value_changed",this,nameof(OnChanged));
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;
			
			template.GetValue( "defaultValue", out Vector4 vector );
			_x.Value = vector.x;
			_y.Value = vector.y;
			_z.Value = vector.z;
			_w.Value = vector.w;
			
			_parentModel.AddValue( _label.Text, vector );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			LibT.Maths.Vector4 vector = new Vector4((float)_x.Value,(float)_y.Value,(float)_z.Value,(float)_w.Value);
			objData.AddValue( _label.Text, vector );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out Vector4 vector );

			_x.Value = vector.x;
			_y.Value = vector.y;
			_z.Value = vector.z;
			_w.Value = vector.w;
		}
		
		private void OnChanged(float value)
		{
			LibT.Maths.Vector4 vector = new Vector4((float)_x.Value,(float)_y.Value,(float)_z.Value,(float)_w.Value);
			_parentModel.AddValue( _label.Text, vector );
		}
		
		public string GetString()
		{
			return $"({(float)_x.Value},{(float)_y.Value},{(float)_z.Value},{(float)_w.Value})";
		}
	}
}