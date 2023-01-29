using Godot;

namespace MetaMaker.UI;

public partial class Inspector : Control
{
	[Export] public NodePath _toggleButtonPath;
	[Export] public NodePath _headerLabelPath;

	public Label headerLabel;
	
	public override void _Ready()
	{
		var button = GetNode<Button>( _toggleButtonPath );
		button.Connect("pressed", new Callable(this, nameof(ToggleVisibility)));
		
		headerLabel = GetNode<Label>( _headerLabelPath );
	}

	/// <summary>
	/// Hides and shows the inspector section
	/// </summary>
	private void ToggleVisibility()
	{
		Visible = !Visible;
	}
}
