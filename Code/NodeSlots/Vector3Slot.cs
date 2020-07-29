using Godot;
using LibT;
using LibT.Serialization;

public class Vector3Slot : Container, IGdaLoadable
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
			
		GenericDataArray gda = data.GetGdo( "defaultValue" ) as GenericDataArray;
		gda.GetValue( "x", out float x );
		gda.GetValue( "y", out float y );
		gda.GetValue( "z", out float z );
		_x.Value = x;
		_y.Value = y;
		_z.Value = z;
	}
}
