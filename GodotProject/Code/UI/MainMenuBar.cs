using System;
using System.Collections.Generic;
using Godot;
using LibT.Services;
using MetaMaker.Services;

namespace MetaMaker.UI;

public partial class MainMenuBar : Control
{
	[Export] public NodePath _fileMenuPath;
	[Export] public NodePath _dataMenuPath;
	[Export] public NodePath _editMenuPath;
	[Export] public NodePath _settingsMenuPath;
	
	private MenuButton _fileMenuButton;
	private MenuButton _dataMenuButton;
	private MenuButton _editMenuButton;
	private MenuButton _settingsMenuButton;
	private PopupMenu _editMenu;
	private PopupMenu _recentTemplateSubmenu;
	private PopupMenu _recentShiftSubmenu;
	private PopupMenu _recentSubmenu;
	private PopupMenu _createSubmenu;
	private PopupMenu _exportSubmenu;

	[Injectable]
	private MetaMakerApp _app;
	[Injectable]
	private SaveService _saveService;
	[Injectable]
	private FileDialogAsync _fileDialog;
	[Injectable]
	private SettingsPopup _settingsPopup;
	[Injectable]
	private AppSettings _settings;

	public override void _Ready()
	{
		_fileMenuButton = GetNode<MenuButton>(_fileMenuPath);
		_dataMenuButton = GetNode<MenuButton>(_dataMenuPath);
		_editMenuButton = GetNode<MenuButton>(_editMenuPath);
		_settingsMenuButton = GetNode<MenuButton>(_settingsMenuPath);
			
		PopupMenu fileMenu = _fileMenuButton.GetPopup();
		fileMenu.Connect("id_pressed", new Callable(this, nameof(OnFileMenuSelection)));
		fileMenu.AddItem("New Graph (CTRL+N)", 0);
		fileMenu.AddItem("Save Graph (CTRL+S)", 1);
		fileMenu.AddItem("Save Graph As", 2);
		fileMenu.AddItem("Load Graph", 3);
		_recentSubmenu = new PopupMenu { Name = "RecentMenu" };
		_recentSubmenu.Connect( "id_pressed", new Callable(this, nameof(OnRecentMenuSelection)));
		fileMenu.AddChild( _recentSubmenu );
		fileMenu.AddSubmenuItem("Recent Graph", "RecentMenu");
		fileMenu.AddSeparator();
		fileMenu.AddItem("Export All (CTRL+E)", 4);
		_exportSubmenu = new PopupMenu { Name = "ExportMenu" };
		//_exportSubmenu.Connect( "id_pressed", new Callable(this, nameof(OnExportMenuSelection)));
		fileMenu.AddChild(_exportSubmenu);
		fileMenu.AddSubmenuItem("Export", "ExportMenu");
		fileMenu.AddSeparator();
		fileMenu.AddItem("Quit", 5);

		PopupMenu dataMenu = _dataMenuButton.GetPopup();
		dataMenu.Connect("id_pressed", new Callable(this, nameof(OnDataMenuSelection)));
		dataMenu.AddItem("Build Template (CTRL+SHIFT+N)", 0);
		dataMenu.AddItem("Load Template", 1);
		_recentTemplateSubmenu = new PopupMenu { Name = "RecentTemplateMenu" };
		_recentTemplateSubmenu.Connect( "id_pressed", new Callable(this, nameof(OnRecentTemplateMenuSelection)));
		dataMenu.AddChild( _recentTemplateSubmenu );
		dataMenu.AddSubmenuItem("Recent Template", "RecentTemplateMenu");
		dataMenu.AddSeparator();
		dataMenu.AddItem("Shift Template Under Data", 2);
		_recentShiftSubmenu = new PopupMenu { Name = "RecentShiftMenu" };
		_recentShiftSubmenu.Connect( "id_pressed", new Callable(this, nameof(OnRecentShiftMenuSelection)));
		dataMenu.AddChild( _recentShiftSubmenu );
		dataMenu.AddSubmenuItem("Recent Template Under Data", "RecentShiftMenu");
		dataMenu.AddSeparator();
		dataMenu.AddItem("Clear Data", 3);
		dataMenu.AddItem("Trim Dissconnected Data", 4);
		dataMenu.AddItem("Import Data from JSON", 5);

		_editMenu = _editMenuButton.GetPopup();
		_editMenu.Connect("id_pressed", new Callable(this, nameof(OnEditMenuSelection)));
		_createSubmenu = new PopupMenu { Name = "CreateMenu" };
		//_createSubmenu.Connect( "id_pressed", new Callable(this, nameof(OnCreateMenuSelection)));
		_editMenu.AddChild( _createSubmenu );
		_editMenu.AddSubmenuItem("Create", "CreateMenu");
		_editMenu.AddSeparator();
		_editMenu.AddItem("Copy Nodes (CTRL+C)", 0);
		_editMenu.AddItem("Paste Nodes (CTRL+V)", 1);
		_editMenu.AddItem("Delete Nodes (DEL)", 2);
		_editMenu.AddSeparator();
		_editMenu.AddItem("Find (CTRL+F)", 3);

		PopupMenu settingsMenu = _settingsMenuButton.GetPopup();
		settingsMenu.Connect("id_pressed", new Callable(this, nameof(OnSettingsMenuSelection)));
		settingsMenu.AddItem("Editor Settings...", 0);
		settingsMenu.AddSeparator();
		settingsMenu.AddItem("Help and FAQ", 1);
	}


	private void OnFileMenuSelection( int id )
	{
		try
		{
			switch(id)
			{
				// case 0 : _app.NewFromLastTemplate(); break;
				// case 1 : _app.SaveGraph(); break;
				// case 2 : SaveGraphAs(); break;
				case 3 : _saveService.PickGraphToLoad(); break;
				// case 4 :  _app.ExportAll(); break;
				case 5 :  _app.RequestQuit(); break;
			}
		}
		catch( Exception e )
		{
			_app.CatchException( e );
		}
	}
	
	private void OnDataMenuSelection( int id )
	{
		try
		{
			switch(id)
			{
				// case 0 :  _app.LoadDefaultTemplate(); break;
				// case 1 :  PickTemplateToLoad(); break;
				// case 2 :  ShiftTemplateUnderData(); break;
				// case 3 :  _app.ClearData(); break;
				// case 4 :  TrimOldData(); break;
				// case 5 :  ImportDataJson(); break;
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
				// case 0 :  RequestCopyNode(); break;
				// case 1 :  RequestPasteNode(); break;
				// case 2 :  RequestDeleteNode(); break;
				// case 3 :  _app.OpenSearch(); break;
			}
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
				case 0 : _settingsPopup.PopupCentered(); break;
				// case 1 :  _app.OpenHelpPopup(); break;
			}
		}
		catch( Exception e )
		{
			_app.CatchException( e );
		}
	}
	
	public async void OnRecentMenuSelection( int id )
	{
		try
		{
			await _saveService.LoadGraph( _settings.RecentFiles[id] );
		}
		catch( Exception e )
		{
			_app.CatchException( e );
		}
	}
	
	public async void OnRecentTemplateMenuSelection( int id )
	{
		try
		{
			await _saveService.LoadGraph( _settings.RecentTemplates[id] );
		}
		catch( Exception e )
		{
			_app.CatchException( e );
		}
	}
	
	public async void OnRecentShiftMenuSelection( int id )
	{
		try
		{
			// todo not implimented yet
			//await _saveService.ShiftTemplateUnderGraph( _settings.RecentTemplates[id] );
		}
		catch( Exception e )
		{
			_app.CatchException( e );
		}
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
			_fileDialog.CurrentDir = recents[0].Substring( 0, recents[0].LastIndexOf( '/' ));
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
	
	public void UpdateCreateMenu(List<string> nodeNames)
	{
		_createSubmenu.Clear();
		_createSubmenu.Size = Vector2i.Zero;

		foreach( string name in nodeNames )
		{
			_createSubmenu.AddItem( name );
		}
	}

	// public void ResetExportMenu(Dictionary<string, ExportSet> exports)
	// {
	// 	_exportSubmenu.Clear();
	// 	_exportSubmenu.Size = Vector2.Zero;
	//
	// 	foreach( var export in exports )
	// 	{
	// 		_exportSubmenu.AddItem( export.Key );
	// 	}
	// }
}
