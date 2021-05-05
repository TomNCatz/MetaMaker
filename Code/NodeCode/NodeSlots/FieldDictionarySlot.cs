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
		private Label _label;
		[Export] public NodePath _selectorPath;
		private LineEdit _selector;
		[Export] public NodePath _addButtonPath;
		private Button _addButton;
		[Export] public NodePath _deleteButtonPath;
		private Button _deleteButton;
		
		private SlottedGraphNode _graphNode;
		private GenericDataDictionary _field;
		private readonly Dictionary<string,Node> _children = new Dictionary<string, Node>();
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();

		public int Count => _children.Count;
		private GenericDataDictionary _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_selector = this.GetNodeFromPath<LineEdit>( _selectorPath );
			
			_addButton = this.GetNodeFromPath<Button>( _addButtonPath );
			_addButton.Connect( "pressed", this, nameof(Add) );
			
			_deleteButton = this.GetNodeFromPath<Button>( _deleteButtonPath );
			_deleteButton.Connect( "pressed", this, nameof(Delete) );
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			_field = template.GetGdo( "field" ) as GenericDataDictionary;

			parentModel.TryGetValue(_label.Text, out GenericDataDictionary model);
			if(model != null)
			{
				_model = model;
				
				foreach( var dataArray in _model.values )
				{
					_selector.Text = dataArray.Key;
					InternalAdd();
				}
			}
			else
			{
				_model = parentModel.TryAddValue(_label.Text, new GenericDataDictionary()) as GenericDataDictionary;
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
				
				InternalAdd();
				
				OnValueUpdated?.Invoke();
			}
			catch(Exception ex)
			{
				_app.Get.CatchException(ex);
			}
		}

		private void InternalAdd()
		{
			_field.AddValue("label", _selector.Text);
			int index = _graphNode.GetChildIndex(this) + 1;
			_children[_selector.Text] = _graphNode.AddChildField(_field, index, _model);

			if(_children[_selector.Text] is LinkToChildSlot linkField)
			{
				linkField.parentListing = this;
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