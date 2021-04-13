using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class LinkToChildSlot : Label, IField, IGdoConvertible
	{
		private readonly ServiceInjection<MainView> _mainView = new ServiceInjection<MainView>();
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();
		private SlottedGraphNode _child;
		private string _explicitNode;
		private EmptyHandling emptyHandling;
		private SlottedGraphNode _parent;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;

		private enum EmptyHandling
		{
			EMPTY_OBJECT,
			SKIP
		}
		
		public override void _Ready()
		{
			_parent = GetParent<SlottedGraphNode>();
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			Text = label;

			if( template.values.ContainsKey( "explicitNode" ) )
			{
				template.GetValue( "explicitNode", out _explicitNode );
			}
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out emptyHandling );
			}

			_parentModel = parentModel;
		}

		public bool LinkChild( SlottedGraphNode child )
		{
			if( _child != null ) return false;

			_child = child;
			_parent.children.Add( child );
			UpdateField( _parentModel );

			return true;
		}

		public bool UnLinkChild( SlottedGraphNode child )
		{
			if( _child != child ) return false;

			_child = null;
			_parent.children.Remove( child );
			UpdateField( _parentModel );

			return true;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			UpdateField( objData );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( Text, out GenericDataArray data );

			if( data == null || data.values.Count == 0 ) return;

			if( !data.values.ContainsKey( "ngMapNodeName" ) )
			{
				if( string.IsNullOrEmpty( _explicitNode ) )
				{
					throw new InvalidCastException($"Type definition not determinate for node {objData}");
				}
				
				data.AddValue( "ngMapNodeName", _explicitNode );
			}

			SlottedGraphNode node = _app.Get.LoadNode( data );

			_mainView.Get.OnConnectionRequest(_parent.Name, _parent.GetChildIndex( this ),node.Name, 0);
		}

		private void UpdateField( GenericDataArray objData )
		{
			if( _child == null ||
			    ( _mainView.Get.copyingData && 
			       !_mainView.Get.IsSelected( _child )))
			{
				switch(emptyHandling)
				{
					case EmptyHandling.EMPTY_OBJECT :
						objData.AddValue( Text, new GenericDataArray() );
						break;
					case EmptyHandling.SKIP : 
						objData.RemoveValue(Text);
						break;
					default : throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				objData.AddValue( Text, _child.Model );
			}
			OnValueUpdated?.Invoke();
		}
	}
}