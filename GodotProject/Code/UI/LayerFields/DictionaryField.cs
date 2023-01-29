using System;
using Godot;
using LibT.Serialization;

namespace MetaMaker.UI.LayerFields;

public partial class DictionaryField : PureLabel, IField
{
	[Export] public NodePath buttonPath;

	public event Action<GenericDataObject, string> onClick;
	
	private GenericDataDictionary _gdd;
	
	public override void _Ready()
	{
		base._Ready();
		var button = GetNode<Button>(buttonPath);
		button.Connect("pressed", new Callable(this, nameof(OnButtonPressed)));
	}

	public void Init(GenericDataObject gdo)
	{
		_gdd = gdo as GenericDataDictionary;
	}

	public void OnButtonPressed()
	{
		onClick?.Invoke(_gdd, Label);
	}
}