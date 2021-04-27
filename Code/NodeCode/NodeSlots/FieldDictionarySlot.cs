using System;
using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class FieldDictionarySlot : Container, IField
	{
		[Export] public NodePath _titlePath;
		private Label _title;
		[Export] public NodePath _selectorPath;
		private LineEdit _selector;
		[Export] public NodePath _addButtonPath;
		private Button _addButton;
		[Export] public NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataArray _field;
		private readonly Dictionary<string,Node> _children = new Dictionary<string, Node>();
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();

		public int Count => _children.Count;
		public string Label => _title.Text;
		private GenericDataArray _parentModel;
		private GenericDataArray _model;
		public event System.Action OnValueUpdated;

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
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_title.Text = label;
			_parentModel = parentModel;
			_field = template.GetGdo( "field" ) as GenericDataArray;
			
			if(parentModel.values.ContainsKey(_title.Text))
			{
				parentModel.GetValue( _title.Text, out _model );
				parentModel.GetValue( _title.Text, out Dictionary<string,GenericDataObject> childSlots );
				
				foreach( var dataArray in childSlots )
				{
					_selector.Text = dataArray.Key;
					Add();
				}
			}
			else
			{
				_model = new GenericDataArray();
				_parentModel.AddValue(_title.Text, _model);
			}
		}

		public void Add()
		{
			try
			{
				if (string.IsNullOrEmpty(_selector.Text))
				{
					throw new NullReferenceException("Must specify a key to add");
				}

				if (_children.ContainsKey(_selector.Text))
				{
					throw new ArgumentException("That key already exists in the dictionary");
				}

				_field.AddValue("label", _selector.Text);
				int index = _graphNode.GetChildIndex(this) + 1;
				_children[_selector.Text] = _graphNode.AddChildField(_field, index, _model);

				if(_children[_selector.Text] is LinkToChildSlot link)
				{
					link.parentListing = this;
				}
				
				OnValueUpdated?.Invoke();
			}
			catch(Exception ex)
			{
				_app.Get.CatchException(ex);
			}
		}

		public void Delete()
		{
			try
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
				_model.RemoveValue(_selector.Text);
				OnValueUpdated?.Invoke();
			}
			catch(Exception ex)
			{
				_app.Get.CatchException(ex);
			}
		}
	}
}