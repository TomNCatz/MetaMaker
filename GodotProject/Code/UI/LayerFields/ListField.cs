using System;
using Godot;
using LibT.Serialization;

namespace MetaMaker.UI.LayerFields;

public partial class ListField : PureLabel, IField
{
	[Export] public NodePath buttonPath;

	public event Action<GenericDataObject, string> onClick;

	private GenericDataList _gdl;
	
	public override void _Ready()
	{
		base._Ready();
		var button = GetNode<Button>(buttonPath);
		button.Connect("pressed", new Callable(this, nameof(OnButtonPressed)));
	}

	public void Init(GenericDataObject gdo)
	{
		_gdl = gdo as GenericDataList;
	}

	public void OnButtonPressed()
	{
		onClick?.Invoke(_gdl, Label);
	}
}