using System;
using System.Collections.Generic;
using Godot;
using MetaMakerLib;
using LibT;
using LibT.Maths;
using LibT.Serialization;
using LibT.Services;
using RSG;
using Color = Godot.Color;
using Vector2 = Godot.Vector2;

namespace MetaMaker
{
	public class MainView : ColorRect
	{
		#region Resources
		[Export] public PackedScene nodeScene;
		[Export] public PackedScene separatorScene;
		[Export] public PackedScene keyScene;
		[Export] public PackedScene keyLinkScene;
		[Export] public PackedScene linkToParentScene;
		[Export] public PackedScene linkToChildScene;
		[Export] public PackedScene subGraphScene;
		[Export] public PackedScene fieldListScene;
		[Export] public PackedScene fieldDictionaryScene;
		[Export] public PackedScene infoScene;
		[Export] public PackedScene autoScene;
		[Export] public PackedScene typeScene;
		[Export] public PackedScene enumScene;
		[Export] public PackedScene flagsScene;
		[Export] public PackedScene textLineScene;
		[Export] public PackedScene textAreaScene;
		[Export] public PackedScene textAreaRichScene;
		[Export] public PackedScene booleanScene;
		[Export] public PackedScene intScene;
		[Export] public PackedScene floatScene;
		[Export] public PackedScene doubleScene;
		[Export] public PackedScene longScene;
		[Export] public PackedScene colorScene;
		[Export] public PackedScene vector3Scene;
		[Export] public PackedScene vector4Scene;
		[Export] public PackedScene vector2Scene;
		[Export] public PackedScene dateTimeOffsetScene;
		[Export] public PackedScene dateTimeScene;
		[Export] public PackedScene timeSpanScene;
		[Export] public PackedScene relativePathScene;
		#endregion

		#region Variables
		[Export] public NodePath _graphPath;
		private GraphEdit _graph;
		[Export] public NodePath _fileMenuButtonPath;
		private MenuButton _fileMenuButton;
		[Export] public NodePath _editMenuButtonPath;
		private MenuButton _editMenuButton;
		[Export] public NodePath _settingsMenuButtonPath;
		private MenuButton _settingsMenuButton;
		[Export] public NodePath _navUpButtonPath;
		private Button _navUpButton;
		[Export] public NodePath _addressBarLabelPath;
		private Label _addressBar;
		[Export] public NodePath _searchBarPath;
		private LineEdit _searchBar;
		[Export] public NodePath _searchButtonPath;
		private Button _searchButton;
		[Export] public NodePath _backupTimerPath;
		private Timer _backupTimer;
		[Export] public NodePath _filePopupPath;
		private FileDialogExtended _filePopup;
		[Export] public NodePath _colorPopupPath;
		private Popup _colorPopup;
		private ColorPicker _colorPicker;
		[Export] public NodePath _errorPopupPath;
		private AcceptDialog _errorPopup;
		[Export] public NodePath _areYouSurePopupPath;
		private AreYouSurePopup _areYouSurePopup;
		[Export] public NodePath _helpInfoPopupPath;
		private HelpPopup _helpInfoPopup;
		[Export] public NodePath _settingsPath;
		private SettingsPopup _settingsPopup;

		public readonly List<Tuple<KeyLinkSlot, string>> loadingLinks = new List<Tuple<KeyLinkSlot, string>>();
		public readonly List<SlottedGraphNode> nodes = new List<SlottedGraphNode>();

		private readonly List<SlottedGraphNode> _selection = new List<SlottedGraphNode>();
		private readonly List<GenericDataDictionary> _copied = new List<GenericDataDictionary>();
		private Vector2 _copiedCorner = new Vector2(50,100);
		private Vector2 _offset = new Vector2(50,100);
		private Vector2 _buffer = new Vector2(40,10);
		private PopupMenu _editMenu;
		private PopupMenu _createSubmenu;
		private PopupMenu _exportSubmenu;
		private PopupMenu _recentTemplateSubmenu;
		private PopupMenu _recentShiftSubmenu;
		private PopupMenu _recentSubmenu;
		private Promise<Color> _pickingColor;
		private Tween _tween;
		private App _app;

		public int CurrentParentIndex {set; get;}

		
		public static bool ScrollLock
		{
			set
			{
				if(value)
				{
					_scrollLock++;
				}
				else
				{
					_scrollLock--;
				}
			}
			get => _scrollLock != 0;
		}
		private static int _scrollLock = 0;
		private Vector2 _scrollCurrent;
		
		public Color GridMajorColor
		{
			set
			{
				_graph.Set( "custom_colors/grid_major", value );
			}
			get => (Color)_graph.Get( "custom_colors/grid_major" );
		}
		
		public Color GridMinorColor
		{
			set
			{
				_graph.Set( "custom_colors/grid_minor", value );
			}
			get => (Color)_graph.Get( "custom_colors/grid_minor" );
		}

		public bool SettingAutoSave
		{
			set
			{
				if(_app.AutoBackup != value)
				{
					_app.AutoBackup = value;
				}
			}
			get => _app.AutoBackup;
		}

		public bool SettingGraphButtons
		{
			set
			{
				_graph.GetZoomHbox().Visible = value;
			}
			get => _graph.GetZoomHbox().Visible;
		}

		public FileDialogExtended FilePopup => _filePopup;
		#endregion

		#region Godot and signal connectors
		public override void _Ready()
		{
			try
			{
				ServiceProvider.Add( this );

				_errorPopup = this.GetNodeFromPath<AcceptDialog>( _errorPopupPath );
				_graph = this.GetNodeFromPath<GraphEdit>( _graphPath );
				_fileMenuButton = this.GetNodeFromPath<MenuButton>( _fileMenuButtonPath );
				_editMenuButton = this.GetNodeFromPath<MenuButton>( _editMenuButtonPath );
				_settingsMenuButton = this.GetNodeFromPath<MenuButton>( _settingsMenuButtonPath );
				_navUpButton = this.GetNodeFromPath<Button>( _navUpButtonPath );
				_addressBar = this.GetNodeFromPath<Label>( _addressBarLabelPath );
				_searchBar = this.GetNodeFromPath<LineEdit>( _searchBarPath );
				_searchButton = this.GetNodeFromPath<Button>( _searchButtonPath );
				_backupTimer = this.GetNodeFromPath<Timer>( _backupTimerPath );
				_filePopup = this.GetNodeFromPath<FileDialogExtended>( _filePopupPath );
				_areYouSurePopup = this.GetNodeFromPath<AreYouSurePopup>( _areYouSurePopupPath );
				_colorPopup = this.GetNodeFromPath<Popup>( _colorPopupPath );
				_colorPicker = _colorPopup.GetNode<ColorPicker>( "ColorPicker" );
				_helpInfoPopup = this.GetNodeFromPath<HelpPopup>( _helpInfoPopupPath );
				_settingsPopup = this.GetNodeFromPath<SettingsPopup>( _settingsPath );

				_recentTemplateSubmenu = new PopupMenu { Name = "RecentTemplateMenu" };
				_recentShiftSubmenu = new PopupMenu { Name = "RecentShiftMenu" };
				_recentSubmenu = new PopupMenu { Name = "RecentMenu" };
				_createSubmenu = new PopupMenu { Name = "CreateMenu" };
				_exportSubmenu = new PopupMenu { Name = "ExportMenu" };

				_recentTemplateSubmenu.Connect( "id_pressed", this, nameof(OnRecentTemplateMenuSelection));
				_recentShiftSubmenu.Connect( "id_pressed", this, nameof(OnRecentShiftMenuSelection));
				_recentSubmenu.Connect( "id_pressed", this, nameof(OnRecentMenuSelection));

				_errorPopup.AddButton("Copy",true,nameof(CopyErrorInfo));

				_errorPopup.Connect( "custom_action", this, nameof(ErrorCustomActions) );

				PopupMenu menu = _fileMenuButton.GetPopup();
				menu.Connect( "id_pressed", this, nameof(OnFileMenuSelection));
				menu.AddItem( "Build Template (CTRL+N)", 0 );
				menu.AddItem( "Load Template", 1 );
				menu.AddChild( _recentTemplateSubmenu );
				menu.AddSubmenuItem( "Recent Template", "RecentTemplateMenu" );
				menu.AddItem( "Shift Template Under Data", 7 );
				menu.AddChild( _recentShiftSubmenu );
				menu.AddSubmenuItem( "Recent Template Under Data", "RecentShiftMenu" );
				menu.AddSeparator(  );
				menu.AddItem( "Save Graph (CTRL+S)", 11 );
				menu.AddItem( "Save Graph As", 2 );
				menu.AddItem( "Load Graph", 3 );
				menu.AddChild( _recentSubmenu );
				menu.AddSubmenuItem( "Recent Graph", "RecentMenu" );
				menu.AddSeparator(  );
				menu.AddItem( "Clear Data", 6 );
				menu.AddItem( "Import Data from JSON", 5 );
				menu.AddItem( "Export All", 4 );
				_exportSubmenu.Connect( "id_pressed", this, nameof(OnExportMenuSelection));
				menu.AddChild(_exportSubmenu);
				menu.AddSubmenuItem( "Export", "ExportMenu" );
				menu.AddSeparator(  );
				menu.AddItem( "Quit", 8 );

				_createSubmenu.Connect( "id_pressed", this, nameof(OnCreateMenuSelection));

				_editMenu = _editMenuButton.GetPopup();
				_editMenu.Connect( "id_pressed", this, nameof(OnEditMenuSelection));
				_editMenu.AddChild( _createSubmenu );
				_editMenu.AddSubmenuItem( "Create", "CreateMenu" );
				_editMenu.AddItem( "Copy Nodes (CTRL+C)", 0 );
				_editMenu.AddItem( "Paste Nodes (CTRL+V)", 1 );
				_editMenu.AddItem( "Delete Nodes (DEL)", 2 );
				
				PopupMenu settingsMenu = _settingsMenuButton.GetPopup();
				settingsMenu.Connect( "id_pressed", this, nameof(OnSettingsMenuSelection));
				settingsMenu.AddItem( "Editor Settings...", 4 );
				settingsMenu.AddSeparator(  );
				settingsMenu.AddItem( "Help and FAQ", 3 );

				_navUpButton.Connect( "pressed", this, nameof(OnNavUpPress) );

				_searchBar.Connect( "text_entered", this, nameof(OnSearch) );
				_searchButton.Connect( "pressed", this, nameof(OnSearchPress) );
				
				_colorPopup.Connect( "popup_hide", this, nameof(SelectColor) );

				
				_graph.Connect( "connection_request", this, nameof(OnConnectionRequest) );
				_graph.Connect( "disconnection_request", this, nameof(OnDisconnectionRequest) );
				_graph.Connect( "node_selected", this, nameof(SelectNode) );
				_graph.Connect( "node_unselected", this, nameof(DeselectNode) );
				_graph.Connect( "copy_nodes_request", this, nameof(RequestCopyNode) );
				_graph.Connect( "paste_nodes_request", this, nameof(RequestPasteNode) );
				_graph.Connect( "delete_nodes_request", this, nameof(RequestDeleteNode) );
				_graph.Connect( "gui_input", this, nameof(GraphClick) );
				_graph.Connect( "scroll_offset_changed", this, nameof(GraphOnScroll) );
								
				_tween = new Tween();
				AddChild( _tween );
				
				GetTree().SetAutoAcceptQuit(false);
				
				_app = new App(this);
				_app.Init();

				_backupTimer.WaitTime = _app.BackupFrequency;
				_backupTimer.Connect( "timeout", this, nameof(SaveBackup) );
				_backupTimer.Start();

				_helpInfoPopup.Version = _app.Version;
				GenericDataDictionary helpData = new GenericDataDictionary();
				helpData.FromJson( _app.LoadJsonFile( App.HELP_INFO_SOURCE ) );
				_helpInfoPopup.SetObjectData( helpData );
			}
			catch( Exception e )
			{
				//throw e;
				_app.CatchException( e );
			}
		}
		
		public override void _Notification(int what)
		{
			if( what == MainLoop.NotificationWmQuitRequest )
			{
				RequestQuit();
			}
		}

		public override void _Input( InputEvent @event )
		{
			try
			{
				if( @event is InputEventKey eventKey )
				{
					if(!eventKey.IsPressed())
					{
						if( eventKey.Control || eventKey.Command )
						{
							if( OS.GetScancodeString( eventKey.Scancode ).Equals( "S" ) )
							{
								_app.SaveGraph();
							}
							else if( OS.GetScancodeString( eventKey.Scancode ).Equals( "N" ) )
							{
								_app.LoadDefaultTemplate();
							}
							else if( OS.GetScancodeString( eventKey.Scancode ).Equals( "P" ) )
							{
								SortNodes();
							}
						}
					}
				}
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}

		public void OnRecentTemplateMenuSelection( int id )
		{
			_app.OnRecentTemplateMenuSelection(id);
		}

		public void OnRecentShiftMenuSelection( int id )
		{
			_app.OnRecentShiftMenuSelection(id);
		}

		public void OnRecentMenuSelection( int id )
		{
			_app.OnRecentMenuSelection(id);
		}

		public void SaveBackup()
		{
			_app.SaveBackup();
		}
		private void OnFileMenuSelection( int id )
		{
			try
			{
				switch(id)
				{
					case 0 :  _app.LoadDefaultTemplate(); break;
					case 1 :  PickTemplateToLoad(); break;
					case 2 :  SaveGraphAs(); break;
					case 3 :  LoadGraph(); break;
					case 4 :  _app.ExportAll(); break;
					case 5 :  ImportDataJson(); break;
					case 6 :  _app.ClearData(); break;
					case 7 :  ShiftTemplateUnderData(); break;
					case 8 :  RequestQuit(); break;
					case 11 :  _app.SaveGraph(); break;
					case 12 :  OpenHelpPopup(); break;
				}
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}

		private void OnEditMenuSelection( int id )
		{
			try
			{
				switch(id)
				{
					case 0 :  RequestCopyNode(); break;
					case 1 :  RequestPasteNode(); break;
					case 2 :  RequestDeleteNode(); break;
				}
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}
		
		private void OnExportMenuSelection( int id )
		{
			try
			{
				_app.ExportSet(_exportSubmenu.GetItemText( id ));
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}
		
		private void OnCreateMenuSelection( int id )
		{
			try
			{
				CreateNode( _app.GetNodeData(_createSubmenu.GetItemText( id )) );
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}
		
		private void OnSettingsMenuSelection( int id )
		{
			try
			{
				switch(id)
				{
					case 3 :  OpenHelpPopup(); break;
					case 4 :  _settingsPopup.PopupCentered(); break;
				}
			}
			catch( Exception e )
			{
				_app.CatchException( e );
			}
		}

		private void OnSearchPress()
		{
			OnSearch(_searchBar.Text);
		}

		private void OnSearch( string text )
		{
			if(string.IsNullOrEmpty(text)) return;

			if( !_app.generatedKeys.ContainsKey( text ) )
			{
				string partial = PartialFindKey( text );
				
				if( string.IsNullOrEmpty( partial ) ) return;
				
				CenterViewOnKeyedNode( partial );
				
				return;
			}
			
			CenterViewOnKeyedNode( text );
		}

		private void GraphOnScroll(Vector2 offset)
		{
			if(ScrollLock)
			{
				_graph.ScrollOffset = _scrollCurrent;
			}
			else
			{
				_scrollCurrent = offset;
			}
		}

		private void GraphClick( InputEvent @event )
		{
			if( @event is InputEventMouseButton eventMouse )
			{
				if(!eventMouse.IsPressed())
				{
					_offset = _graph.ScrollOffset + eventMouse.Position;
					
					if( eventMouse.ButtonIndex == 2 )
					{
						_editMenu.Popup_(  );
						_editMenu.RectPosition = eventMouse.Position;
					}
				}
			}
		}

		private void PickTemplateToLoad()
		{
			_filePopup.Show( new[] { "*.tmplt" }, false, "Open a Template file" )
				.Then( path =>
				{
					_app.LoadTemplate( _app.LoadJsonFile( path ) );

					_app.AddRecentTemplateFile( path );
					_app.HasUnsavedChanges = false;
					_app.SaveFilePath = null;
				} )
				.Catch( _app.CatchException );
		}

		public IPromise SaveGraphAs()
		{
			IPromise promise =
			_filePopup.Show( new[] { "*.ngmap" }, true, "Save Full Graph" )
				.Then( _app.SaveGraph );
			
			promise.Catch( _app.CatchException );
			
			return promise;
		}

		private void LoadGraph()
		{
			_filePopup.Show( new[] { "*.ngmap" }, false, "Load Full Graph" )
				.Then( _app.LoadGraph )
				.Catch( _app.CatchException );
		}

		private void ShiftTemplateUnderData()
		{
			_filePopup.Show( new[] { "*.tmplt" }, false, "Select Template File for Use" )
				.Then( _app.ShiftTemplateUnderData )
				.Catch( _app.CatchException );
		}
		
		private void ImportDataJson()
		{
			_filePopup.Show( new[] { "*.json" }, false, "Load Data from JSON" )
				.Then( path =>
				{
					_app.ClearData();

					string json = _app.LoadJsonFile( path );
					var data = new GenericDataDictionary();
					data.FromJson( json );
					_app.LoadData( data );

					this.DelayedInvoke( 0.5f, SortNodes );
				} )
				.Catch( _app.CatchException );
		}

		private void RequestQuit()
		{
			_app.SaveSettings();
			_app.CheckIfSavedFirst()
				.Then( ()=> GetTree().Quit() );
		}

		private void OpenHelpPopup()
		{
			_helpInfoPopup.PopupCentered();
		}

		private void RequestCopyNode()
		{
			try
			{
				if(_selection.Count == 0) return;
				
				_copied.Clear();

				_copiedCorner = _selection[0].Offset;

				foreach( SlottedGraphNode node in _selection )
				{
					_copiedCorner.x = Maths.Min( node.Offset.x, _copiedCorner.x );
					_copiedCorner.y = Maths.Min( node.Offset.y, _copiedCorner.y );

					_copied.Add( node.Model.DataCopy() );
				}
				
				foreach( GenericDataDictionary node in _copied )
				{
					TrimToSelection(node);
				}
				List<GenericDataDictionary> toRemove = new List<GenericDataDictionary>();
				foreach( GenericDataDictionary node in _copied )
				{
					FindDuplicates(node, toRemove);
				}
				
				foreach (GenericDataDictionary item in toRemove)
				{
					_copied.Remove(item);
				}
			}
			catch(Exception ex)
			{
				_app.CatchException(ex);
			}
		}

		private void TrimToSelection( GenericDataDictionary target )
		{
			List<string> removeKeys = new List<string>();

			foreach (var pair in target.values)
			{
				if(pair.Value is GenericDataDictionary subTarget)
				{
					if(subTarget.values.ContainsKey(App.NODE_NAME_KEY))
					{
						bool isSelected = false;
						foreach(SlottedGraphNode selected in _selection)
						{
							if(subTarget.DeepEquals(selected.Model))
							{
								isSelected = true;
							}
						}
						if(isSelected)
						{
							TrimToSelection(subTarget);
						}
						else
						{
							removeKeys.Add(pair.Key);
						}
					}
					else
					{
						TrimToSelection( subTarget );
					}
				}
			}

			foreach(var key in removeKeys)
			{
				target.RemoveValue(key);
			}
		}

		private void FindDuplicates( GenericDataDictionary target, List<GenericDataDictionary> toRemove )
		{
			foreach (var pair in target.values)
			{
				if(pair.Value is GenericDataDictionary subTarget)
				{
					if(subTarget.values.ContainsKey(App.NODE_NAME_KEY))
					{
						for (int i = _copied.Count-1; i >= 0; i--)
						{
							GenericDataDictionary copy = _copied[i];
							if (!subTarget.DeepEquals(copy) || toRemove.Contains(copy)) continue;

							toRemove.Add(copy);
						}
					}

					FindDuplicates( subTarget, toRemove );
				}
			}
		}

		private void RequestPasteNode()
		{
			if( _copied.Count == 0 ) return;

			_app.pastingData = true;
			int start = nodes.Count;
			Vector2 startOffset = _offset - _copiedCorner;
			
			for( int i = 0; i < _copied.Count; i++ )
			{
				_app.LoadNode(  _copied[i].DataCopy() );
			}

			for( int i = start; i < nodes.Count; i++ )
			{
				nodes[i].Offset += startOffset;
			}
			_app.pastingData = false;
		}

		private void RequestDeleteNode()
		{
			for( int i = _selection.Count-1; i >= 0; i-- )
			{
				if(_selection[i] == null) return;
				
				_selection[i].CloseRequest();
			}
			
			_selection.Clear();
			
			_app.HasUnsavedChanges = true;
		}

		private void SelectNode(Node node)
		{
			SlottedGraphNode graphNode = node as SlottedGraphNode;
			
			if( _selection.Contains( graphNode ) ) return;
			
			_selection.Add( graphNode );
			UpdateAddress(graphNode);
		}

		private void DeselectNode(Node node)
		{
			SlottedGraphNode graphNode = node as SlottedGraphNode;
			
			if( !_selection.Contains( graphNode ) ) return;
			
			_selection.Remove( graphNode );
		}

		private void OnNavUpPress()
		{
			
		}

		private void UpdateAddress(SlottedGraphNode graphNode)
		{
			_addressBar.Text = RecurseAddress(graphNode);
		}

		private string RecurseAddress(SlottedGraphNode node)
		{
			if(node.ParentType >= 0)
			{
				Godot.Collections.Array fullList = _graph.GetConnectionList();
				
				foreach( Godot.Collections.Dictionary connection in fullList )
				{
					if( Equals( connection["to"], node.Name ) && Equals( connection["to_port"], 0 ) )
					{
						SlottedGraphNode parent = _graph.GetNode<SlottedGraphNode>( (string)connection["from"] );
						string before = RecurseAddress(parent);

						if(string.IsNullOrEmpty(before)) return null;
						
						return before + parent.GetPathToPort((int)connection["from_port"]) + "/";
					}
				}
			}
			else
			{
				return "/";
			}

			return null;
		}
		#endregion

		#region Helper Functions
		public void ResetCreateMenu(List<string> nodeNames)
		{
			_createSubmenu.Clear();
			_createSubmenu.RectSize = Vector2.Zero;

			foreach( string name in nodeNames )
			{
				_createSubmenu.AddItem( name );
			}
		}

		public void ResetExportMenu(Dictionary<string, ExportSet> exports)
		{
			_exportSubmenu.Clear();
			_exportSubmenu.RectSize = Vector2.Zero;

			foreach( var export in exports )
			{
				_exportSubmenu.AddItem( export.Key );
			}
		}

		public void UpdateBackupTimer()
		{
			_backupTimer.WaitTime = _app.BackupFrequency;
		}

		public void DisplayAreYouSurePopup(AreYouSurePopup.AreYouSureArgs args)
		{
			_areYouSurePopup.Display(args);
		}

		public SlottedGraphNode CreateNode(GenericDataDictionary nodeData, GenericDataDictionary nodeContent = null)
		{
			if( nodeData == null ) throw new ArgumentNullException("CreateNode passed NULL nodeData");

			
			Node child = nodeScene.Instance();
			_graph.AddChild(child);
			if (!(child is SlottedGraphNode node)) throw new NullReferenceException("nodeScene is not a SlottedGraphNode");

			node.Offset = _offset;
			
			nodes.Add( node );

			node.SetSlots( nodeData, nodeContent );
			_app.HasUnsavedChanges = true;

			_app.AddNode(node);
			
			return node;
		}
		
		public void DisplayErrorPopup( Exception ex )
		{
			_errorPopup.DialogText = ex.Message;
			
			#if DEBUG
				_errorPopup.DialogText = $"{ex.Message} \n\n{ex.StackTrace}";
			#endif
			
			_errorPopup.PopupCentered();
		}
		
		
		public void ShowNotice( string notice )
		{
			Log.LogL( notice );
			_errorPopup.DialogText = notice;
			_errorPopup.PopupCentered();
		}

		public void ErrorCustomActions(string action)
		{
			OS.Clipboard = _errorPopup.DialogText;
		}

		public void CopyErrorInfo()
		{
		}
		
		public void UpdateRecentMenu( List<string> recents )
		{
			_recentSubmenu.Clear();

			foreach( string file in recents )
			{
				_recentSubmenu.AddItem( file );
			}

			if( recents.Count > 0 )
			{
				_filePopup.CurrentDir = recents[0].Substring( 0, recents[0].LastIndexOf( '/' ));
			}
		}
		
		public void UpdateRecentTemplateMenu( List<string> recents )
		{
			_recentTemplateSubmenu.Clear();
			_recentShiftSubmenu.Clear();

			foreach( string file in recents )
			{
				_recentTemplateSubmenu.AddItem( file );
				_recentShiftSubmenu.AddItem( file );
			}
		}

		private void SortNodes()
		{
			Vector2 currentPos = Vector2.Zero;
			
			foreach( SlottedGraphNode node in nodes )
			{
				if( !node.LinkedToParent )
				{
					Vector2 size = SortChild( node, currentPos );
					currentPos += new Vector2(0, size.y);
				}
			}
		}

		private Vector2 SortChild( SlottedGraphNode node, Vector2 corner )
		{
			node.Offset = corner;
			Vector2 sizeMin = node.RectSize + _buffer;
			corner += new Vector2( sizeMin.x, 0 );
			float x = 0;
			float y = 0;

			foreach( SlottedGraphNode child in node.children )
			{
				Vector2 size = SortChild( child, new Vector2( 0, y ) + corner );
				y += size.y + _buffer.y;
				if( size.x > x )
				{
					x = size.x;
				}
			}

			if( y < sizeMin.y ) y = sizeMin.y;
			
			return new Vector2( x + _buffer.x, y );
		}

		public IPromise<Color> GetColorFromUser(Color original)
		{
			_colorPicker.Color = original;
			_colorPopup.Popup_();
			_colorPopup.RectPosition = GetGlobalMousePosition();

			_pickingColor = new Promise<Color>();
			return _pickingColor;
		}

		private void SelectColor()
		{
			if( _pickingColor )
			{
				_pickingColor.Resolve( _colorPicker.Color );
				_pickingColor = null;
			}
		}

		public void CenterViewOnKeyedNode(string key)
		{
			CenterViewOnNode(_app.generatedKeys[key].GetParent<SlottedGraphNode>());
		}
		
		public void CenterViewOnNode(SlottedGraphNode node)
		{
			CenterView( node.Offset + (node.RectSize * 0.5f));
		}

		public void CenterView( Vector2 position )
		{
			_tween.Remove( _graph, "scroll_offset" );
			_tween.InterpolateProperty( _graph,
				"scroll_offset",
				_graph.ScrollOffset,
				(position * _graph.Zoom) - (_graph.RectSize * 0.5f),
				0.3f );
			_tween.Start();
		}

		private string PartialFindKey( string partial )
		{
			if( string.IsNullOrEmpty( partial ) ) return null;
			
			foreach( string key in _app.generatedKeys.Keys )
			{
				if( key.Contains( partial ) ) return key;
			}
			
			return null;
		}

		public void SetNodesClean()
		{
			foreach (var node in nodes)
			{
				node.Dirty = false;
			}
		}

		public void ClearScrollLock()
		{
			_scrollLock = 0;
		}
		#endregion

		#region Connections
		public void OnConnectionRequest( string from, int fromSlot, string to, int toSlot)
		{
			Node leftSlot = _graph.GetNode<SlottedGraphNode>( from ).GetSlot( fromSlot );
			var rightGraph = _graph.GetNode<SlottedGraphNode>( to );
			Node rightSlot = rightGraph.GetSlot( toSlot );

			if( leftSlot is KeyLinkSlot keyLink )
			{
				if( !( rightSlot is KeySlot key ) )
				{
					throw new Exception($"{leftSlot.Name} is not a KeyLinkSlot");
				}
				if( !keyLink.AddKey( key.GetKey ) ) return;

				_graph.ConnectNode( @from, fromSlot, to, toSlot );
			}
			else if( leftSlot is LinkToChildSlot leftLink )
			{
				if( !( rightSlot is LinkToParentSlot rightLink) ) return;
				if( !leftLink.LinkChild( rightGraph ) ) return;

				rightLink.Link(leftLink);
				_graph.ConnectNode( @from, fromSlot, to, toSlot );
			}
		}
		
		private void OnDisconnectionRequest( string from, int fromSlot, string to, int toSlot)
		{
			Node leftSlot = _graph.GetNode<SlottedGraphNode>( from ).GetSlot( fromSlot );
			var rightGraph = _graph.GetNode<SlottedGraphNode>( to );
			Node rightSlot = rightGraph.GetSlot( toSlot );

			if( leftSlot is KeyLinkSlot keyLink )
			{
				if( !( rightSlot is KeySlot key ) )
				{
					throw new Exception($"{leftSlot.Name} is not a KeyLinkSlot");
				}
				if( !keyLink.RemoveKey( key.GetKey ) ) return;

				_graph.DisconnectNode( from, fromSlot, to, toSlot );
			}
			else if( leftSlot is LinkToChildSlot leftLink )
			{
				if( !( rightSlot is LinkToParentSlot rightLink) || !rightLink.IsLinked ) return;
				if( !leftLink.UnLinkChild( rightGraph ) ) return;

				rightLink.Unlink();
				_graph.DisconnectNode( @from, fromSlot, to, toSlot );
			}
		}

		public void FreeNode( SlottedGraphNode graphNode )
		{
			if( _selection.Contains( graphNode ) )
			{
				_selection.Remove( graphNode );
			}
			
			Godot.Collections.Array connections = _graph.GetConnectionList();
			foreach( Godot.Collections.Dictionary connection in connections )
			{
				if( Equals( connection["from"], graphNode.Name ) || 
					Equals( connection["to"], graphNode.Name ) )
				{
					OnDisconnectionRequest( (string)connection["from"],
						(int)connection["from_port"],
						(string)connection["to"],
						(int)connection["to_port"] );
				}
			}

			nodes.Remove( graphNode );
			_app.RemoveNode(graphNode);
		}
		
		public List<GraphConnection> GetConnectionsToNode(SlottedGraphNode graphNode)
		{
			Godot.Collections.Array fullList = _graph.GetConnectionList();
			List<GraphConnection> connections = new List<GraphConnection>();
			
			foreach( Godot.Collections.Dictionary connection in fullList )
			{
				if( !Equals( connection["from"], graphNode.Name ) && 
					!Equals( connection["to"], graphNode.Name ) )
				{
					continue;
				}

				var link = new GraphConnection
				{
					from = (string)connection["from"],
					fromPort = (int)connection["from_port"],
					to = (string)connection["to"],
					toPort = (int)connection["to_port"]
				};
				
				link.goingRight = Equals( link.from, graphNode.Name );
				connections.Add( link );
			}

			return connections;
		}

		public void BreakConnections(List<GraphConnection> connections)
		{
			foreach( GraphConnection connection in connections )
			{
				OnDisconnectionRequest( connection.from, connection.fromPort, connection.to, connection.toPort );
			}
		}

		public void RebuildConnections(List<GraphConnection> connections)
		{
			foreach( GraphConnection connection in connections )
			{
				OnConnectionRequest( connection.from, connection.fromPort, connection.to, connection.toPort );
			}
		}

		public void LinkToKey( KeyLinkSlot link, string key )
		{
			if( !_app.generatedKeys.ContainsKey( key ) )
			{
				throw new ArgumentException("Attempting link to key not found",nameof(key));
			}
			
			SlottedGraphNode linkParent = link.GetParent<SlottedGraphNode>();
			int fromSlot = linkParent.GetChildIndex( link );

			if( fromSlot < 0 )
			{
				throw new ArgumentException($"{link.Name} not found in {linkParent.Name}",nameof(link));
			}
			
			SlottedGraphNode keyParent = _app.generatedKeys[key].GetParent<SlottedGraphNode>();
			int toSlot = keyParent.GetChildIndex( _app.generatedKeys[key] );
			
			if( toSlot < 0 )
			{
				throw new ArgumentException($"{_app.generatedKeys[key].Name} not found in {keyParent.Name}",nameof(link));
			}
			
			OnConnectionRequest( linkParent.Name, fromSlot ,keyParent.Name, toSlot );
		}
		#endregion
	}
}