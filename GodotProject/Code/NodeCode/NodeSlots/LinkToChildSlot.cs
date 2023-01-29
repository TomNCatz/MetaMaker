using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public partial class LinkToChildSlot : Control, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;

		private SlottedGraphNode _child;
		private string _explicitNode;
		private EmptyHandling _emptyHandling;
		private SlottedGraphNode _parent;
		private GenericDataObject _parentModel;

		[Injectable] private MainView _mainView;
		[Injectable] private App _app;
		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		public IField parentListing;

		public string LocalAddress
		{
			get
			{
				if(parentListing != null)
				{
					return parentListing.Label + "/" + Label;
				}

				return Label;
			}
		}

		private enum EmptyHandling
		{
			EMPTY_OBJECT,
			NULL,
			SKIP
		}
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_parent = GetParent<SlottedGraphNode>();
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.TooltipText = info;

			if( template.values.ContainsKey( App.GRAPH_EXPLICIT_KEY ) )
			{
				template.GetValue( App.GRAPH_EXPLICIT_KEY, out _explicitNode );
			}
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out _emptyHandling );
			}

			_parentModel = parentModel;
			_parentModel.TryGetValue( _label.Text, out GenericDataDictionary data );

			if( data == null || data.values.Count == 0 ) 
			{
				UpdateField();
				return;
			}

			if( !data.values.ContainsKey( App.NODE_NAME_KEY ) )
			{
				if( string.IsNullOrEmpty( _explicitNode ) )
				{
					throw new InvalidCastException($"Type definition not determinate for node {_parentModel}");
				}
				
				data.AddValue( App.NODE_NAME_KEY, _explicitNode );
			}

			SlottedGraphNode node = _app.LoadNode( data );

			_mainView.OnConnectionRequest(_parent.Name, _parent.GetChildIndex( this ),node.Name, 0);
		}

		public bool LinkChild( SlottedGraphNode child )
		{
			if( _child != null ) return false;

			_child = child;
			_parent.children.Add( child );
			UpdateField();

			return true;
		}

		public bool UnLinkChild( SlottedGraphNode child )
		{
			if( _child != child ) return false;

			_child = null;
			_parent.children.RemoveAt( child );
			UpdateField();

			return true;
		}

		private void UpdateField()
		{
			if( _child == null )
			{
				_parentModel.TryRemoveValue( _label.Text );
				switch (_emptyHandling)
				{
					case EmptyHandling.EMPTY_OBJECT:
						_parentModel.TryAddValue( _label.Text, new GenericDataDictionary() );
						break;
					case EmptyHandling.NULL:
						_parentModel.TryAddValue( _label.Text, new GenericDataNull() );
						break;
					case EmptyHandling.SKIP:
						_parentModel.TryAddValue( _label.Text, new GenericDataSkip() );
						break;
					default:
						break;
				}
			}
			else
			{
				_parentModel.TryRemoveValue( _label.Text );
				_parentModel.TryAddValue( _label.Text, _child.Model );
			}
			OnValueUpdated?.Invoke();
		}
	}
}