using System;
using System.Collections.Generic;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class FieldDictionarySlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _titlePath;
		private Label _title;
		[Export] private NodePath _selectorPath;
		private LineEdit _selector;
		[Export] private NodePath _addButtonPath;
		private Button _addButton;
		[Export] private NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataArray _field;
		private Dictionary<string,Node> _children = new Dictionary<string, Node>();

		public int Count => _children.Count;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_selector = this.GetNodeFromPath<LineEdit>( _selectorPath );
			
			_addButton = this.GetNodeFromPath<Button>( _addButtonPath );
			_addButton.Connect( "pressed", this, nameof(Add) );
			
			_deleteButton = this.GetNodeFromPath<Button>( _deleteButtonPath );
			_deleteButton.Connect( "pressed", this, nameof(Delete) );
		}
		
		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_title.Text = label;
			_field = data.GetGdo( "field" ) as GenericDataArray;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			Dictionary<string,IGdoConvertible> items = new Dictionary<string, IGdoConvertible>();
			foreach( var child in _children )
			{
				if( child.Value == null )
				{
					items[child.Key] = null;
					continue;
				}

				if( !( child.Value is IGdoConvertible convertible ) )
				{
					throw new Exception($"Child item '{child.Value.Name}' is not an IGdoConvertible");
				}

				GenericDataArray gda = convertible.GetObjectData();
				if( gda.values.Count > 0 )
				{
					items[child.Key] = convertible;
				}
			}
			objData.AddValue( _title.Text, items );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _title.Text, out Dictionary<string,GenericDataArray> childSlots );
			
			foreach( var dataArray in childSlots )
			{
				_selector.Text = dataArray.Key;
				Add();
				
				if( !( _children[dataArray.Key] is IGdoConvertible convertible ) )
				{
					throw new Exception($"Child item '{_children[dataArray.Key].Name}' is not an IGdoConvertible");
				}

				convertible.SetObjectData( dataArray.Value );
			}
		}

		public void Add()
		{
			if( string.IsNullOrEmpty( _selector.Text ) )
			{
				throw new NullReferenceException("Must specify a key to add");
			}

			if( _children.ContainsKey( _selector.Text ) )
			{
				throw new ArgumentException("That key already exists in the dictionary");
			}
			
			_field.AddValue( "label", _selector.Text );
			int index = _graphNode.GetChildIndex( this )+1;
			Node child = _graphNode.AddChildField( _field, index );
			_children[_selector.Text] = child;
		}

		public void Delete()
		{
			if( string.IsNullOrEmpty( _selector.Text ) )
			{
				throw new NullReferenceException("Must specify a key to remove");
			}
			if( !_children.ContainsKey( _selector.Text ) )
			{
				throw new ArgumentException("That key does not exist in the dictionary");
			}
			
			Node child = _children[_selector.Text];
			
			_children.Remove( _selector.Text );
			_graphNode.RemoveChild( child );
		}
	}
}