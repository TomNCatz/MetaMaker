using System.Collections.Generic;
using Godot;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker.UI;

public partial class TreeView : Control
{
	
	[Export] public NodePath _contentAreaPath;
	private Control _contentArea;
	
	[Injectable]
	private Prefabricator _prefabricator;

	private List<TreeViewLayer> layers = new List<TreeViewLayer>();

	public override void _Ready()
	{
		_contentArea = GetNode<Control>( _contentAreaPath );
	}

	public void Display(GenericDataObject gdd, int index = 0, string label = null)
	{
		if (index > layers.Count) return;

		for (int i = layers.Count-1; i >= index; i--)
		{
			layers[i].QueueFree();
			layers.RemoveAt(i);
		}

		if (string.IsNullOrEmpty(label)) label = "Root";
		AddLayer(gdd, label);
	}

	public void Clear()
	{
		for (int i = layers.Count-1; i >= 0; i--)
		{
			layers[i].QueueFree();
			layers.RemoveAt(i);
		}
	}

	private void AddLayer(GenericDataObject gdd, string label)
	{
		Node child = _prefabricator.treeViewLayer.Instantiate();
		_contentArea.AddChild(child);
		var layer = child as TreeViewLayer;
		App.Instance.AppContext.Inject(layer);
		layer.Display(gdd, layers.Count, label);
		layers.Add(layer);
	}
}