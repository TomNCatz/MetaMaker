using Godot;
using LibT;
using LibT.Serialization;

public class Vector2Slot : Container, IGdaLoadable
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _xPath;
	private SpinBox _x;
	[Export] private NodePath _yPath;
	private SpinBox _y;

	public override void _Ready()
	{
		_label = this.GetNodeFromPath<Label>( _labelPath );
		
		_x = this.GetNodeFromPath<SpinBox>( _xPath );
		_y = this.GetNodeFromPath<SpinBox>( _yPath );
	}
	
	public void LoadFromGda( GenericDataArray data )
	{
		data.GetValue( "label", out string label );
		_label.Text = label;
			
		GenericDataArray gda = data.GetGdo( "defaultValue" ) as GenericDataArray;
		gda.GetValue( "x", out float x );
		gda.GetValue( "y", out float y );
		_x.Value = x;
		_y.Value = y;
	}
}
