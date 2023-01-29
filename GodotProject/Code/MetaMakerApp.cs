using System;
using System.Threading.Tasks;
using Godot;
using LibT;
using LibT.Services;
using MetaMaker.Services;
using MetaMaker.UI;

namespace MetaMaker;

public partial class MetaMakerApp : App
{
	[Export] public NodePath mainMenuBarPath;
	[Export] public NodePath inspectorPath;
	[Export] public NodePath prefabricatorPath;
	[Export] public NodePath treeViewPath;
	[Export] public NodePath filePopup;
	[Export] public NodePath areYouSurePopup;
	[Export] public NodePath errorPopupPath;
	[Export] public NodePath settingsPopupPath;

	private AcceptDialog _errorPopup;
	private SaveService _saveService;
	
	public override void _Ready()
	{
		Ready();
	}

	public override void _Notification(long what)
	{
		if( what == NotificationWmCloseRequest )
		{
			RequestQuit();
		}
	}

	public override async Task Bindings(Context context)
	{
		OS.LowProcessorUsageMode = true;
		
		GetTree().SetAutoAcceptQuit(false);
		
		// Pull in Directly Linked Bindings
		context.Set(this);
		context.Set(GetNode<MainMenuBar>( mainMenuBarPath ));
		context.Set(GetNode<Inspector>( inspectorPath ));
		context.Set(GetNode<Prefabricator>( prefabricatorPath ));
		context.Set(GetNode<TreeView>( treeViewPath ));
		context.Set(GetNode<FileDialogAsync>( filePopup ));
		context.Set(GetNode<AreYouSurePopup>( areYouSurePopup ));
		_errorPopup = GetNode<AcceptDialog>( errorPopupPath );
		context.Set(GetNode<SettingsPopup>( settingsPopupPath ));
		
		// Create and Bind Services
		context.Set(context.Construct<AppSettings>());
		_saveService = context.Set(context.Construct<SaveService>());
		context.Set(context.Construct<TemplateService>());
		context.Set(context.Construct<DataService>());
		
		context.InjectAll();

		// Not strictly needed, but makes me feel better
		await Task.Delay(500);
	}

	public override async Task StartApp()
	{
		VersionDisplay = _saveService.LoadJsonFile( SaveService.VERSION );
		AppContext.Get<AppSettings>().LoadSettings();
		
		// Not strictly needed, but makes me feel better
		await Task.Delay(500);
		UpdateTitle();
	}
	
	/// <summary>
	/// Update the app title bar to reflect current app state
	/// </summary>
	public void UpdateTitle()
	{
		var title = "MetaMaker" + VersionDisplay;
		if( !string.IsNullOrEmpty( _saveService.SaveFilePath ) )
		{
			title += " - " + _saveService.SaveFilePath;
		}

		if( _saveService.HasUnsavedChanges )
		{
			title += "*";
		}
		DisplayServer.WindowSetTitle(title);
	}
	
	/// <summary>
	/// app wide error handling
	/// </summary>
	/// <param name="ex"></param>
	public void CatchException( Exception ex )
	{
		// todo error popup should probably be async and be awaited so if multiple messages pop at once the user has a chance to read them all
		Log.Except( ex );
		
		_errorPopup.DialogText = ex.Message;
			
#if DEBUG
		_errorPopup.DialogText = $"{ex.Message} \n\n{ex.StackTrace}";
#endif
			
		_errorPopup.PopupCentered();
	}
	
	/// <summary>
	/// Show a notice to the user that isn't necessarily an error
	/// </summary>
	/// <param name="notice"> text that will display</param>
	public void ShowNotice( string notice )
	{
		// todo error popup should probably be async and be awaited so if multiple messages pop at once the user has a chance to read them all
		Log.LogL( notice );
		_errorPopup.DialogText = notice;
		_errorPopup.PopupCentered();
	}
	
	public async Task RequestQuit()
	{
		var shouldQuit = await _saveService.CheckIfSavedFirst();
		if(!shouldQuit) return;
		
		GetTree().Quit();
	}
}
