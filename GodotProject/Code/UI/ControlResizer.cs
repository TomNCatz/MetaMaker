using Godot;

namespace MetaMaker.UI;

public partial class ControlResizer : GodotObject
{
	private Control _resizeGrip;
	private Control _parent;
	
	public float trueMin = 100f;

	private float startingPosition;
	private float startingWidth;
	private bool pressed;


	public ControlResizer(Control parent, NodePath resizeGripPath)
	{
		_parent = parent;
		_parent.CustomMinimumSize = new Vector2(trueMin, 0);
		
		_resizeGrip = _parent.GetNode<Control>(resizeGripPath);
		_resizeGrip.Connect("gui_input", new Callable(this, nameof(ResizeArea)));
	}
	
	
	public void Process()
	{
		if(!pressed) return;
		
		var pos = _parent.GetViewport().GetMousePosition();
		var x = pos.X - startingPosition + startingWidth;
		if (x < trueMin) x = trueMin;
		_parent.CustomMinimumSize = new Vector2(x, 0);
	}
	
	private void ResizeArea( InputEvent @event )
	{
		if (@event is InputEventMouseButton mouseButton
		    && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if( mouseButton.Pressed)
			{
				startingPosition = _parent.GetViewport().GetMousePosition().X;
				startingWidth = _parent.Size.X;
				pressed = true;
			}
			else
			{
				pressed = false;
			}
		}
	}
}