using System;
using System.IO;
using System.Threading.Tasks;
using LibT;
using LibT.Serialization;
using LibT.Services;
using MetaMaker.UI;
using FileAccess = Godot.FileAccess;

namespace MetaMaker.Services;

public class SaveService
{
	#region Const Values
	public const string SETTINGS_SOURCE = "user://Settings.json";
	public const string DEFAULT_TEMPLATE_SOURCE = "res://Resources/TEMPLATE.tmplt";
	public const string HELP_INFO_SOURCE = "res://Resources/HelpInfo.json";
	public const string VERSION = "res://Resources/Version.txt";

	public const string NODE_NAME_KEY = "ngMapNodeName";
	public const string NODE_POSITION_KEY = "ngMapNodePosition";
	public const string NODE_SIZE_KEY = "ngMapNodeSize";

	private const string BACKUP_FILE_EXTENSION = ".back";
	#endregion
	
	private bool _hasUnbackedChanges;
	private string _loadingPath;
	private GenericDataDictionary _model;
	
	[Injectable]
	private FileDialogAsync _fileDialog;
	[Injectable]
	private AreYouSurePopup _areYouSurePopup;
	[Injectable]
	private MetaMakerApp _app;
	[Injectable]
	private AppSettings _settings;
	[Injectable]
	private TemplateService _templateService;
	[Injectable]
	private DataService _dataService;
	
	public string SaveFilePath
	{
		set
		{
			_saveFilePath = value;
			_app.UpdateTitle();
		}
		get => _saveFilePath;
	}
	private string _saveFilePath;

	public bool HasUnsavedChanges
	{
		set
		{
			_hasUnsavedChanges = value;
			_hasUnbackedChanges = _hasUnsavedChanges;
			_app.UpdateTitle();
			if(!_hasUnsavedChanges)
			{
				//_mainView.SetNodesClean();
				//MainView.ScrollLock = false;
			}
		}
		get => _hasUnsavedChanges;
	}
	private bool _hasUnsavedChanges;

	public SaveService()
	{
		_model = new GenericDataDictionary();
	}
	
	
	public async Task ClearData()
	{
		_model.values.Clear();
		//generatedKeys.Clear();
		//keySearch.Clear();

		// for( int i = _mainView.nodes.Count-1; i >= 0; i-- )
		// {
		// 	_mainView.nodes[i].CloseRequest();
		// }
		// 	
		// _mainView.CurrentParentIndex = -1;
		// _nodeData.Clear();
		ClearBackup();
		HasUnsavedChanges = false;
		SaveFilePath = null;
		//_exportRules = null;
		await Task.Delay(100);
	}

	public void ClearBackup()
	{
		if( string.IsNullOrEmpty( SaveFilePath ) ) return;
			
		if( !FileAccess.FileExists( SaveFilePath + BACKUP_FILE_EXTENSION ) ) return;

		File.Delete(SaveFilePath + BACKUP_FILE_EXTENSION);
	}
	
	/// <summary>
	/// makes sure the user has a chance to save any changes if they want to
	/// </summary>
	/// <returns>True if we should continue</returns>
	public async Task<bool> CheckIfSavedFirst()
	{
		if(!HasUnsavedChanges) return true;
		
		var choice = await _areYouSurePopup.Display( new AreYouSurePopup.AreYouSureArgs(){
			title = "Save?",
			info = "Some unsaved changes might be lost\nwould you like to save first?",
			leftText = "Save",
			middleText = "Continue",
			rightText = "Cancel"
		});

		switch (choice)
		{
			case AreYouSurePopup.Choice.LEFT :
				await SaveGraph();
				return true;
			case AreYouSurePopup.Choice.MIDDLE:
				await ClearData();
				return true;
			default: return false;
		}
	}
	
	private async Task<bool> CheckIfShouldRestoreFirst(string path)
	{
		if( !FileAccess.FileExists( path + BACKUP_FILE_EXTENSION ) ) return true;

		var choice = await _areYouSurePopup.Display( new AreYouSurePopup.AreYouSureArgs(){
			title = "Restore?",
			info = $"A backup of was detected for\n{path}\nwould you like to load it or discard it?",
			leftText = "Restore",
			middleText = "Discard",
			rightText = "Cancel",
		});

		switch (choice)
		{
			case AreYouSurePopup.Choice.LEFT:
				try
				{
					string json = LoadJsonFile( path + BACKUP_FILE_EXTENSION);
			
					await ClearData();
			
					_model.GddFromJson( json );
					_model.GetValue( TemplateService.GRAPH_TEMPLATE_KEY, out GenericDataDictionary template );
			
					_templateService.LoadTemplate(template);

					_model.GetValue( DataService.GRAPH_DATA_KEY, out GenericDataDictionary graph );
					_dataService.LoadData( graph );

					SaveFilePath = path;
					HasUnsavedChanges = false;
					_settings.AddRecentFile( path );
					return true; // todo not sure this is right, double check when more is working
				}
				catch( Exception e )
				{
					await _app.CatchException( e );
					return false;
				}
			case AreYouSurePopup.Choice.MIDDLE:
				SaveFilePath = path;
				await ClearData();
				return true;
			default: return false;
		}
	}

	#region SaveFlow
	/// <summary>
	/// opens the file picker to save the graph
	/// </summary>
	public async Task SaveGraphAs()
	{
		try
		{
			var path = await _fileDialog.Show(new[] { "*.ngmap" }, true, "Save Full Graph");
			if(string.IsNullOrEmpty(path)) return;

			SaveGraph(path);
		}
		catch (Exception e)
		{
			await _app.CatchException(e);
			throw;
		}
	}
	
	/// <summary>
	/// attempts to save the graph under it's given name.
	/// opens the file picker if no name has been given
	/// </summary>
	/// <returns>a task that can be awaited</returns>
	public async Task SaveGraph()
	{
		if( string.IsNullOrEmpty( SaveFilePath ) )
		{
			await SaveGraphAs();
			return;
		}

		SaveGraph( SaveFilePath );
	}

	public async Task SaveBackup()
	{
		if( !_settings.AutoBackup ) return;
		if( !_hasUnbackedChanges ) return;
		if( string.IsNullOrEmpty( SaveFilePath ) ) return;
			
		_hasUnbackedChanges = false;

		try
		{
			SaveJsonFile( SaveFilePath + BACKUP_FILE_EXTENSION, _model.DataCopy().GddToJson() );
		}	
		catch(Exception e)
		{
			await _app.CatchException(e);
		}
	}
	
	
	public async Task SaveGraph( string path )
	{
		try
		{
			SaveFilePath = path;
				
			SaveJsonFile( SaveFilePath, _model.DataCopy().GddToJson() );

			HasUnsavedChanges = false;
			ClearBackup();
		}
		catch( Exception e )
		{
			await _app.CatchException( e );
		}
	}
	
	public async Task SaveJsonFile( string path, string json )
	{
		try
		{
			if(!path.Contains("user:")&&!path.Contains("res:"))
			{
				Serializer.CreatePathIfMissing( path.PathGetContainingFolder() );
			}

			var file = FileAccess.Open( path, FileAccess.ModeFlags.Write );
				
			file.StoreString( json );
			file.Dispose();
				
			if(path.Contains(".tmplt"))
			{
				_settings.AddRecentTemplateFile( path );
			}
			if(!path.Contains(BACKUP_FILE_EXTENSION) && path.Contains(".ngmap"))
			{
				_settings.AddRecentFile( path );
			}
		}
		catch(Exception ex)
		{
			await _app.CatchException(ex);
		}
	}
	#endregion

	#region Load Flow
	
	public async Task PickTemplateToLoad()
	{
		string path = await _fileDialog.Show( new[] { "*.tmplt" }, false, "Open a Template file" );
		if (string.IsNullOrEmpty(path)) return;

		await LoadTemplate(path);
	}
	
	public async Task LoadTemplate( string path )
	{
		try
		{
			var json = LoadJsonFile(path);
			if (!await CheckIfSavedFirst()) return;
			
			await ClearData();
			
			_model.GddFromJson( json );
			_model.GetValue( TemplateService.GRAPH_TEMPLATE_KEY, out GenericDataDictionary template );
			
			_templateService.LoadTemplate(template);
			
			_settings.AddRecentTemplateFile( path );
			HasUnsavedChanges = false;
			SaveFilePath = null;
		}
		catch(Exception ex)
		{
			await _app.CatchException(ex);
		}
	}


	public async Task PickGraphToLoad()
	{
		try
		{
			string path = await _fileDialog.Show(new[] { "*.ngmap" }, false, "Load Full Graph");
			if (string.IsNullOrEmpty(path)) return;
			
			await LoadGraph(path);
		}
		catch (Exception e)
		{
			await _app.CatchException(e);
		}
	}
	
	public async Task LoadGraph( string path )
	{
		try
		{
			string json = LoadJsonFile( path );
			if (!await CheckIfSavedFirst()) return;
			if (!await CheckIfShouldRestoreFirst(path)) return;
			
			await ClearData();
			
			_model.GddFromJson( json );
			
			if (_model.values.ContainsKey(TemplateService.GRAPH_TEMPLATE_KEY))
			{
				_model.GetValue( TemplateService.GRAPH_TEMPLATE_KEY, out GenericDataDictionary template );
				// todo should update the template name here if successful
				_templateService.LoadTemplate(template);
			}
			else
			{
				// todo should probably skip this popup if no template is currently loaded
				var choice = await _areYouSurePopup.Display(new AreYouSurePopup.AreYouSureArgs(){
					title = "Choose a Template",
					info = "File does not contain a template.\nShould the currently loaded template be used?",
					leftText = "Current",
					middleText = "No Template",
					rightText = "Cancel"
				});

				switch (choice)
				{
					case AreYouSurePopup.Choice.LEFT:
						break;
					case AreYouSurePopup.Choice.MIDDLE:
						// todo Template service needs a clear option and it should be called here
						break;
					case AreYouSurePopup.Choice.RIGHT:
					case AreYouSurePopup.Choice.CANCEL:
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (_model.values.ContainsKey(DataService.GRAPH_DATA_KEY))
			{
				_model.GetValue(DataService.GRAPH_DATA_KEY, out GenericDataDictionary graph);
				_dataService.LoadData(graph);
			}
			else
			{
				_dataService.LoadData(_model);
			}

			SaveFilePath = path;
			HasUnsavedChanges = false;
			_settings.AddRecentFile( path );
		}
		catch(Exception ex)
		{
			await _app.CatchException(ex);
		}
	}
	
	public string LoadJsonFile( string path )
	{
		if( !FileAccess.FileExists( path ) )
		{
			throw new FileNotFoundException($"File '{path}' not found!");
		}
		
		_loadingPath = path;
			
		var file = FileAccess.Open( path, FileAccess.ModeFlags.Read );
			
		string json = file.GetAsText();
		
		file.Dispose();
		
		return json;
	}
	#endregion
}