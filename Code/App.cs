using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using LibT;
using LibT.Maths;
using LibT.Serialization;
using LibT.Services;
using RSG;
using RSG.Exceptions;
using Color = Godot.Color;
using File = Godot.File;
using Vector2 = Godot.Vector2;

namespace MetaMaker
{
	public class App
	{
		public const string SETTINGS_SOURCE = "user://Settings.json";
		
		public const string DEFAULT_TEMPLATE_SOURCE = "res://Resources/TEMPLATE.tmplt";
		public const string HELP_INFO_SOURCE = "res://Resources/HelpInfo.json";
		public const string VERSION = "res://Resources/Version.txt";

		
		#region Variables
		private readonly MainView _mainView;

		public readonly List<Tuple<KeyLinkSlot, string>> loadingLinks = new List<Tuple<KeyLinkSlot, string>>();
		public readonly Dictionary<string, KeySlot> generatedKeys = new Dictionary<string, KeySlot>();
		
		private readonly Dictionary<string, GenericDataArray> _nodeData = new Dictionary<string, GenericDataArray>();
		private readonly List<SlottedGraphNode> _nodes = new List<SlottedGraphNode>();
		private readonly List<Color> _parentChildColors = new List<Color> ();
		private readonly List<Color> _keyColors = new List<Color>();
		public bool includeNodeData = true;
		public bool includeGraphData = true;
		public bool copyingData;
		public bool pastingData;

		private List<string> _recentTemplates = new List<string>();
		private List<string> _recentFiles = new List<string>();
		private string _defaultListing;
		private string _explicitNode;
		private bool _hasUnbackedChanges;
		private GenericDataArray _model;

		public string Version => _version;
		private string _version;
		
		public bool AutoBackup
		{
			set
			{
				_autoBackup = value;
				_mainView.SettingAutoSave = value;
			}
			get => _autoBackup;
		}
		private bool _autoBackup;
		
		public float BackupFrequency
		{
			set
			{
				_backupFrequency = value;
				_mainView.UpdateBackupTimer();
			}
			get => _backupFrequency;
		}
		private float _backupFrequency = 10;

		public string SaveFilePath
		{
			set
			{
				_saveFilePath = value;
				UpdateTitle();
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
				UpdateTitle();
			}
			get => _hasUnsavedChanges;
		}
		private bool _hasUnsavedChanges;
		#endregion

		public App(MainView mainView)
		{
			ServiceProvider.Add(this);

			_mainView = mainView;
		}

		public void Init()
		{
			OS.LowProcessorUsageMode = true;

			_version = LoadJsonFile( VERSION );
				
			LoadSettings();
			LoadDefaultTemplate();
		}
		
		private void LoadSettings()
		{
			File file = new File();
			if( !file.FileExists( SETTINGS_SOURCE ) )return;

			GenericDataArray gda = new GenericDataArray();
			gda.FromJson( LoadJsonFile( SETTINGS_SOURCE ) );
			
			gda.GetValue( "backgroundColor", out Color background );
			_mainView.Color = background;
			gda.GetValue( "gridMajorColor", out Color gridMajor );
			_mainView.GridMajorColor = gridMajor;
			gda.GetValue( "gridMinorColor", out Color gridMinor );
			_mainView.GridMinorColor = gridMinor;

			gda.GetValue( "autoBackup", out bool autoBackup );
			_mainView.SettingAutoSave = autoBackup;
			_autoBackup = autoBackup;

			gda.GetValue( "backupFrequency", out float backupFrequency );
			BackupFrequency = backupFrequency;

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

			if( _recentTemplates.Count == 0 ) return;

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
		
		public void UpdateTitle()
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

		
		
		public void AddRecentFile( string path )
		{
			if( _recentFiles.Contains( path ) )
			{
				_recentFiles.Remove( path );
			}
			_recentFiles.Insert( 0, path );
			
			_mainView.UpdateRecentMenu(_recentFiles);
		}
		
		public void AddRecentTemplateFile( string path )
		{
			if( _recentTemplates.Contains( path ) )
			{
				_recentTemplates.Remove( path );
			}
			_recentTemplates.Insert( 0, path );
			
			_mainView.UpdateRecentTemplateMenu(_recentTemplates);
		}

		public void OnRecentMenuSelection( int id )
		{
			try
			{
				LoadGraph( _recentFiles[id] );
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}

		public void OnRecentTemplateMenuSelection( int id )
		{
			try
			{
				string path = _recentTemplates[id];
				LoadTemplate( LoadJsonFile( path ) );
				AddRecentTemplateFile( path );
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}

		public void OnRecentShiftMenuSelection( int id )
		{
			try
			{
				string path = _recentTemplates[id];
				ShiftTemplateUnderData( path );
				AddRecentTemplateFile( path );
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}
		
		public void CatchException( Exception ex )
		{
			if(ex is PromiseExpectedError) return;
			
			Log.Except( ex );
			_mainView.DisplayErrorPopup(ex);
		}
		
		public GenericDataArray GetNodeData(string index)
		{
			return _nodeData[index];
		}

		public void ClearData()
		{
			generatedKeys.Clear();
			for( int i = _nodes.Count-1; i >= 0; i-- )
			{
				_nodes[i].CloseRequest();
			}
			
			ClearBackup();
			HasUnsavedChanges = false;
			SaveFilePath = null;
		}

		public SlottedGraphNode LoadNode( GenericDataArray nodeContent )
		{
			if( _nodeData.Count == 0 ) throw new KeyNotFoundException("No Nodes currently loaded");

			nodeContent.GetValue( "ngMapNodeName", out string nodeName );

			if( !_nodeData.ContainsKey( nodeName ) )
			{
				throw new ArgumentOutOfRangeException($"'{nodeName}' not found in loaded data");
			}
			
			GenericDataArray nodeData = _nodeData[nodeName];
			
			if( nodeData == null ) throw new ArgumentNullException($"Data for '{nodeName}' was null");

			return _mainView.CreateNode(nodeData, nodeContent);
		}

		public Color GetParentChildColor( int slotType )
		{
			return _parentChildColors[slotType % _parentChildColors.Count];
		}

		public Color GetKeyColor( int slotType )
		{
			return _keyColors[slotType % _keyColors.Count];
		}

		public void SaveSettings()
		{
			GenericDataArray gda = new GenericDataArray();
			gda.AddValue( "RecentFiles", _recentFiles );
			gda.AddValue( "RecentTemplates", _recentTemplates );
			gda.AddValue( "backgroundColor", _mainView.Color );
			gda.AddValue( "gridMajorColor", _mainView.GridMajorColor );
			gda.AddValue( "gridMinorColor", _mainView.GridMinorColor );
			gda.AddValue( "autoBackup", _autoBackup );
			gda.AddValue( "backupFrequency", BackupFrequency );
			
			SaveJsonFile( SETTINGS_SOURCE, gda.ToJson() );
		}


		public void LoadDefaultTemplate()
		{
			LoadTemplate( LoadJsonFile( App.DEFAULT_TEMPLATE_SOURCE ) );

			HasUnsavedChanges = false;
			SaveFilePath = null;
		}
		
		public IPromise SaveGraph()
		{
			if( string.IsNullOrEmpty( SaveFilePath ) )
			{
				return _mainView.SaveGraphAs();
			}

			SaveGraph( SaveFilePath );

			return Promise.Resolved();
		}

		public void SaveBackup()
		{
			if( !_autoBackup ) return;
			if( !_hasUnbackedChanges ) return;
			if( string.IsNullOrEmpty( SaveFilePath ) ) return;
			
			_hasUnbackedChanges = false;
			SaveGraphFile( SaveFilePath + ".back" );
		}

		public void ClearBackup()
		{
			if( string.IsNullOrEmpty( SaveFilePath ) ) return;
			
			File file = new File();
			if( !file.FileExists( SaveFilePath + ".back" ) ) return;

			System.IO.File.Delete(SaveFilePath + ".back");
		}

		public void SaveGraph( string path )
		{
			try
			{
				SaveFilePath = path;
				
				SaveGraphFile(path);

				HasUnsavedChanges = false;
				ClearBackup();
				
				AddRecentFile( path );
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}

		public void SaveGraphFile( string path )
		{
			try
			{
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
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}

		public void LoadGraph( string path )
		{
			CheckIfSavedFirst()
			.Then(() => {
				CheckIfShouldRestoreFirst(path)
				.Then(() =>{
					ClearData();
					string json = LoadJsonFile( path );

					LoadTemplate( json );

					var graph = new GenericDataArray();
					graph.FromJson( json );
					graph.GetValue( "data", out GenericDataArray data );
					LoadData( data );

					HasUnsavedChanges = false;
					SaveFilePath = path;
					AddRecentFile( path );
				});
			});
		}

		public void ShiftTemplateUnderData(string path)
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
		public string LoadJsonFile( string path )
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

		public void SaveJsonFile( string path, string json )
		{
			File file = new File();
			file.Open( path, File.ModeFlags.Write );
			
			file.StoreString( json );
			file.Close();
		}

		public void LoadTemplate( string json )
		{
			CheckIfSavedFirst()
			.Then(() => {
				GenericDataArray gda = new GenericDataArray();
				gda.FromJson( json );

				gda.GetValue( "targetVersion", out string targetVersion );

				if( targetVersion != _version )
				{
					_mainView.ShowNotice($"File version is '{targetVersion}' which does not match app version '{_version}'");
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
			});
		}

		public void LoadTemplate(List<GenericDataObject> dataList, List<Color> nestingColors, List<Color> keyColors )
		{
			if( dataList == null )
			{
				throw new NullReferenceException("LoadTemplate does not accept a null dataList");
			}
			
			ClearData();
			_nodeData.Clear();

			if(nestingColors?.Count > 0)
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

			if(keyColors?.Count > 0)
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
					_mainView.ShowNotice( $"Duplicate node title '{title}' found in the template.\nNodes must all have unique titles" );
				}
				else
				{
					_nodeData[title] = item;
				}
			}

			_mainView.ResetCreateMenu(_nodeData.Keys.ToList());
		}

		public GenericDataArray SaveData()
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

		public void LoadData(GenericDataArray data)
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

			foreach( Tuple<KeyLinkSlot,string> loadingLink in loadingLinks )
			{
				_mainView.LinkToKey( loadingLink.Item1, loadingLink.Item2 );
			}
			
			loadingLinks.Clear();
		}
		
		public IPromise CheckIfSavedFirst()
		{
			if(!HasUnsavedChanges) return Promise.Resolved();
			
			Promise promise = new Promise();

			_mainView.DisplayAreYouSurePopup( new AreYouSurePopup.AreYouSureArgs(){
				title = "Save?",
				info = "Some unsaved changes might be lost\nwould you like to save first?",
				showLeft = true,
				leftText = "Save",
				leftPress = () => SaveGraph().Then(promise.Resolve),
				showMiddle = true,
				middleText = "Continue",
				middlePress = () => { 
					ClearData();
					promise.Resolve();
					},
				showRight = true,
				rightText = "Cancel",
				rightPress = () => promise.Reject(new PromiseExpectedError("cancel"))
			});
			
			return promise;
		}

		private IPromise CheckIfShouldRestoreFirst(string path)
		{
			File file = new File();
			if( !file.FileExists( path + ".back" ) ) return Promise.Resolved();
			
			Promise promise = new Promise();

			_mainView.DisplayAreYouSurePopup( new AreYouSurePopup.AreYouSureArgs(){
				title = "Restore?",
				info = $"A backup of was detected for\n{path}\nwould you like to load it or discard it?",
				width = 600,
				showLeft = true,
				leftText = "Restore",
				leftPress = () => { 
						try
						{
							ClearData();
							string json = LoadJsonFile( path + ".back" );

							LoadTemplate( json );

							var graph = new GenericDataArray();
							graph.FromJson( json );
							graph.GetValue( "data", out GenericDataArray data );
							LoadData( data );

							HasUnsavedChanges = true;
							SaveFilePath = path;
							AddRecentFile( path );
						}
						catch( Exception e )
						{
							CatchException( e );
						}
					},
				showMiddle = true,
				middleText = "Discard",
				middlePress = () => { 
					SaveFilePath = path;
					ClearData();
					promise.Resolve();
					},
				showRight = true,
				rightText = "Cancel",
				rightPress = () => promise.Reject(new PromiseExpectedError("cancel"))
			});
			
			return promise;
		}
	}
}