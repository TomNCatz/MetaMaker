using System.Collections.Generic;
using Godot;

namespace MetaMaker.UI;

public partial class Prefabricator : Node
{
	[Export] public PackedScene treeViewLayer;
	[Export] public PackedScene dictionaryField;
	[Export] public PackedScene listField;
	[Export] public PackedScene labelField;
	[Export] public PackedScene intField;
	[Export] public PackedScene floatField;
	[Export] public PackedScene boolField;
	[Export] public PackedScene textAreaField;
	[Export] public PackedScene stringField;
}