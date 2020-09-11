using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT.Serialization;
using LibT.Services;

namespace LibT
{
	public class ValidateStringSlot : Container, IGdaLoadable, IGdoConvertible
	{
		private ServiceInjection<JsonBuilder> _builder = new ServiceInjection<JsonBuilder>();
		private SlottedGraphNode _graphNode;
		private GenericDataArray _field;
		private Node _child;
		private string _saveName;
		private bool _asList;

		public override void _Ready()
		{
			_graphNode = GetParent<SlottedGraphNode>();
		}
		
		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "saveName", out _saveName );
			data.GetValue( "asList", out _asList );
			data.GetValue( "validation", out _field );
			
			if( _field == null ) return;
			
			int index = _graphNode.GetChildIndex( this )+1;
			_child = _graphNode.AddChildField( _field, index );
		}

		public void GetObjectData( GenericDataArray objData )
		{
			List<GenericDataObject> childSlots = new List<GenericDataObject>();

			if( _child == null ) return;
			if( !( _child is IGdoConvertible convertible ) ) return;

			GenericDataArray gda = convertible.GetObjectData();
			if( gda.values.Count > 0 )
			{
				childSlots.Add( gda.values.Values.First() );
			}
			
			if(_builder.Get.includeNodeData && _builder.Get.includeGraphData )
			{
				convertible.GetObjectData( objData );
			}

			if( _child is StringRetriever retriever )
			{
				if( _asList )
				{
					_graphNode.AddValidatedToList( _saveName, retriever.GetString() );
				}
				else
				{
					objData.AddValue( _saveName, retriever.GetString() );
				}
			}
		}

		public void SetObjectData( GenericDataArray objData )
		{
			if( !( _child is IGdoConvertible convertible ) ) return;

			convertible.SetObjectData( objData );
		}
	}
}