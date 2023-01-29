using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public partial class FieldListSlot : Container, IField
	{
		[Export] public NodePath _titlePath;
		private Label _label;
		[Export] public NodePath _selectorPath;
		private SpinBox _selector;
		[Export] public NodePath _addButtonPath;
		private Button _addButton;
		[Export] public NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataDictionary _field;
		private readonly List<Node> _children = new List<Node>();
		private GenericDataList _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		private readonly List<GenericDataDictionary> _childData = new List<GenericDataDictionary>();

		public int Count => _children.Count;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_selector = this.GetNodeFromPath<SpinBox>( _selectorPath );
			
			_addButton = this.GetNodeFromPath<Button>( _addButtonPath );
			_addButton.Connect("pressed",new Callable(this,nameof(Add)));
			
			_deleteButton = this.GetNodeFromPath<Button>( _deleteButtonPath );
			_deleteButton.Connect("pressed",new Callable(this,nameof(Delete)));
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			template.GetValue( "field", out _field );
			
			template.GetValue( "info", out string info );
			_label.TooltipText = info;

			parentModel.TryGetValue(_label.Text, out GenericDataList model);
			if(model != null)
			{
				_model = model;
				for( int i = 0; i < _model.values.Count; i++)
				{
					int select = (int)_selector.Value;
					int index = _graphNode.GetChildIndex( this )+1;
					index += select;
					_field.AddValue("label", _selector.Value.ToString());
					Node child = _graphNode.AddChildField( _field, index, _model );
					_children.Insert( select, child );
					_selector.MaxValue = _children.Count;
					_selector.Value++;

					if(child is LinkToChildSlot linkField)
					{
						linkField.parentListing = this;
					}

					UpdateField();
				}
			}
			else
			{
				_model = parentModel.TryAddValue(_label.Text, new GenericDataList()) as GenericDataList;
			}
		}

		public void Add()
		{
			int select = (int)_selector.Value;
			int index = _graphNode.GetChildIndex( this )+1;
			index += select;
			_field.AddValue("label", _selector.Value.ToString());
			if(select < _children.Count && select >= 0)
			{
				_model.AddValue(select, new GenericDataNull());
				for(int i = select; i < _children.Count; i++)
				{
					(_children[i] as IField).Label = (i+1).ToString();
				}
			}
			Node child = _graphNode.AddChildField( _field, index, _model );
			_children.Insert( select, child );
			_selector.MaxValue = _children.Count;
			_selector.Value++;

			if(child is LinkToChildSlot linkField)
			{
				linkField.parentListing = this;
			}

			UpdateField();
		}

		public void Delete()
		{
			var target = (int)_selector.Value;
			if( target >= _children.Count )
			{
				target--;
			}
			
			if(target<0) return;
			
			Node child = _children[target];
			_children.RemoveAt( child );
			_graphNode.RemoveChild( child );
			_model.RemoveValue(target);
			_selector.MaxValue = _children.Count;

			UpdateField();
		}

		private void UpdateField()
		{
			for (int i = 0; i < _children.Count; i++)
			{
				if( _children[i] is IField field )
				{
					field.Label = i.ToString();
				}
			}
			OnValueUpdated?.Invoke();
		}

		public int GetIndex(Node child)
		{
			for(int i = 0; i < _children.Count; i++)
			{
				if(_children[i] == child)
				{
					return i;
				}
			}

			return -1;
		}
	}
}