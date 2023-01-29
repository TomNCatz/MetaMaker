using Godot;
using LibT.Serialization;

namespace MetaMaker.UI.LayerFields;

public partial class BoolField : PureLabel, IField
{
	[Export] public NodePath checkBoxPath;
	
	private GenericDataObject<bool> _gdo;
	private CheckBox _checkBox;
	
	public override void _Ready()
	{
		base._Ready();
		_checkBox = GetNode<CheckBox>(checkBoxPath);
		_checkBox.Connect("toggled", new Callable(this, nameof(OnValueChanged)));
	}

	public void Init(GenericDataObject gdo)
	{
		_gdo = gdo as GenericDataObject<bool>;
		_checkBox.SetPressedNoSignal(_gdo.value);
	}

	public void OnValueChanged(bool value)
	{
		_gdo.value = value;
	}
}