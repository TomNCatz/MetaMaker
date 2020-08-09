using Godot;
using LibT;
using LibT.Serialization;

public class Vector3Slot : Container, IGdaLoadable, IGdoConvertible
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _xPath;
	private SpinBox _x;
	[Export] private NodePath _yPath;
	private SpinBox _y;
	[Export] private NodePath _zPath;
	private SpinBox _z;

	public override void _Ready()
	{
		_label = this.GetNodeFromPath<Label>( _labelPath );
		
		_x = this.GetNodeFromPath<SpinBox>( _xPath );
		_y = this.GetNodeFromPath<SpinBox>( _yPath );
		_z = this.GetNodeFromPath<SpinBox>( _zPath );
	}
	
	public void LoadFromGda( GenericDataArray data )
	{
		data.GetValue( "label", out string label );
		_label.Text = label;

		data.GetValue( "defaultValue", out Vector3 vector );
		_x.Value = vector.x;
		_y.Value = vector.y;
		_z.Value = vector.z;
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
}
