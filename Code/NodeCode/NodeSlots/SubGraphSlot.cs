using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class SubGraphSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _countLabelPath;
		private Label _countLabel;
		[Export] public NodePath _openButtonPath;
		private Button _openButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataDictionary _field;
		private GenericDataObject _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_countLabel = this.GetNodeFromPath<Label>( _countLabelPath );
			
			_openButton = this.GetNodeFromPath<Button>( _openButtonPath );
			_openButton.Connect( "pressed", this, nameof(Open) );
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			template.GetValue( "field", out _field );

			parentModel.TryGetValue(_label.Text, out GenericDataList listModel);
			parentModel.TryGetValue(_label.Text, out GenericDataDictionary dictModel);
			if(listModel != null)
			{
				_model = listModel;
				_countLabel.Text = listModel.values.Count.ToString();
			}
			if(dictModel != null)
			{
				_model = dictModel;
				_countLabel.Text = dictModel.values.Count.ToString();
			}
			else
			{
				template.GetValue( "fieldType", out FieldType fieldType );
				if(fieldType == FieldType.SUB_GRAPH_LIST)
				{
					_model = parentModel.TryAddValue(_label.Text, new GenericDataList());
				}
				else if( fieldType == FieldType.SUB_GRAPH_DICTIONARY)
				{
					_model = parentModel.TryAddValue(_label.Text, new GenericDataDictionary());
				}
				else
				{
					throw new System.Exception($"Sub graph of type '{fieldType}' not supported at {label}");
				}
				_countLabel.Text = "0";
			}
		}

		public void Open()
		{
			// todo open subgraph to this model
		}
	}
}