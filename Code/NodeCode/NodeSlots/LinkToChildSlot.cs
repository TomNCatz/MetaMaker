using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class LinkToChildSlot : Control, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;

		private readonly ServiceInjection<MainView> _mainView = new ServiceInjection<MainView>();
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();
		private SlottedGraphNode _child;
		private string _explicitNode;
		private EmptyHandling emptyHandling;
		private SlottedGraphNode _parent;
		private GenericDataObject _parentModel;


		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		// public string Label
		// {
		// 	get
		// 	{
		// 		if(parentListing != null)
		// 		{
		// 			if(parentListing is FieldListSlot listSlot)
		// 			{
		// 				return listSlot.Label + "/" + listSlot.GetIndex(this);
		// 			}

		// 			if(parentListing is FieldDictionarySlot dictSlot)
		// 			{
		// 				return dictSlot.Label + "/" + Text;
		// 			}
		// 		}

		// 		return Text;
		// 	}
		// }

		private enum EmptyHandling
		{
			EMPTY_OBJECT,
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

			if( template.values.ContainsKey( App.GRAPH_EXPLICIT_KEY ) )
			{
				template.GetValue( App.GRAPH_EXPLICIT_KEY, out _explicitNode );
			}
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out emptyHandling );
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

			SlottedGraphNode node = _app.Get.LoadNode( data );

			_mainView.Get.OnConnectionRequest(_parent.Name, _parent.GetChildIndex( this ),node.Name, 0);
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
			_parent.children.Remove( child );
			UpdateField();

			return true;
		}

		private void UpdateField()
		{
			// TODO : We lost the option to skip empties, I would like something similar back somehow
			if( _child == null )
			{
				_parentModel.TryRemoveValue( _label.Text );
				_parentModel.TryAddValue( _label.Text, new GenericDataDictionary() );
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