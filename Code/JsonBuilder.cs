using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;
using RSG;
using RSG.Exceptions;
using File = Godot.File;

public class JsonBuilder : ColorRect
{
	#region Resources
	private const string SETTINGS_SOURCE = "user://Settings.json";
	
	private const string DEFAULT_TEMPLATE_SOURCE = "res://Resources/TEMPLATE.tmplt";
	private const string HELP_INFO_SOURCE = "res://Resources/HelpInfo.json";
	private const string VERSION = "res://Resources/Version.txt";

	[Export] public PackedScene nodeScene;
	[Export] public PackedScene separatorScene;
	[Export] public PackedScene keyScene;
	[Export] public PackedScene keyLinkScene;
	[Export] public PackedScene linkToParentScene;
	[Export] public PackedScene linkToChildScene;
	[Export] public PackedScene fieldListScene;
	[Export] public PackedScene fieldDictionaryScene;
	[Export] public PackedScene validateStringScene;
	[Export] public PackedScene infoScene;
	[Export] public PackedScene autoScene;
	[Export] public PackedScene typeScene;
	[Export] public PackedScene enumScene;
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
	#endregion

	#region Variables
	[Export] private NodePath _graphPath;
	private GraphEdit _graph;
	[Export] private NodePath _zoomOutPath;
	private Button _zoomOutButton;
	[Export] private NodePath _zoomNormalPath;
	private Button _zoomNormalButton;
	[Export] private NodePath _zoomInPath;
	private Button _zoomInButton;
	[Export] private NodePath _snapPath;
	private Button _snapButton;
	[Export] private NodePath _snapSizePath;
	private SpinBox _snapSizeField;
	[Export] private NodePath _fileMenuButtonPath;
	private MenuButton _fileMenuButton;
	[Export] private NodePath _editMenuButtonPath;
	private MenuButton _editMenuButton;
	[Export] private NodePath _settingsMenuButtonPath;
	private MenuButton _settingsMenuButton;
	[Export] private NodePath _searchBarPath;
	private LineEdit _searchBar;
	[Export] private NodePath _searchButtonPath;
	private Button _searchButton;
	[Export] private NodePath _filePopupPath;
	private FileDialogExtended _filePopup;
	[Export] private NodePath _colorPopupPath;
	private Popup _colorPopup;
	private ColorPicker _colorPicker;
	[Export] private NodePath _errorPopupPath;
	private AcceptDialog _errorPopup;
	[Export] private NodePath _areYouSurePopupPath;
	private AreYouSurePopup _areYouSurePopup;
	[Export] private NodePath _helpInfoPopupPath;
	private HelpPopup _helpInfoPopup;

	public Dictionary<string, KeySlot> generatedKeys = new Dictionary<string, KeySlot>();
	public readonly List<Tuple<KeyLinkSlot, string>> _loadingLinks = new List<Tuple<KeyLinkSlot, string>>();
	public bool includeNodeData = true;
	public bool includeGraphData = true;
	public bool pastingData = false;
	
	private Vector2 _offset = new Vector2(50,100);
	private Vector2 _buffer = new Vector2(40,10);
	private List<string> _recentTemplates = new List<string>();
	private List<string> _recentFiles = new List<string>();
	private Dictionary<string,GenericDataArray> _nodeData = new Dictionary<string, GenericDataArray>();
	private List<SlottedGraphNode> _nodes = new List<SlottedGraphNode>();
	private SlottedGraphNode _lastSelection;
	private GenericDataArray _copied;
	private PopupMenu _editMenu;
	private PopupMenu _createSubmenu;
	private PopupMenu _recentTemplateSubmenu;
	private PopupMenu _recentShiftSubmenu;
	private PopupMenu _recentSubmenu;
	private string _defaultListing;
	private string _explicitNode;
	private Promise<Color> _pickingColor;
	private Tween _tween;
	private string _version;
	
	private string SaveFilePath
	{
		set
		{
			_saveFilePath = value;
			UpdateTitle();
		}
		get => _saveFilePath;
	}
	private string _saveFilePath;

	private List<Color> _parentChildColors = new List<Color> ();
	private List<Color> _keyColors = new List<Color>();


	public bool HasUnsavedChanges
	{
		set
		{
			_hasUnsavedChanges = value;
			UpdateTitle();
		}
		get => _hasUnsavedChanges;
	}
	private bool _hasUnsavedChanges;
	#endregion

	#region Setup
	public override void _Ready()
	{
		OS.LowProcessorUsageMode = true;
		
		try
		{
			ServiceProvider.Add( this );

			_recentTemplateSubmenu = new PopupMenu();
			_recentTemplateSubmenu.Name = "RecentTemplateMenu";
			_recentTemplateSubmenu.Connect( "id_pressed", this, nameof(OnRecentTemplateMenuSelection));
			
			_recentShiftSubmenu = new PopupMenu();
			_recentShiftSubmenu.Name = "RecentShiftMenu";
			_recentShiftSubmenu.Connect( "id_pressed", this, nameof(OnRecentShiftMenuSelection));
			
			_recentSubmenu = new PopupMenu();
			_recentSubmenu.Name = "RecentMenu";
			_recentSubmenu.Connect( "id_pressed", this, nameof(OnRecentMenuSelection));

			_fileMenuButton = this.GetNodeFromPath<MenuButton>( _fileMenuButtonPath );
			PopupMenu menu = _fileMenuButton.GetPopup();
			menu.Connect( "id_pressed", this, nameof(OnFileMenuSelection));
			menu.AddItem( "Build Template (CTRL+N)", 0 );
			menu.AddItem( "Load Template", 1 );
			menu.AddChild( _recentTemplateSubmenu );
			menu.AddSubmenuItem( "Recent Template", "RecentTemplateMenu" );
			menu.AddItem( "Shift Template Under Data", 7 );
			menu.AddChild( _recentShiftSubmenu );
			menu.AddSubmenuItem( "Recent Template Under Data", "RecentShiftMenu" );
			menu.AddItem( "Export as Template", 9 );
			menu.AddSeparator(  );
			menu.AddItem( "Save Graph (CTRL+S)", 11 );
			menu.AddItem( "Save Graph As", 2 );
			menu.AddItem( "Load Graph", 3 );
			menu.AddChild( _recentSubmenu );
			menu.AddSubmenuItem( "Recent Graph", "RecentMenu" );
			menu.AddSeparator(  );
			menu.AddItem( "Clear Data", 6 );
			menu.AddItem( "Export Slim JSON", 10 );
			menu.AddItem( "Export Data to JSON", 4 );
			menu.AddItem( "Import Data from JSON", 5 );
			menu.AddSeparator(  );
			menu.AddItem( "Quit", 8 );
			
			_createSubmenu = new PopupMenu();
			_createSubmenu.Name = "CreateMenu";
			_createSubmenu.Connect( "id_pressed", this, nameof(OnCreateMenuSelection));

			_editMenuButton = this.GetNodeFromPath<MenuButton>( _editMenuButtonPath );
			_editMenu = _editMenuButton.GetPopup();
			_editMenu.Connect( "id_pressed", this, nameof(OnEditMenuSelection));
			_editMenu.AddChild( _createSubmenu );
			_editMenu.AddSubmenuItem( "Create", "CreateMenu" );
			_editMenu.AddItem( "Copy Downstream (CTRL+C)", 0 );
			_editMenu.AddItem( "Paste Nodes (CTRL+V)", 1 );
			_editMenu.AddItem( "Delete Node (DEL)", 2 );
			
			_settingsMenuButton = this.GetNodeFromPath<MenuButton>( _settingsMenuButtonPath );
			PopupMenu settingsMenu = _settingsMenuButton.GetPopup();
			settingsMenu.Connect( "id_pressed", this, nameof(OnSettingsMenuSelection));
			settingsMenu.AddItem( "Change Background Color", 0 );
			settingsMenu.AddItem( "Change Grid Major Color", 1 );
			settingsMenu.AddItem( "Change Grid Minor Color", 2 );
			settingsMenu.AddSeparator(  );
			settingsMenu.AddItem( "Help and FAQ", 3 );
			
			_searchBar = this.GetNodeFromPath<LineEdit>( _searchBarPath );
			_searchBar.Connect( "text_entered", this, nameof(OnSearch) );
			_searchButton = this.GetNodeFromPath<Button>( _searchButtonPath );
			_searchButton.Connect( "pressed", this, nameof(OnSearchPress) );

			_filePopup = this.GetNodeFromPath<FileDialogExtended>( _filePopupPath );
			_errorPopup = this.GetNodeFromPath<AcceptDialog>( _errorPopupPath );
			_areYouSurePopup = this.GetNodeFromPath<AreYouSurePopup>( _areYouSurePopupPath );
			
			_colorPopup = this.GetNodeFromPath<Popup>( _colorPopupPath );
			_colorPicker = _colorPopup.GetNode<ColorPicker>( "ColorPicker" );
			_colorPopup.Connect( "popup_hide", this, nameof(SelectColor) );

			_version = LoadJsonFile( VERSION );
			_helpInfoPopup = this.GetNodeFromPath<HelpPopup>( _helpInfoPopupPath );
			_helpInfoPopup.Version = _version;
			GenericDataArray helpData = new GenericDataArray();
			helpData.FromJson( LoadJsonFile( HELP_INFO_SOURCE ) );
			_helpInfoPopup.LoadFromGda( helpData );
			
			_graph = this.GetNodeFromPath<GraphEdit>( _graphPath );
			_graph.Connect( "connection_request", this, nameof(OnConnectionRequest) );
			_graph.Connect( "disconnection_request", this, nameof(OnDisconnectionRequest) );
			_graph.Connect( "node_selected", this, nameof(SelectNode) );
			_graph.Connect( "node_unselected", this, nameof(DeselectNode) );
			_graph.Connect( "copy_nodes_request", this, nameof(RequestCopyNode) );
			_graph.Connect( "paste_nodes_request", this, nameof(RequestPasteNode) );
			_graph.Connect( "delete_nodes_request", this, nameof(RequestDeleteNode) );
			_graph.Connect( "gui_input", this, nameof(GraphClick) );
			_graph.GetZoomHbox().Visible = false;
			
			_zoomOutButton = this.GetNodeFromPath<Button>( _zoomOutPath );
			_zoomOutButton.Connect( "pressed", this, nameof(ZoomOutPress) );
			_zoomNormalButton = this.GetNodeFromPath<Button>( _zoomNormalPath );
			_zoomNormalButton.Connect( "pressed", this, nameof(ZoomNormalPress) );
			_zoomInButton = this.GetNodeFromPath<Button>( _zoomInPath );
			_zoomInButton.Connect( "pressed", this, nameof(ZoomInPress) );
			_snapButton = this.GetNodeFromPath<Button>( _snapPath );
			_snapButton.Connect( "pressed", this, nameof(SnapPress) );
			_snapSizeField = this.GetNodeFromPath<SpinBox>( _snapSizePath );
			_snapSizeField.Connect( "changed", this, nameof(SnapChanged) );
			
			_tween = new Tween();
			AddChild( _tween );
			
			GetTree().SetAutoAcceptQuit(false);
			
			LoadSettings();
			LoadDefaultTemplate();
		}
		catch( Exception e )
		{
			CatchException( e );
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
							SaveGraph();
						}
						else if( OS.GetScancodeString( eventKey.Scancode ).Equals( "N" ) )
						{
							LoadDefaultTemplate();
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
			CatchException( e );
		}
	}

	private void LoadSettings()
	{
		File file = new File();
		if( !file.FileExists( SETTINGS_SOURCE ) )return;

		GenericDataArray gda = new GenericDataArray();
		gda.FromJson( LoadJsonFile( SETTINGS_SOURCE ) );
		
		gda.GetValue( "backgroundColor", out Color background );
		Color = background;
		gda.GetValue( "gridMajorColor", out Color gridMajor );
		_graph.Set( "custom_colors/grid_major", gridMajor );
		gda.GetValue( "gridMinorColor", out Color gridMinor );
		_graph.Set( "custom_colors/grid_minor", gridMinor );

		gda.GetValue( "RecentFiles", out _recentFiles );

		if( _recentFiles.Count > 0 )
		{
			for( int i = 0; i < _recentFiles.Count; i++ )
			{
				if( !file.FileExists( _recentFiles[i] ) )
				{
					_recentFiles.RemoveAt( i );
					i--;
				}
			}

			AddRecentFile( _recentFiles[0] );
		}
		
		gda.GetValue( "RecentTemplates", out _recentTemplates );

		if( _recentTemplates.Count <= 0 ) return;

		for( int i = 0; i < _recentTemplates.Count; i++ )
		{
			if( !file.FileExists( _recentTemplates[i] ) )
			{
				_recentTemplates.RemoveAt( i );
				i--;
			}
		}
		
		AddRecentTemplateFile( _recentTemplates[0] );
	}

	private void SaveSettings()
	{
		GenericDataArray gda = new GenericDataArray();
		gda.AddValue( "RecentFiles", _recentFiles );
		gda.AddValue( "RecentTemplates", _recentTemplates );
		gda.AddValue( "backgroundColor", Color );
		gda.AddValue( "gridMajorColor", (Color)_graph.Get( "custom_colors/grid_major" ) );
		gda.AddValue( "gridMinorColor", (Color)_graph.Get( "custom_colors/grid_minor" ) );
		
		SaveJsonFile( SETTINGS_SOURCE, gda.ToJson() );
	}
	#endregion

	#region Buttons
	private void OnFileMenuSelection( int id )
	{
		try
		{
			switch(id)
			{
				case 0 :  LoadDefaultTemplate(); break;
				case 1 :  PickTemplateToLoad(); break;
				case 2 :  SaveGraphAs(); break;
				case 3 :  LoadGraph(); break;
				case 4 :  ExportDataJson(); break;
				case 5 :  ImportDataJson(); break;
				case 6 :  ClearData(); break;
				case 7 :  ShiftTemplateUnderData(); break;
				case 8 :  RequestQuit(); break;
				case 9 :  ExportTemplateJson(); break;
				case 10 :  
					includeNodeData = false;
					ExportDataJson();
					break;
				case 11 :  SaveGraph(); break;
				case 12 :  OpenHelpPopup(); break;
			}
		}
		catch( Exception e )
		{
			CatchException( e );
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
			CatchException( e );
		}
	}
	
	private void OnCreateMenuSelection( int id )
	{
		try
		{
			CreateNode( _nodeData[_createSubmenu.GetItemText( id )] );
		}
		catch( Exception e )
		{
			CatchException( e );
		}
	}
	
	private void OnSettingsMenuSelection( int id )
	{
		try
		{
			switch(id)
			{
				case 0 :  
					GetColorFromUser( Color )
						.Then( color =>
						{
							Color = color;
							SaveSettings();
						}); 
						break;
				case 1 :  
					GetColorFromUser( Color )
						.Then( color =>
						{
							_graph.Set( "custom_colors/grid_major", color );
							SaveSettings();
						}); 
					break;
				case 2 :  
					GetColorFromUser( Color )
						.Then( color =>
						{
							_graph.Set( "custom_colors/grid_minor", color );
							SaveSettings();
						}); 
					break;
				case 3 :  OpenHelpPopup(); break;
			}
		}
		catch( Exception e )
		{
			CatchException( e );
		}
	}

	private void OnSearchPress()
	{
		OnSearch(_searchBar.Text);
	}

	private void OnSearch( string text )
	{
		if(string.IsNullOrEmpty(text)) return;

		if( !generatedKeys.ContainsKey( text ) )
		{
			string partial = PartialFindKey( text );
			
			if( string.IsNullOrEmpty( partial ) ) return;
			
			CenterViewOnKeyedNode( partial );
			
			return;
		}
		
		CenterViewOnKeyedNode( text );
	}

	private void ZoomOutPress()
	{
		_graph.Zoom -= 0.1f;
	}

	private void ZoomInPress()
	{
		_graph.Zoom += 0.1f;
	}

	private void ZoomNormalPress()
	{
		_graph.Zoom = 1;
	}

	private void SnapPress()
	{
		_graph.UseSnap = !_graph.UseSnap;

		_snapSizeField.Visible = _graph.UseSnap;
	}

	private void SnapChanged()
	{
		_graph.SnapDistance = (int)_snapSizeField.Value;
	}

	private void OnRecentTemplateMenuSelection( int id )
	{
		try
		{
			string path = _recentTemplateSubmenu.GetItemText( id );
			LoadTemplate( LoadJsonFile( path ) );
			AddRecentTemplateFile( path );
		}
		catch( Exception e )
		{
			CatchException( e );
		}
	}

	private void OnRecentShiftMenuSelection( int id )
	{
		try
		{
			ShiftTemplateUnderData(_recentShiftSubmenu.GetItemText( id ));
			string path = _recentShiftSubmenu.GetItemText( id );
			ShiftTemplateUnderData( path );
			AddRecentTemplateFile( path );
		}
		catch( Exception e )
		{
			CatchException( e );
		}
	}

	private void OnRecentMenuSelection( int id )
	{
		try
		{
			LoadGraph( _recentSubmenu.GetItemText( id ) );
		}
		catch( Exception e )
		{
			CatchException( e );
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

	private void LoadDefaultTemplate()
	{
		LoadTemplate( LoadJsonFile( DEFAULT_TEMPLATE_SOURCE ) );

		HasUnsavedChanges = false;
		SaveFilePath = null;
	}

	private void PickTemplateToLoad()
	{
		_filePopup.Show( new[] { "*.tmplt" }, false, "Open a Template file" )
			.Then( path =>
			{
				LoadTemplate( LoadJsonFile( path ) );

				AddRecentTemplateFile( path );
				HasUnsavedChanges = false;
				SaveFilePath = null;
			} )
			.Catch( CatchException );
	}

	private IPromise CheckIfSavedFirst()
	{
		if(!HasUnsavedChanges) return Promise.Resolved();
		
		Promise promise = new Promise();

		_areYouSurePopup.Display( 
			leftPress: () => { SaveGraph().Then( promise.Resolve ); },
			middlePress: promise.Resolve,
			rightPress: () => { promise.Reject( new PromiseExpectedError( "cancel" ) ); } 
			);
		
		return promise;
	}
	
	private IPromise SaveGraph()
	{
		if( string.IsNullOrEmpty( SaveFilePath ) )
		{
			return SaveGraphAs();
		}

		SaveGraph( SaveFilePath );

		return Promise.Resolved();
	}

	private IPromise SaveGraphAs()
	{
		IPromise promise =
		_filePopup.Show( new[] { "*.ngmap" }, true, "Save Full Graph" )
			.Then( SaveGraph );
		
		promise.Catch( CatchException );
		
		return promise;
	}

	private void SaveGraph( string path )
	{
		try
		{
			SaveFilePath = path;
			includeNodeData = true;
			includeGraphData = true;
			var graph = new GenericDataArray();

			// save template
			graph.AddValue( "targetVersion", _version );
			graph.AddValue( "defaultListing", _defaultListing );
			graph.AddValue( "explicitNode", _explicitNode );
			graph.AddValue( "nestingColors", _parentChildColors );
			graph.AddValue( "keyColors", _keyColors );
			List<GenericDataArray> nodeList = _nodeData.Values.ToList();
			graph.AddValue( "nodeList", nodeList );
					
			// save data
			graph.AddValue( "data", SaveData() );
					
			string json = graph.ToJson();
					
			SaveJsonFile( path, json );

			HasUnsavedChanges = false;
			
			AddRecentFile( path );
		}
		catch( Exception e )
		{
			CatchException( e );
		}
	}

	private void LoadGraph()
	{
		_filePopup.Show( new[] { "*.ngmap" }, false, "Load Full Graph" )
			.Then( LoadGraph )
			.Catch( CatchException );
	}

	private void LoadGraph( string path )
	{
		string json = LoadJsonFile( path );

		LoadTemplate( json );

		var graph = new GenericDataArray();
		graph.FromJson( json );
		graph.GetValue( "data", out GenericDataArray data );
		LoadData( data );

		HasUnsavedChanges = false;
		SaveFilePath = path;
		AddRecentFile( path );
	}

	private void ExportDataJson()
	{
		_filePopup.Show( new[] { "*.json" }, true, "Save Data to JSON" )
			.Then( path =>
			{
				includeGraphData = false;
				SaveJsonFile( path, SaveData().ToJson() );
			} )
			.Catch( CatchException );
	}

	private void ExportTemplateJson()
	{
		_filePopup.Show( new[] { "*.tmplt" }, true, "Save Data to Template" )
			.Then( path =>
			{
				includeNodeData = false;
				includeGraphData = false;
				
				GenericDataArray gda = new GenericDataArray();
				gda.AddValue( "targetVersion", _version );

				GenericDataArray data = SaveData();
				foreach( KeyValuePair<string,GenericDataObject> keyValuePair in data.values )
				{
					gda.AddValue( keyValuePair.Key, keyValuePair.Value );
				}
				
				SaveJsonFile( path, gda.ToJson() );
				AddRecentTemplateFile( path );
			} )
			.Catch( CatchException );
	}
	
	private void ImportDataJson()
	{
		_filePopup.Show( new[] { "*.json" }, false, "Load Data from JSON" )
			.Then( path =>
			{
				ClearData();

				string json = LoadJsonFile( path );
				var data = new GenericDataArray();
				data.FromJson( json );
				LoadData( data );

				this.DelayedInvoke( 0.5f, SortNodes );
			} )
			.Catch( CatchException );
	}

	private void ShiftTemplateUnderData()
	{
		_filePopup.Show( new[] { "*.tmplt" }, false, "Select Template File for Use" )
			.Then( ShiftTemplateUnderData )
			.Catch( CatchException );
	}

	private void ShiftTemplateUnderData(string path)
	{
		try
		{
			includeNodeData = true;
			includeGraphData = true;
			GenericDataArray data = SaveData();
			LoadTemplate( LoadJsonFile( path ) );
			LoadData( data );
			AddRecentTemplateFile( path );
		}
		catch( Exception ex )
		{
			CatchException( ex );
		}
	}

	private void RequestQuit()
	{
		SaveSettings();
		CheckIfSavedFirst()
			.Then( ()=> GetTree().Quit() );
	}

	private void ClearData()
	{
		generatedKeys.Clear();
		for( int i = _nodes.Count-1; i >= 0; i-- )
		{
			_nodes[i].CloseRequest();
		}
		
		HasUnsavedChanges = false;
		SaveFilePath = null;
	}

	private void OpenHelpPopup()
	{
		_helpInfoPopup.PopupCentered();
	}

	private void RequestCopyNode()
	{
		if( _lastSelection == null ) return;

		includeNodeData = true;
		includeGraphData = true;
	
		_copied = _lastSelection.GetObjectData();
	}

	private void RequestPasteNode()
	{
		if( _copied == null ) return;

		pastingData = true;
		
		int start = _nodes.Count;
		LoadNode( _copied );

		Vector2 startOffset = _offset - _nodes[start].Offset;

		for( int i = start; i < _nodes.Count; i++ )
		{
			_nodes[i].Offset = _nodes[i].Offset + startOffset;
			_graph.SetSelected( _nodes[i] );
		}

		pastingData = false;
	}

	private void RequestDeleteNode()
	{
		if( _lastSelection == null ) return;
		
		_lastSelection.CloseRequest();
		
		HasUnsavedChanges = true;
	}

	private void SelectNode(Node node)
	{
		//Log.LogL( $"Selected {node.Name}" );
		_lastSelection = node as SlottedGraphNode;
		
		HasUnsavedChanges = true;
	}

	private void DeselectNode(Node node)
	{
		//Log.LogL( $"Deselected {node.Name}" );
		//_lastSelection = null;
	}
	#endregion

	#region Helper Functions
	private SlottedGraphNode CreateNode(GenericDataArray nodeData, GenericDataArray nodeContent = null)
	{
		if( nodeData == null ) throw new ArgumentNullException($"CreateNode passed NULL nodeData");

		
		Node child = nodeScene.Instance();
		_graph.AddChild(child);
		var node = child as SlottedGraphNode;

		if(node == null) throw new NullReferenceException("nodeScene is not a SlottedGraphNode");
		
		node.Offset = _offset;
		
		_nodes.Add( node );

		node.SetSlots( nodeData );
		HasUnsavedChanges = true;

		if( nodeContent == null ) return node;
		
		node.SetObjectData( nodeContent );

		return node;
	}

	private string LoadJsonFile( string path )
	{
		File file = new File();
		if( !file.FileExists( path ) )
		{
			throw new FileNotFoundException($"File '{path}' not found!");
		}
		
		file.Open( path, File.ModeFlags.Read );
		
		string json = file.GetAsText();
		file.Close();

		return json;
	}

	private void SaveJsonFile( string path, string json )
	{
		File file = new File();
		file.Open( path, File.ModeFlags.Write );
		
		file.StoreString( json );
		file.Close();
	}

	private void LoadTemplate( string json )
	{
		GenericDataArray gda = new GenericDataArray();
		gda.FromJson( json );

		gda.GetValue( "targetVersion", out string targetVersion );

		if( targetVersion != _version )
		{
			ShowNotice($"File version is '{targetVersion}' which does not match app version '{_version}'");
		}

		gda.GetValue( "defaultListing", out _defaultListing );
		gda.GetValue( "explicitNode", out _explicitNode );
		
		if( string.IsNullOrEmpty( _defaultListing ) )
		{
			_defaultListing = "listing";
		}
		GenericDataArray listing = gda.GetGdo( "nodeList" ) as GenericDataArray;

		gda.GetValue( "nestingColors", out List<Color> nestingColors );
		gda.GetValue( "keyColors", out List<Color> keyColors );
		
		LoadTemplate( listing.values.Values.ToList(), nestingColors, keyColors );
	}

	private void LoadTemplate(List<GenericDataObject> dataList, List<Color> nestingColors, List<Color> keyColors )
	{
		if( dataList == null )
		{
			throw new NullReferenceException("LoadTemplate does not accept a null dataList");
		}
		
		ClearData();
		_nodeData.Clear();
		_createSubmenu.Clear();
		_createSubmenu.RectSize = Vector2.Zero;

		if( nestingColors != null && nestingColors.Count > 0 )
		{
			_parentChildColors.Clear();
			_parentChildColors.AddRange( nestingColors );
		}
		else
		{
			_parentChildColors.Clear();
			_parentChildColors.Add( Colors.Aquamarine );
			_parentChildColors.Add( Colors.Beige );
			_parentChildColors.Add( Colors.Gold );
			_parentChildColors.Add( Colors.Black );
			_parentChildColors.Add( Colors.Firebrick );
		}

		if( keyColors != null && keyColors.Count > 0 )
		{
			_keyColors.Clear();
			_keyColors.AddRange( keyColors );
		}
		else
		{
			_keyColors.Clear();
			_keyColors.Add( Colors.Chartreuse );
			_keyColors.Add( Colors.Cornflower );
			_keyColors.Add( Colors.Cornsilk );
			_keyColors.Add( Colors.Indigo );
			_keyColors.Add( Colors.HotPink );
		}
		
		foreach( GenericDataObject gdo in dataList )
		{
			GenericDataArray item = gdo as GenericDataArray;
			
			item.GetValue( "title", out string title );

			if( _nodeData.ContainsKey( title ) )
			{
				ShowNotice( $"Duplicate node title '{title}' found in the template.\nNodes must all have unique titles" );
			}
			else
			{
				_nodeData[title] = item;
			}
		}

		foreach( KeyValuePair<string,GenericDataArray> dataPair in _nodeData )
		{
			_createSubmenu.AddItem( dataPair.Key );
		}
	}

	private GenericDataArray SaveData()
	{
		List<GenericDataArray> parentSlots = new List<GenericDataArray>();

		foreach( SlottedGraphNode node in _nodes )
		{
			if( !node.LinkedToParent )
			{
				parentSlots.Add( node.GetObjectData() );
			}
		}

		if( parentSlots.Count == 1 )
		{
			return parentSlots[0];
		}
		
		GenericDataArray gda = new GenericDataArray();

		gda.AddValue( _defaultListing, parentSlots );

		return gda;
	}

	private void LoadData(GenericDataArray data)
	{
		if( data.values.ContainsKey( "ngMapNodeName" ) )
		{
			LoadNode( data );
		}
		else if(data.values.Count == 1)
		{
			string key = data.values.Keys.First();
			
			data.GetValue( key, out List<GenericDataArray> items );

			foreach( GenericDataArray item in items )
			{
				if( !item.values.ContainsKey( "ngMapNodeName" ) )
				{
					item.AddValue( "ngMapNodeName", _explicitNode );
				}
				LoadNode( item );
			}
		}
		else
		{
			data.AddValue( "ngMapNodeName", _explicitNode );
			LoadNode( data );
		}

		foreach( Tuple<KeyLinkSlot,string> loadingLink in _loadingLinks )
		{
			LinkToKey( loadingLink.Item1, loadingLink.Item2 );
		}
		
		_loadingLinks.Clear();
	}
	
	public void CatchException( Exception ex )
	{
		if(ex is PromiseExpectedError) return;
		
		Log.Except( ex );
		_errorPopup.DialogText = ex.Message;
		
		#if DEBUG
			_errorPopup.DialogText = $"{ex.Message} \n\n{ex.StackTrace}";
		#endif
		
		_errorPopup.PopupCentered();
	}
	
	
	private void ShowNotice( string notice )
	{
		Log.LogL( notice );
		_errorPopup.DialogText = notice;
		_errorPopup.PopupCentered();
	}
	
	private void AddRecentFile( string path )
	{
		if( _recentFiles.Contains( path ) )
		{
			_recentFiles.Remove( path );
		}
		_recentFiles.Insert( 0, path );
		_recentSubmenu.Clear();

		foreach( string file in _recentFiles )
		{
			_recentSubmenu.AddItem( file );
		}

		if( _recentFiles.Count > 0 )
		{
			_filePopup.CurrentDir = _recentFiles[0].Substring( 0, _recentFiles[0].LastIndexOf( '/' ));
		}
	}
	
	private void AddRecentTemplateFile( string path )
	{
		if( _recentTemplates.Contains( path ) )
		{
			_recentTemplates.Remove( path );
		}
		_recentTemplates.Insert( 0, path );
		_recentTemplateSubmenu.Clear();
		_recentShiftSubmenu.Clear();

		foreach( string file in _recentTemplates )
		{
			_recentTemplateSubmenu.AddItem( file );
			_recentShiftSubmenu.AddItem( file );
		}
	}

	private void SortNodes()
	{
		Vector2 currentPos = Vector2.Zero;
		
		foreach( SlottedGraphNode node in _nodes )
		{
			if( !node.LinkedToParent )
			{
				Vector2 size = SortChild( node, currentPos );
				currentPos += new Vector2(0, size.y);
			}
		}
	}

	private void UpdateTitle()
	{
		var title = "MetaMaker";
		if( !string.IsNullOrEmpty( _saveFilePath ) )
		{
			title += " - " + _saveFilePath;
		}

		if( HasUnsavedChanges )
		{
			title += "*";
		}
		
		OS.SetWindowTitle( title );
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

	public SlottedGraphNode LoadNode( GenericDataArray nodeContent )
	{
		if( _nodeData.Count <= 0 ) throw new KeyNotFoundException("No Nodes currently loaded");

		nodeContent.GetValue( "ngMapNodeName", out string nodeName );

		if( !_nodeData.ContainsKey( nodeName ) )
		{
			throw new ArgumentOutOfRangeException($"'{nodeName}' not found in loaded data");
		}
		
		GenericDataArray nodeData = _nodeData[nodeName];
		
		if( nodeData == null ) throw new ArgumentNullException($"Data for '{nodeName}' was null");

		return CreateNode(nodeData, nodeContent);
	}

	public Color GetParentChildColor( int slotType )
	{
		return _parentChildColors[slotType % _parentChildColors.Count];
	}

	public Color GetKeyColor( int slotType )
	{
		return _keyColors[slotType % _keyColors.Count];
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
		CenterViewOnNode(generatedKeys[key].GetParent<SlottedGraphNode>());
	}
	
	public void CenterViewOnNode(SlottedGraphNode node)
	{
		CenterView( node.Offset + node.RectSize * 0.5f );
	}

	public void CenterView( Vector2 position )
	{
		_tween.Remove( _graph, "scroll_offset" );
		_tween.InterpolateProperty( _graph,
			"scroll_offset",
			_graph.ScrollOffset,
			position * _graph.Zoom - _graph.RectSize * 0.5f,
			0.3f );
		_tween.Start();
	}

	private string PartialFindKey( string partial )
	{
		if( string.IsNullOrEmpty( partial ) ) return null;
		
		foreach( string key in generatedKeys.Keys )
		{
			if( key.Contains( partial ) ) return key;
		}
		
		return null;
	}
	#endregion

	#region Connections
	public void OnConnectionRequest( string from, int fromSlot, string to, int toSlot)
	{
		Node leftSlot = _graph.GetNode<SlottedGraphNode>( from ).GetSlot( fromSlot );
		var rightGraph = _graph.GetNode<SlottedGraphNode>( to );
		Node rightSlot = rightGraph.GetSlot( toSlot );

		if( leftSlot is KeySlot leftKey )
		{
			if( !( rightSlot is KeyLinkSlot keyLink ) )
			{
				throw new Exception($"{rightSlot.Name} is not a KeyLinkSlot");
			}
			if( !keyLink.AddKey( leftKey.GetKey ) ) return;

			_graph.ConnectNode( @from, fromSlot, to, toSlot );
		}
		else if( leftSlot is LinkToChildSlot leftLink )
		{
			if( !( rightSlot is LinkToParentSlot rightLink) || rightLink.IsLinked ) return;
			if( !leftLink.LinkChild( rightGraph ) ) return;

			rightLink.Link();
			_graph.ConnectNode( @from, fromSlot, to, toSlot );
		}
	}
	
	private void OnDisconnectionRequest( string from, int fromSlot, string to, int toSlot)
	{
		Node leftSlot = _graph.GetNode<SlottedGraphNode>( from ).GetSlot( fromSlot );
		var rightGraph = _graph.GetNode<SlottedGraphNode>( to );
		Node rightSlot = rightGraph.GetSlot( toSlot );

		if( leftSlot is KeySlot leftKey )
		{
			if( !( rightSlot is KeyLinkSlot keyLink ) )
			{
				throw new Exception($"{rightSlot.Name} is not a KeyLinkSlot");
			}
			if( !keyLink.RemoveKey( leftKey.GetKey ) ) return;

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
		if( _lastSelection == graphNode )
		{
			_lastSelection = null;
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

		_nodes.Remove( graphNode );
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

	private void LinkToKey( KeyLinkSlot link, string key )
	{
		if( !generatedKeys.ContainsKey( key ) )
		{
			throw new ArgumentException("Attempting link to key not found",nameof(key));
		}
		
		SlottedGraphNode linkParent = link.GetParent<SlottedGraphNode>();
		int toSlot = linkParent.GetChildIndex( link );

		if( toSlot < 0 )
		{
			throw new ArgumentException($"{link.Name} not found in {linkParent.Name}",nameof(link));
		}
		
		SlottedGraphNode keyParent = generatedKeys[key].GetParent<SlottedGraphNode>();
		int fromSlot = keyParent.GetChildIndex( generatedKeys[key] );
		
		if( fromSlot < 0 )
		{
			throw new ArgumentException($"{generatedKeys[key].Name} not found in {keyParent.Name}",nameof(link));
		}
		
		OnConnectionRequest( keyParent.Name, fromSlot ,linkParent.Name, toSlot );
	}
	#endregion
}
