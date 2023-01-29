using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;
using MetaMaker.Services;
using MetaMaker.UI.LayerFields;

namespace MetaMaker.UI;

public partial class TreeViewLayer : Control
{
	[Export] public NodePath _labelPath;
	private Label _label;
	
	[Export] public NodePath _contentAreaPath;
	private Control _contentArea;
	
	[Export] public NodePath _resizeGripPath;
	private ControlResizer _resizer;
	
	[Injectable]
	private TemplateService _templateService;
	[Injectable]
	private Prefabricator _prefabricator;
	[Injectable]
	private TreeView _treeView;

	private bool _list;
	private GenericDataDictionary _gdd;
	private GenericDataList _gdl;
	
	
	public int Layer { get; private set; }
	
	public override void _Ready()
	{
		_label = GetNode<Label>( _labelPath );
		_contentArea = GetNode<Control>( _contentAreaPath );
		_resizer = new ControlResizer(this, _resizeGripPath);
	}
	
	public override void _Process(double delta)
	{
		_resizer.Process();
	}

	public void Display(GenericDataObject gdo, int index, string label)
	{
		_label.Text = label;
		Layer = index;
		_gdd = gdo as GenericDataDictionary;
		_gdl = gdo as GenericDataList;
		_list = _gdl != null;
		if (!_list && _gdd == null) return;
		
		if (_list)
		{
			for (int i = 0; i < _gdl.values.Count; i++)
			{
				var item = GetLabel(_gdl.values[i]);
				if(item == null) continue;
				
				item.Label = i.ToString();
			}
		}
		else
		{
			foreach (var dataPair in _gdd.values)
			{
				var item = GetLabel(dataPair.Value);
				if(item == null) continue;

				item.Label = dataPair.Key;
			}
		}
	}

	private void ShowChild(GenericDataObject gdo, string label)
	{
		_treeView.Display(gdo, Layer+1, label);
	}

	private ILabel GetLabel(GenericDataObject gdo)
	{
		Node child = null;

		switch (gdo.type)
		{
			case DataTypeIndicator.OBJECT:
				child = _prefabricator.dictionaryField.Instantiate();
				(child as DictionaryField).onClick += ShowChild;
				break;
			case DataTypeIndicator.LIST:
				child = _prefabricator.listField.Instantiate();
				(child as ListField).onClick += ShowChild;
				break;
			case DataTypeIndicator.INT:
				child = _prefabricator.intField.Instantiate();
				break;
			case DataTypeIndicator.FLOAT:
				child = _prefabricator.floatField.Instantiate();
				break;
			case DataTypeIndicator.BOOL:
				child = _prefabricator.boolField.Instantiate();
				break;
			case DataTypeIndicator.STRING:
				child = _prefabricator.stringField.Instantiate();
				break;
			case DataTypeIndicator.NULL:
				child = _prefabricator.labelField.Instantiate();
				break;
			case DataTypeIndicator.SKIP:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (child == null) return null;
		
		_contentArea.AddChild(child);

		if (child is IField field)
		{
			field.Init(gdo);
		}
			
		return child as ILabel;
	}
}
