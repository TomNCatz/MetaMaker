using Godot;
using LibT.Serialization;

namespace MetaMaker.UI.LayerFields;

public partial class StringField : PureLabel, IField
{
	[Export] public NodePath lineEditPath;
	
	private GenericDataObject<string> _gdo;
	private LineEdit _lineEdit;
	
	public override void _Ready()
	{
		base._Ready();
		_lineEdit = GetNode<LineEdit>(lineEditPath);
		_lineEdit.Connect("text_changed", new Callable(this, nameof(OnValueChanged)));
	}

	public void Init(GenericDataObject gdo)
	{
		_gdo = gdo as GenericDataObject<string>;
		_lineEdit.Text = _gdo.value;
	}

	public void OnValueChanged(string value)
	{
		_gdo.value = value;
	}
}