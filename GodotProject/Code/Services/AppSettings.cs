using System.Collections.Generic;
using Godot;
using LibT.Serialization;
using LibT.Services;
using MetaMaker.UI;

namespace MetaMaker.Services;

public class AppSettings
{
	/// <summary>
	/// graph view background color
	/// </summary>
	public Color BackgroundColor
	{
		get => _backgroundColor;
		set => _backgroundColor = value;
	}

	private Color _backgroundColor;
	
	/// <summary>
	/// graph view GridMajor color
	/// </summary>
	public Color GridMajor
	{
		get => _gridMajor;
		set => _gridMajor = value;
	}
	private Color _gridMajor;
	
	/// <summary>
	/// graph view GridMinor color
	/// </summary>
	public Color GridMinor
	{
		get => _gridMinor;
		set => _gridMinor = value;
	}
	private Color _gridMinor;
	
	/// <summary>
	/// show or hide nav area in graph view
	/// </summary>
	public bool ShowGraphNav
	{
		get => _showGraphNav;
		set => _showGraphNav = value;
	}
	private bool _showGraphNav;
	
	/// <summary>
	/// should the app auto save backups while working
	/// </summary>
	public bool AutoBackup
	{
		get => _autoBackup;
		set => _autoBackup = value;
	}
	private bool _autoBackup;
	
	/// <summary>
	/// how often to save a backup in seconds
	/// </summary>
	public float BackupFrequency
	{
		get => _backupFrequency;
		set => _backupFrequency = value;
	}
	private float _backupFrequency;
	
	public List<string> RecentFiles => _recentFiles;
	private List<string> _recentFiles;
	
	public List<string> RecentTemplates => _recentTemplates;
	private List<string> _recentTemplates;

	[Injectable]
	private MainMenuBar _mainMenuBar;
	[Injectable]
	private SaveService _saveService;

	public AppSettings()
	{
		_recentFiles = new List<string>();
		_recentTemplates = new List<string>();
	}
	
	private void GetObjectData(GenericDataDictionary objData)
	{
		objData.AddValue("backgroundColor", _backgroundColor);
		objData.AddValue("gridMajor", _gridMajor);
		objData.AddValue("gridMinor", _gridMinor);
		objData.AddValue("showGraphNav", _showGraphNav);
		objData.AddValue("autoBackup", _autoBackup);
		objData.AddValue("backupFrequency", _backupFrequency);
		objData.AddValue("RecentFiles", _recentFiles);
		objData.AddValue("RecentTemplates", _recentTemplates);
	}

	private void SetObjectData(GenericDataDictionary objData)
	{
		objData.GetValue("backgroundColor", out _backgroundColor);
		BackgroundColor = _backgroundColor;
		objData.GetValue("gridMajor", out _gridMajor);
		GridMajor = _gridMajor;
		objData.GetValue("gridMinor", out _gridMinor);
		GridMinor = _gridMinor;
		objData.GetValue("showGraphNav", out _showGraphNav);
		ShowGraphNav = _showGraphNav;
		objData.GetValue("autoBackup", out _autoBackup);
		AutoBackup = _autoBackup;
		objData.GetValue("backupFrequency", out _backupFrequency);
		BackupFrequency = _backupFrequency;
		
		objData.GetValue("RecentFiles", out _recentFiles);
		for( int i = 0; i < _recentFiles.Count; i++ )
		{
			if( !FileAccess.FileExists( _recentFiles[i] ) )
			{
				_recentFiles.RemoveAt( i );
				i--;
			}
		}
		if( _recentFiles.Count > 0)
		{
			AddRecentFile( _recentFiles[0] );
		}
		
		objData.GetValue("RecentTemplates", out _recentTemplates);
		for( int i = 0; i < _recentTemplates.Count; i++ )
		{
			if( !FileAccess.FileExists( _recentTemplates[i] ) )
			{
				_recentTemplates.RemoveAt( i );
				i--;
			}
		}
		if( _recentTemplates.Count > 0)
		{
			AddRecentTemplateFile( _recentTemplates[0] );
		}
	}
	
	
	public void AddRecentFile( string path )
	{
		if( _recentFiles.Contains( path ) )
		{
			_recentFiles.Remove( path );
		}
		_recentFiles.Insert( 0, path );
			
		_mainMenuBar.UpdateRecentMenu(_recentFiles);
		SaveSettings();
	}
	
	public void AddRecentTemplateFile( string path )
	{
		if( _recentTemplates.Contains( path ) )
		{
			_recentTemplates.Remove( path );
		}
		_recentTemplates.Insert( 0, path );
			
		_mainMenuBar.UpdateRecentTemplateMenu(_recentTemplates);
		SaveSettings();
	}
	
	public void LoadSettings()
	{
		if(!FileAccess.FileExists(SaveService.SETTINGS_SOURCE)) return;
		
		var settings = new GenericDataDictionary();
		var json = _saveService.LoadJsonFile(SaveService.SETTINGS_SOURCE);
		settings.GddFromJson(json);
		SetObjectData(settings);
	}

	public void SaveSettings()
	{
		var settings = new GenericDataDictionary();
		GetObjectData(settings);
		_saveService.SaveJsonFile(SaveService.SETTINGS_SOURCE, settings.GddToJson());
	}
}