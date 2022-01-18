using System;
using Godot;
using System.Collections.Generic;
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
		private MainView _mainView;
		private string _title;

		public GenericDataDictionary Model => _model;
		private GenericDataDictionary _model;
		private GenericDataDictionary _definition;

		public bool Dirty{
			get => _dirty;
			set
			{
				_dirty = value;
				if(_dirty)
				{
					Title = _title + "*";
					ServiceInjection<App>.Service.HasUnsavedChanges = true;
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
			_mainView = GetParent().GetParent<MainView>();
		}

		public void SetSlots(GenericDataDictionary definition, GenericDataDictionary objData = null)
		{
			_definition = definition;
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
				_model = new GenericDataDictionary();

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
				GenericDataDictionary gda = new GenericDataDictionary();
				gda.AddValue( "fieldType", FieldType.LINK_TO_PARENT );
				gda.AddValue( "slotType", ParentType );
				AddChildField( gda );
			}

			GenericDataList listing = definition.GetGdo( "fields" ) as GenericDataList;

			foreach( GenericDataObject dataObject in listing.values )
			{
				AddChildField( dataObject as GenericDataDictionary );
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

		public Node AddChildField( GenericDataDictionary fieldData, int insertIndex = -1, GenericDataObject parentOveride = null)
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
					child = _mainView.separatorScene.Instance();
					break;
				case FieldType.KEY : 
					child = _mainView.keyScene.Instance();
					fieldData.GetValue( "slotType", out leftType );
					leftColor = ServiceInjection<App>.Service.GetKeyColor( leftType );
					break;
				case FieldType.KEY_TRACKER : 
					child = _mainView.keyLinkScene.Instance();
					fieldData.GetValue( "slotType", out rightType );
					rightColor = ServiceInjection<App>.Service.GetKeyColor( rightType );
					break;
				case FieldType.KEY_MANUAL : 
					child = _mainView.keyManualScene.Instance();
					fieldData.GetValue( "slotType", out rightType );
					rightColor = ServiceInjection<App>.Service.GetKeyColor( rightType );
					break;
				case FieldType.KEY_SELECT : 
					child = _mainView.keySelectScene.Instance();
					break;
				case FieldType.LINK_TO_PARENT :
					fieldData.GetValue( "slotType", out leftType );
					child = _mainView.linkToParentScene.Instance();
					(child as LinkToParentSlot).LinkType = ParentType;
					
					if( leftType != _mainView.CurrentParentIndex)
					{
						leftColor = ServiceInjection<App>.Service.GetParentChildColor( leftType );
					}
					break;
				case FieldType.LINK_TO_CHILD : 
					child = _mainView.linkToChildScene.Instance();
					fieldData.GetValue( "slotType", out rightType );
					rightColor = ServiceInjection<App>.Service.GetParentChildColor( rightType );
					break;
				case FieldType.SUB_GRAPH_LIST :
				case FieldType.SUB_GRAPH_DICTIONARY :
					child = _mainView.subGraphScene.Instance();
					break;
				case FieldType.FIELD_LIST : 
					child = _mainView.fieldListScene.Instance();
					break;
				case FieldType.FIELD_DICTIONARY : 
					child = _mainView.fieldDictionaryScene.Instance();
					break;
				case FieldType.INFO : 
					child = _mainView.infoScene.Instance();
					break;
				case FieldType.AUTO : 
					child = _mainView.autoScene.Instance();
					break;
				case FieldType.TYPE : 
					child = _mainView.typeScene.Instance();
					break;
				case FieldType.ENUM : 
					child = _mainView.enumScene.Instance();
					break;
				case FieldType.FLAGS : 
					child = _mainView.flagsScene.Instance();
					break;
				case FieldType.TEXT_LINE : 
					child = _mainView.textLineScene.Instance();
					break;
				case FieldType.TEXT_AREA : 
					child = _mainView.textAreaScene.Instance();
					break;
				case FieldType.TEXT_AREA_RICH : 
					child = _mainView.textAreaRichScene.Instance();
					break;
				case FieldType.FLOAT : 
					child = _mainView.floatScene.Instance();
					break;
				case FieldType.DOUBLE : 
					child = _mainView.doubleScene.Instance();
					break;
				case FieldType.INT : 
					child = _mainView.intScene.Instance();
					break;
				case FieldType.LONG : 
					child = _mainView.longScene.Instance();
					break;
				case FieldType.BOOLEAN : 
					child = _mainView.booleanScene.Instance();
					break;
				case FieldType.COLOR : 
					child = _mainView.colorScene.Instance();
					break;
				case FieldType.VECTOR2 : 
					child = _mainView.vector2Scene.Instance();
					break;
				case FieldType.VECTOR3 : 
					child = _mainView.vector3Scene.Instance();
					break;
				case FieldType.VECTOR4 : 
				case FieldType.QUATERNION : 
					child = _mainView.vector4Scene.Instance();
					break;
				case FieldType.DATE_TIME : 
					child = _mainView.dateTimeScene.Instance();
					break;
				case FieldType.DATE_TIME_OFFSET : 
					child = _mainView.dateTimeOffsetScene.Instance();
					break;
				case FieldType.TIME_SPAN : 
					child = _mainView.timeSpanScene.Instance();
					break;
				case FieldType.RELATIVE_PATH : 
					child = _mainView.relativePathScene.Instance();
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
				var connections = _mainView.GetConnectionsToNode( this );
				_mainView.BreakConnections( connections );
				
				AddChild(child);
				MoveChild(child, insertIndex);
				slots.Insert( insertIndex, child );
				ShiftSlotsDown( insertIndex );
				
				ShiftConnections( insertIndex, connections, false );
			}
			
			SetSlot( insertIndex, 
				true, leftType, leftColor, 
				true, rightType, rightColor );

			if( child is IField field )
			{
				field.Init( fieldData, parentOveride ?? _model );
			}

			return child;
		}

		public string GetPathToPort(int port)
		{
			return (GetSlot(port) as LinkToChildSlot).LocalAddress;
		}

		public new void RemoveChild( Node child )
		{
			int index = GetChildIndex( child );

			if( index < 0 ) return;
			
			var connections = _mainView.GetConnectionsToNode( this );
			_mainView.BreakConnections( connections );
			
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
			
			_mainView.RebuildConnections( connections );
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

		public void TrimDisconenctedData()
		{
			var oldData = new List<string>();
			var currentData = new List<string>();
			currentData.Add("ngMapNodeName");
			currentData.Add("ngMapNodePosition");
			currentData.Add("ngMapNodeSize");

			_definition.GetValue("fields", out List<GenericDataObject> fields);

			foreach(var gdo in fields)
			{
				var gdd = gdo as GenericDataDictionary;
				gdd.GetValue("label", out string label);
				if(string.IsNullOrEmpty(label)) continue;

				currentData.Add(label);
			}

			foreach(var pair in _model.values)
			{
				if(currentData.Contains(pair.Key)) continue;

				oldData.Add(pair.Key);
			}

			foreach(var item in oldData)
			{
				_model.RemoveValue(item);
			}
		}

		public void CloseRequest()
		{
			_mainView.FreeNode( this );
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