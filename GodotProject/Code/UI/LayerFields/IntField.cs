using Godot;
using LibT.Serialization;

namespace MetaMaker.UI.LayerFields;

public partial class IntField : PureLabel, IField
{
	[Export] public NodePath spinBoxPath;
	
	private GenericDataObject<int> _gdo;
	private SpinBox _spinBox;
	
	public override void _Ready()
	{
		base._Ready();
		_spinBox = GetNode<SpinBox>(spinBoxPath);
		_spinBox.Connect("value_changed", new Callable(this, nameof(OnValueChanged)));
	}

	public void Init(GenericDataObject gdo)
	{
		_gdo = gdo as GenericDataObject<int>;
		_spinBox.Value = _gdo.value;
	}

	public void OnValueChanged(float value)
	{
		_gdo.value = (int)value;
	}
}