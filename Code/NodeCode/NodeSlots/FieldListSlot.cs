using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class FieldListSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _titlePath;
		private Label _title;
		[Export] public NodePath _selectorPath;
		private SpinBox _selector;
		[Export] public NodePath _addButtonPath;
		private Button _addButton;
		[Export] public NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataArray _field;
		private readonly List<Node> _children = new List<Node>();
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		private readonly List<GenericDataArray> _childData = new List<GenericDataArray>();

		public int Count => _children.Count;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_selector = this.GetNodeFromPath<SpinBox>( _selectorPath );
			
			_addButton = this.GetNodeFromPath<Button>( _addButtonPath );
			_addButton.Connect( "pressed", this, nameof(Add) );
			
			_deleteButton = this.GetNodeFromPath<Button>( _deleteButtonPath );
			_deleteButton.Connect( "pressed", this, nameof(Delete) );
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_title.Text = label;
			template.GetValue( "field", out _field );
			
			_parentModel = parentModel;
		}

		public void GetObjectData( GenericDataArray objData )
		{
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _title.Text, out List<GenericDataObject> childSlots );

			foreach( GenericDataObject dataArray in childSlots )
			{
				Add();
				
				if( !( _children[_children.Count - 1] is IGdoConvertible convertible ) ) continue;

				GenericDataArray gda = new GenericDataArray();
				_field.GetValue( "label", out string name );
				gda.AddValue( name, dataArray );
				convertible.SetObjectData( gda );
			}
		}

		public void Add()
		{
			int index = _graphNode.GetChildIndex( this )+1;
			index += (int)_selector.Value;
			GenericDataArray childData = new GenericDataArray();
			Node child = _graphNode.AddChildField( _field, index, childData );
			_children.Insert( (int)_selector.Value, child );
			_childData.Insert( (int)_selector.Value, childData );
			_selector.MaxValue = _children.Count;
			_selector.Value++;
			(child as IField).OnValueUpdated += UpdateField;
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
			_children.Remove( child );
			_childData.RemoveAt( target );
			_graphNode.RemoveChild( child );
			_selector.MaxValue = _children.Count;
			UpdateField();
		}

		private void UpdateField()
		{
			List<GenericDataObject> childSlots = new List<GenericDataObject>();

			foreach(GenericDataArray gda in _childData)
			{
				if( gda == null ) continue;

				if( gda.values.Count > 0 )
				{
					childSlots.Add( gda.values.Values.First() );
				}
			}
			_parentModel.AddValue( _title.Text, childSlots );
			OnValueUpdated?.Invoke();
		}
	}
}