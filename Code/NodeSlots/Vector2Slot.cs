using Godot;
using LibT;
using LibT.Serialization;

public class Vector2Slot : Container, IGdaLoadable, IGdoConvertible
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

		data.GetValue( "defaultValue", out Vector2 vector );
		_x.Value = vector.x;
		_y.Value = vector.y;
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
}
