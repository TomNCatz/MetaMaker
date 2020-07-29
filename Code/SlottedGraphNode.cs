using System;
using Godot;
using System.Collections.Generic;
using LibT;
using LibT.Serialization;
using LibT.Services;

public class SlottedGraphNode : GraphNode, IGdoConvertible
{
	private List<Node> slots = new List<Node>();
	private JsonBuilder _builder;

	public bool LinkedToParent { get; private set; }
	public readonly List<SlottedGraphNode> children = new List<SlottedGraphNode>();
	public readonly Dictionary<string,BooleanSlot> booleanLookup = new Dictionary<string, BooleanSlot>();
	
	public override void _Ready()
	{
		_builder = GetParent().GetParent<JsonBuilder>();
		
	    Connect( "close_request", this, nameof(CloseRequest) );
	    Connect( "resize_request", this, nameof(OnResizeRequest) );
    }
	
	public void GetObjectData( GenericDataArray objData )
	{
		if(_builder.includeNodeData)
		{
			objData.AddValue( "ngMapNodeName", Title );
			if( _builder.includeGraphData )
			{
				objData.AddValue( "ngMapNodePosition", Offset );
				objData.AddValue( "ngMapNodeSize", RectSize );
			}
		}
		
		for( int i = 0; i < slots.Count; i++ )
		{
			if( slots[i] is IGdoConvertible convertable )
			{
				convertable.GetObjectData( objData );
			}

			if( slots[i] is FieldListSlot listSlot)
			{
				i += listSlot.Count;
			}

			if( slots[i] is FieldDictionarySlot dictionarySlot)
			{
				i += dictionarySlot.Count;
			}
		}
	}

	public void SetObjectData( GenericDataArray objData )
	{
		if(objData.values.ContainsKey( "ngMapNodePosition" ))
		{
			objData.GetValue( "ngMapNodePosition", out Vector2 offset );
			Offset = offset;
		}
		if(objData.values.ContainsKey( "ngMapNodeSize" ))
		{
			objData.GetValue( "ngMapNodeSize", out Vector2 size );
			RectSize = size;
		}

		for( int i = 0; i < slots.Count; i++ )
		{
			var slot = slots[i];
			if( slot is IGdoConvertible convertible )
			{
				convertible.SetObjectData( objData );
			}

			if( slot is FieldListSlot listSlot)
			{
				i += listSlot.Count;
			}

			if( slot is FieldDictionarySlot dictionarySlot)
			{
				i += dictionarySlot.Count;
			}
		}
	}

	public void SetSlots(GenericDataArray definition)
	{
		definition.GetValue( "title", out string title );
		Title = title;

		GenericDataArray listing = definition.GetGdo( "fields" ) as GenericDataArray;

		foreach( GenericDataObject dataObject in listing.values.Values )
		{
			AddChildField( dataObject as GenericDataArray );
		}
	}

	public Node GetSlot( int index )
    {
	    if( index < 0 )
	    {
		    throw new ArgumentOutOfRangeException( $"GetSlot({index}) cannot be negative" );
	    }
	    if( index >= slots.Count )
	    {
		    throw new ArgumentOutOfRangeException( $"GetSlot({index}) out of range, max is {slots.Count - 1}" );
	    }

	    return slots[index];
    }

	public Node AddChildField( GenericDataArray field, int insertIndex = -1)
    {
	    int leftType = -1;
	    Color leftColor = Colors.Transparent;
	    int rightType = -2;
	    Color rightColor = Colors.Transparent;
	    Node child;
	    
	    field.GetValue( "fieldType", out FieldType fieldType );
	    
	    switch(fieldType)
	    {
		    case FieldType.SEPARATOR : 
			    child = JsonBuilder.separatorScene.Instance();
			    break;
		    case FieldType.KEY : 
			    child = JsonBuilder.keyScene.Instance();
			    field.GetValue( "slotType", out rightType );
			    rightColor = JsonBuilder.GetKeyColor( rightType );
			    break;
		    case FieldType.KEY_TRACKER : 
			    child = JsonBuilder.keyLinkScene.Instance();
			    field.GetValue( "slotType", out leftType );
			    leftColor = JsonBuilder.GetKeyColor( leftType );
			    break;
		    case FieldType.LINK_TO_PARENT :
			    if( LinkedToParent )
			    {
				    throw new Exception( $"Node '{Title}' has more than one parent link defined!" );
			    }
			    LinkedToParent = true;
			    
			    child = JsonBuilder.linkToParentScene.Instance();
			    field.GetValue( "slotType", out leftType );
			    leftColor = JsonBuilder.GetParentChildColor( leftType );
			    break;
		    case FieldType.LINK_TO_CHILD : 
			    child = JsonBuilder.linkToChildScene.Instance();
			    field.GetValue( "slotType", out rightType );
			    rightColor = JsonBuilder.GetParentChildColor( rightType );
			    break;
		    case FieldType.FIELD_LIST : 
			    child = JsonBuilder.fieldListScene.Instance();
			    break;
		    case FieldType.FIELD_DICTIONARY : 
			    child = JsonBuilder.fieldDictionaryScene.Instance();
			    break;
		    case FieldType.INFO : 
			    child = JsonBuilder.infoScene.Instance();
			    break;
		    case FieldType.AUTO : 
			    child = JsonBuilder.autoScene.Instance();
			    break;
		    case FieldType.TYPE : 
			    child = JsonBuilder.typeScene.Instance();
			    break;
		    case FieldType.ENUM : 
			    child = JsonBuilder.enumScene.Instance();
			    break;
		    case FieldType.TEXT_LINE : 
			    child = JsonBuilder.textLineScene.Instance();
			    break;
		    case FieldType.TEXT_AREA : 
			    child = JsonBuilder.textAreaScene.Instance();
			    break;
		    case FieldType.TEXT_AREA_RICH : 
			    child = JsonBuilder.textAreaRichScene.Instance();
			    break;
		    case FieldType.FLOAT : 
			    child = JsonBuilder.floatScene.Instance();
			    break;
		    case FieldType.INT : 
			    child = JsonBuilder.intScene.Instance();
			    break;
		    case FieldType.LONG : 
			    child = JsonBuilder.longScene.Instance();
			    break;
		    case FieldType.BOOLEAN : 
			    child = JsonBuilder.booleanScene.Instance();
			    break;
		    case FieldType.COLOR : 
			    child = JsonBuilder.colorScene.Instance();
			    break;
		    case FieldType.VECTOR2 : 
			    child = JsonBuilder.vector2Scene.Instance();
			    break;
		    case FieldType.VECTOR3 : 
			    child = JsonBuilder.vector3Scene.Instance();
			    break;
		    case FieldType.VECTOR4 : 
			    child = JsonBuilder.vector4Scene.Instance();
			    break;
		    case FieldType.QUATERNION : 
			    child = JsonBuilder.vector4Scene.Instance();
			    break;
		    case FieldType.DATE_TIME : 
			    child = JsonBuilder.dateTimeScene.Instance();
			    break;
		    case FieldType.DATE_TIME_OFFSET : 
			    child = JsonBuilder.dateTimeOffsetScene.Instance();
			    break;
		    case FieldType.TIME_SPAN : 
			    child = JsonBuilder.timeSpanScene.Instance();
			    break;
		    default : throw new ArgumentOutOfRangeException( nameof(field), field, null );
	    }

	    if( insertIndex < 0 )
	    {
		    insertIndex = slots.Count;
		    AddChild(child);
		    slots.Add( child );
	    }
	    else
	    {
		    var connections = _builder.GetConnectionsToNode( this );
		    _builder.BreakConnections( connections );
		    
		    AddChild(child);
		    MoveChild(child, insertIndex);
		    slots.Insert( insertIndex, child );
		    ShiftSlotsDown( insertIndex );
		    
		    ShiftConnections( insertIndex, connections, false );
	    }
	    
	    SetSlot( insertIndex, 
		    true, leftType, leftColor, 
		    true, rightType, rightColor );

	    if( child is IGdaLoadable loadable )
	    {
		    loadable.LoadFromGda( field );
	    }

	    return child;
    }

	public void RemoveChild( Node child )
	{
		int index = GetChildIndex( child );

		if( index < 0 ) return;
		
		ShiftSlotsUp( index );

		slots.Remove( child );
		child.QueueFree();
	}
	
	public int GetChildIndex( Node child )
	{
		int length = GetChildCount();
		
		for( int i = 0; i < length; i++ )
		{
			if( GetChild( i ) != child ) continue;

			return i;
		}

		return -1;
	}

    private void ShiftSlotsDown(int start)
    {
	    var connections = _builder.GetConnectionsToNode( this );
	    _builder.BreakConnections( connections );
	    
	    for( int i = slots.Count-2; i >= start; i-- )
	    {
		    int leftType = GetSlotTypeLeft( i );
		    Color leftColor = GetSlotColorLeft( i );
		    int rightType = GetSlotTypeRight( i );
		    Color rightColor = GetSlotColorRight( i );
		    
		    SetSlot( i+1, 
			    true, leftType, leftColor, 
			    true, rightType, rightColor );
	    }
    }

    private void ShiftConnections(int start, List<GraphConnection> connections, bool up)
    {
	    for( int i = 0; i < connections.Count; i++ )
	    {
		    GraphConnection connection = connections[i];
		    if( connection.goingRight )
		    {
			    if( up && connection.fromPort == start )
			    {
				    connections.RemoveAt( i );
				    i--;
				    continue;
			    }
			    
			    if( connection.fromPort >= start )
			    {
				    connection.fromPort += up ? -1 : 1;
			    }
		    }
		    else
		    {
			    if( up && connection.toPort == start )
			    {
				    connections.RemoveAt( i );
				    i--;
				    continue;
			    }
			    
			    if( connection.toPort >= start )
			    {
				    connection.toPort += up ? -1 : 1;
			    }
		    }
	    }
	    
	    _builder.RebuildConnections( connections );
    }
    
    private void ShiftSlotsUp(int start)
    {
	    var connections = _builder.GetConnectionsToNode( this );
	    _builder.BreakConnections( connections );
	    
	    for( int i = start+1; i < slots.Count; i++ )
	    {
		    int leftType = GetSlotTypeLeft( i );
		    Color leftColor = GetSlotColorLeft( i );
		    int rightType = GetSlotTypeRight( i );
		    Color rightColor = GetSlotColorRight( i );
		    
		    SetSlot( i-1, 
			    true, leftType, leftColor, 
			    true, rightType, rightColor );
	    }

	    ShiftConnections( start, connections, true );
    }

    public void CloseRequest()
    {
	    JsonBuilder builder = ServiceProvider.Get<JsonBuilder>();
	    builder.FreeNode( this );
	    
	    QueueFree();
    }

    private void OnResizeRequest(Vector2 newSize)
    {
	    RectSize = new Vector2( newSize.x, newSize.y < RectSize.y ? newSize.y : RectSize.y );
    }
}
