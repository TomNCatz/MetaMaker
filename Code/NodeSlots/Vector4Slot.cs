using Godot;
using LibT;
using LibT.Maths;
using LibT.Serialization;
using Color = Godot.Color;

public class Vector4Slot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _xPath;
	private SpinBox _x;
	[Export] private NodePath _yPath;
	private SpinBox _y;
	[Export] private NodePath _zPath;
	private SpinBox _z;
	[Export] private NodePath _wPath;
	private SpinBox _w;

	public override void _Ready()
	{
		_label = this.GetNodeFromPath<Label>( _labelPath );
		
		_x = this.GetNodeFromPath<SpinBox>( _xPath );
		_y = this.GetNodeFromPath<SpinBox>( _yPath );
		_z = this.GetNodeFromPath<SpinBox>( _zPath );
		_w = this.GetNodeFromPath<SpinBox>( _wPath );
	}
	
	public void LoadFromGda( GenericDataArray data )
	{
		data.GetValue( "label", out string label );
		_label.Text = label;
		
		data.GetValue( "defaultValue", out Vector4 vector );
		_x.Value = vector.x;
		_y.Value = vector.y;
		_z.Value = vector.z;
		_w.Value = vector.w;
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
	
	public string GetString()
	{
		return $"({(float)_x.Value},{(float)_y.Value},{(float)_z.Value},{(float)_w.Value})";
	}
}
