using Godot;

namespace MetaMaker.UI.LayerFields;

public partial class PureLabel : Control, ILabel
{
	[Export] public NodePath labelPath;
	
	public string Label
	{
		get => _label.Text;
		set
		{
			if (_label != null)
			{
				_label.Text = value;
			}
		}
	}
	private Label _label;
	
	
	public override void _Ready()
	{
		_label = GetNode<Label>(labelPath);
	}
}