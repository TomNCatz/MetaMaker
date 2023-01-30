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
	[Export] public NodePath noticePopupPath;
	[Export] public NodePath settingsPopupPath;

	private NoticePopup _noticePopup;
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
		_noticePopup = GetNode<NoticePopup>( noticePopupPath );
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
	public async Task CatchException( Exception ex )
	{
		Log.Except( ex );
		
#if DEBUG
		await _noticePopup.Show($"{ex.Message} \n\n{ex.StackTrace}");
#else
		await _errorPopup.Show(ex.Message);
#endif
			
	}
	
	/// <summary>
	/// Show a notice to the user that isn't necessarily an error
	/// </summary>
	/// <param name="notice"> text that will display</param>
	public async Task ShowNotice( string notice )
	{
		Log.LogL( notice );
		await _noticePopup.Show(notice);
	}
	
	public async Task RequestQuit()
	{
		var shouldQuit = await _saveService.CheckIfSavedFirst();
		if(!shouldQuit) return;
		
		GetTree().Quit();
	}
}
