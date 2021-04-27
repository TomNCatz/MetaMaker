using System;
using Godot;
using System.Collections.Generic;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class SlottedGraphNode : GraphNode
	{
		[Export] public StyleBoxTexture normalStyle;
		[Export] public StyleBoxTexture selectedStyle;

		public bool LinkedToParent => ParentType >= 0;
		public int ParentType { get; private set; } = -1;
		public readonly List<SlottedGraphNode> children = new List<SlottedGraphNode>();
		public readonly Dictionary<string,BooleanSlot> booleanLookup = new Dictionary<string, BooleanSlot>();
		
		private readonly List<Node> slots = new List<Node>();
		private MainView _builder;
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();
		private string _title;

		public GenericDataArray Model => _model;
		private GenericDataArray _model;

		public bool Dirty{
			get => _dirty;
			set
			{
				_dirty = value;
				if(_dirty)
				{
					Title = _title + "*";
					_app.Get.HasUnsavedChanges = true;
				}
				else
				{
					Title = _title;
				}
			}
		}
		private bool _dirty;

		public string CleanTitle => _title;

		public override void _Ready()
		{
			_builder = GetParent().GetParent<MainView>();
		}

		public void SetSlots(GenericDataArray definition, GenericDataArray objData = null)
		{
			definition.GetValue( "title", out _title );
			Dirty = false;

			if(objData != null)
			{
				_model = objData;
				objData.GetValue( App.NODE_NAME_KEY, out _title );
				if(objData.values.ContainsKey( App.NODE_POSITION_KEY ))
				{
					objData.GetValue( App.NODE_POSITION_KEY, out Vector2 offset );
					Offset = offset;
				}
				if(objData.values.ContainsKey( App.NODE_SIZE_KEY ))
				{
					objData.GetValue( App.NODE_SIZE_KEY, out Vector2 size );
					RectSize = size;
				}
			}
			else
			{
				_model = new GenericDataArray();

				_model.AddValue( App.NODE_NAME_KEY, _title );
				_model.AddValue( App.NODE_POSITION_KEY, Offset );
				_model.AddValue( App.NODE_SIZE_KEY, RectSize );
			}
			
			Connect( "close_request", this, nameof(CloseRequest) );
			Connect( "resize_request", this, nameof(OnResizeRequest) );
			Connect( "offset_changed", this, nameof(OnMove) );
			
			if(definition.values.ContainsKey( "color" ))
			{
				definition.GetValue( "color", out Color color );
				if(color.a > 0.01f)
				{
					StyleBoxTexture style = normalStyle.Duplicate() as StyleBoxTexture;
					style.ModulateColor = color;
					Set( "custom_styles/frame", style );
					
					style = selectedStyle.Duplicate() as StyleBoxTexture;
					style.ModulateColor = color;
					Set( "custom_styles/selectedframe", style );
				}
			}
			
			if( definition.values.ContainsKey( "parentType" ) )
			{
				definition.GetValue( "parentType", out int parentType );
				ParentType = parentType;
				GenericDataArray gda = new GenericDataArray();
				gda.AddValue( "fieldType", FieldType.LINK_TO_PARENT );
				gda.AddValue( "slotType", ParentType );
				AddChildField( gda );
			}

			GenericDataArray listing = definition.GetGdo( "fields" ) as GenericDataArray;

			foreach( GenericDataObject dataObject in listing.values.Values )
			{
				AddChildField( dataObject as GenericDataArray );
				if (slots[slots.Count - 1] is IField field)
				{
					field.OnValueUpdated += OnSlotChange;
				}
			}

			if(objData != null) Dirty = false;
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

		public Node AddChildField( GenericDataArray fieldData, int insertIndex = -1, GenericDataArray parentOveride = null)
		{
			int leftType = -1;
			Color leftColor = Colors.Transparent;
			int rightType = -2;
			Color rightColor = Colors.Transparent;
			Node child;
			
			fieldData.GetValue( "fieldType", out FieldType fieldType );
			
			switch(fieldType)
			{
				case FieldType.SEPARATOR : 
					child = _builder.separatorScene.Instance();
					break;
				case FieldType.KEY : 
					child = _builder.keyScene.Instance();
					fieldData.GetValue( "slotType", out rightType );
					rightColor = _app.Get.GetKeyColor( rightType );
					break;
				case FieldType.KEY_TRACKER : 
					child = _builder.keyLinkScene.Instance();
					fieldData.GetValue( "slotType", out leftType );
					leftColor = _app.Get.GetKeyColor( leftType );
					break;
				case FieldType.LINK_TO_PARENT :
					fieldData.GetValue( "slotType", out leftType );
					
					if( leftType < 0 ) return null;
					
					child = _builder.linkToParentScene.Instance();
					leftColor = _app.Get.GetParentChildColor( leftType );
					break;
				case FieldType.LINK_TO_CHILD : 
					child = _builder.linkToChildScene.Instance();
					fieldData.GetValue( "slotType", out rightType );
					rightColor = _app.Get.GetParentChildColor( rightType );
					break;
				case FieldType.FIELD_LIST : 
					child = _builder.fieldListScene.Instance();
					break;
				case FieldType.FIELD_DICTIONARY : 
					child = _builder.fieldDictionaryScene.Instance();
					break;
				case FieldType.INFO : 
					child = _builder.infoScene.Instance();
					break;
				case FieldType.AUTO : 
					child = _builder.autoScene.Instance();
					break;
				case FieldType.TYPE : 
					child = _builder.typeScene.Instance();
					break;
				case FieldType.ENUM : 
					child = _builder.enumScene.Instance();
					break;
				case FieldType.FLAGS : 
					child = _builder.flagsScene.Instance();
					break;
				case FieldType.TEXT_LINE : 
					child = _builder.textLineScene.Instance();
					break;
				case FieldType.TEXT_AREA : 
					child = _builder.textAreaScene.Instance();
					break;
				case FieldType.TEXT_AREA_RICH : 
					child = _builder.textAreaRichScene.Instance();
					break;
				case FieldType.FLOAT : 
					child = _builder.floatScene.Instance();
					break;
				case FieldType.DOUBLE : 
					child = _builder.doubleScene.Instance();
					break;
				case FieldType.INT : 
					child = _builder.intScene.Instance();
					break;
				case FieldType.LONG : 
					child = _builder.longScene.Instance();
					break;
				case FieldType.BOOLEAN : 
					child = _builder.booleanScene.Instance();
					break;
				case FieldType.COLOR : 
					child = _builder.colorScene.Instance();
					break;
				case FieldType.VECTOR2 : 
					child = _builder.vector2Scene.Instance();
					break;
				case FieldType.VECTOR3 : 
					child = _builder.vector3Scene.Instance();
					break;
				case FieldType.VECTOR4 : 
					child = _builder.vector4Scene.Instance();
					break;
				case FieldType.QUATERNION : 
					child = _builder.vector4Scene.Instance();
					break;
				case FieldType.DATE_TIME : 
					child = _builder.dateTimeScene.Instance();
					break;
				case FieldType.DATE_TIME_OFFSET : 
					child = _builder.dateTimeOffsetScene.Instance();
					break;
				case FieldType.TIME_SPAN : 
					child = _builder.timeSpanScene.Instance();
					break;
				case FieldType.RELATIVE_PATH : 
					child = _builder.relativePathScene.Instance();
					break;
				default : throw new ArgumentOutOfRangeException( nameof(fieldData), fieldData, null );
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
				loadable.LoadFromGda( fieldData );
			}

			if( child is IField field )
			{
				field.Init( fieldData, parentOveride ?? _model );
			}

			return child;
		}

		public string GetPathToPort(int port)
		{
			return (GetSlot(port) as LinkToChildSlot).Label;
		}

		public new void RemoveChild( Node child )
		{
			int index = GetChildIndex( child );

			if( index < 0 ) return;
			
			var connections = _builder.GetConnectionsToNode( this );
			_builder.BreakConnections( connections );
			
			ShiftSlotsUp( index );

			slots.Remove( child );
			child.QueueFree();
			ShiftConnections( index, connections, true );
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
		}

		public void CloseRequest()
		{
			_builder.FreeNode( this );
			QueueFree();
		}

		private void OnResizeRequest(Vector2 newSize)
		{
			RectSize = new Vector2( newSize.x, newSize.y < RectSize.y ? newSize.y : RectSize.y );
			_model.AddValue( App.NODE_SIZE_KEY, RectSize );
			Dirty = true;
		}

		private void OnMove()
		{
			_model.AddValue( App.NODE_POSITION_KEY, Offset );
			Dirty = true;
		}

		private void OnSlotChange()
		{
			Dirty = true;
		}
	}
}