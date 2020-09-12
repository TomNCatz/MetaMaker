using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class FieldListSlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _titlePath;
		private Label _title;
		[Export] private NodePath _selectorPath;
		private SpinBox _selector;
		[Export] private NodePath _addButtonPath;
		private Button _addButton;
		[Export] private NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataArray _field;
		private readonly List<Node> _children = new List<Node>();

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
		
		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_title.Text = label;
			data.GetValue( "field", out _field );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			List<GenericDataObject> childSlots = new List<GenericDataObject>();

			foreach( Node node in _children )
			{
				if( node == null ) continue;

				if( !( node is IGdoConvertible convertible ) ) continue;

				GenericDataArray gda = convertible.GetObjectData();
				if( gda.values.Count > 0 )
				{
					childSlots.Add( gda.values.Values.First() );
				}
			}
			objData.AddValue( _title.Text, childSlots );
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
			Node child = _graphNode.AddChildField( _field, index );
			_children.Insert( (int)_selector.Value, child );
			_selector.MaxValue = _children.Count;
			_selector.Value++;
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
			_graphNode.RemoveChild( child );
			_selector.MaxValue = _children.Count;
		}

		public List<string> GetStrings()
		{
			List<string> strings = new List<string>();
			
			foreach( Node child in _children )
			{

				if( child is StringRetriever retriever )
				{
					strings.Add( retriever.GetString() );
				}
			}

			return strings;
		}
	}
}