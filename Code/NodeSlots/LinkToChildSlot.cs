using System;
using Godot;
using LibT.Serialization;
using LibT.Services;

namespace LibT
{
	public class LinkToChildSlot : Label, IGdaLoadable, IGdoConvertible
	{
		private ServiceInjection<JsonBuilder> _builderInjection = new ServiceInjection<JsonBuilder>();
		private SlottedGraphNode _child;
		private string _explicitNode;
		private SlottedGraphNode _parent;
		
		public override void _Ready()
		{
			_parent = GetParent<SlottedGraphNode>();
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			Text = label;

			if( data.values.ContainsKey( "explicitNode" ) )
			{
				data.GetValue( "explicitNode", out _explicitNode );
			}
		}

		public bool LinkChild( SlottedGraphNode child )
		{
			if( _child != null ) return false;

			_child = child;
			_parent.children.Add( child );
			return true;
		}

		public bool UnLinkChild( SlottedGraphNode child )
		{
			if( _child != child ) return false;

			_child = null;
			_parent.children.Remove( child );
			return true;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			if( _child == null )
			{
				objData.AddValue( Text, new GenericDataArray() );
			}
			else
			{
				objData.AddValue( Text, _child.GetObjectData() );
			}
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
			
			//SlottedGraphNode parent = GetParent<SlottedGraphNode>();
			
			SlottedGraphNode node = _builderInjection.Get.LoadNode( data );

			_builderInjection.Get.OnConnectionRequest(_parent.Name, _parent.GetChildIndex( this ),node.Name, 0);
		}
	}
}