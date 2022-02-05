using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using MetaMakerLib;
using LibT;
using LibT.Serialization;
using LibT.Services;
using RSG;
using RSG.Exceptions;
using Color = Godot.Color;
using File = Godot.File;
using System.Threading.Tasks;

namespace MetaMaker
{
	public class App : LibT.Services.App
	{
		#region Const Values
		public const string SETTINGS_SOURCE = "user://Settings.json";
		public const string DEFAULT_TEMPLATE_SOURCE = "res://Resources/TEMPLATE.tmplt";
		public const string HELP_INFO_SOURCE = "res://Resources/HelpInfo.json";
		public const string VERSION = "res://Resources/Version.txt";

		public const string NODE_NAME_KEY = "ngMapNodeName";
		public const string NODE_POSITION_KEY = "ngMapNodePosition";
		public const string NODE_SIZE_KEY = "ngMapNodeSize";
		
		public const string GRAPH_VERSION_KEY = "metaMakerVersion";
		public const string GRAPH_LISTING_KEY = "defaultListing";
		public const string GRAPH_EXPLICIT_KEY = "explicitNode";
		public const string GRAPH_NESTED_COLORS_KEY = "nestingColors";
		public const string GRAPH_KEY_COLORS_KEY = "keyColors";
		public const string GRAPH_EXPORT_DLL_KEY = "reuseDll";
		public const string GRAPH_EXPORT_SCRIPT_KEY = "exportScript";
		public const string GRAPH_NODE_LIST_KEY = "nodeList";
		public const string GRAPH_DATA_KEY = "data";
		#endregion
		
		#region Variables
		[Export] public NodePath _mainViewPath;
		private MainView _mainView;
		[Export] public NodePath _backupTimerPath;
		private Timer _backupTimer;
		[Export] public NodePath _helpInfoPopupPath;
		private HelpPopup _helpInfoPopup;
		[Export] public NodePath _settingsPath;
		private SettingsPopup _settingsPopup;

		public readonly Dictionary<string, KeyAbstraction> generatedKeys = new Dictionary<string, KeyAbstraction>();
		public readonly Dictionary<int, List<string>> keySearch = new Dictionary<int, List<string>>();
		public readonly List<ITextSearchable> textSearch = new List<ITextSearchable>();
		
		private readonly Dictionary<string, GenericDataDictionary> _nodeData = new Dictionary<string, GenericDataDictionary>();
		private readonly List<Color> _parentChildColors = new List<Color> ();
		private readonly List<Color> _keyColors = new List<Color>();
		public bool pastingData;

		private List<string> _recentTemplates = new List<string>();
		private List<string> _recentFiles = new List<string>();
		private string _defaultListing;
		private string _explicitNode;
		private bool _hasUnbackedChanges;
		private GenericDataDictionary _model;
		private readonly List<GenericDataDictionary> _topModels = new List<GenericDataDictionary>();
		private ExportRulesAbstract _exportRules;
		private string _loadingPath;

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
				_backupTimer.WaitTime = _backupFrequency;
			}
			get => _backupFrequency;
		}
		private float _backupFrequency = 300;
		
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
				if(!_hasUnsavedChanges)
				{
					_mainView.SetNodesClean();
					_mainView.ClearScrollLock();
				}
			}
			get => _hasUnsavedChanges;
		}
		private bool _hasUnsavedChanges;
		#endregion

		#region Setup
		public override Task Bindings(Context context)
		{
			OS.LowProcessorUsageMode = true;
				
			GetTree().SetAutoAcceptQuit(false);
			
			_mainView = this.GetNodeFromPath<MainView>( _mainViewPath );
			context.Set<MetaMaker.App>(this);
			
			context.Inject(_mainView);
			context.Set(_mainView);
			
			_model = new GenericDataDictionary();

			_backupTimer = this.GetNodeFromPath<Timer>( _backupTimerPath );
			_backupTimer.WaitTime = BackupFrequency;
			_backupTimer.Connect("timeout", this, nameof(SaveBackup));
			_backupTimer.Start();
			
			_helpInfoPopup = this.GetNodeFromPath<HelpPopup>( _helpInfoPopupPath );
			_helpInfoPopup.Version = Version;
			GenericDataDictionary helpData = new GenericDataDictionary();
			helpData.FromJson(LoadJsonFile(App.HELP_INFO_SOURCE));
			_helpInfoPopup.SetObjectData(helpData);
			
			_settingsPopup = this.GetNodeFromPath<SettingsPopup>( _settingsPath );
			context.Inject(_settingsPopup);
			
			return Task.CompletedTask;
		}

		public override Task StartApp()
		{
			_version = LoadJsonFile( VERSION );
				
			LoadSettings();
			LoadDefaultTemplate();
			
			return Task.CompletedTask;
		}

		public void OpenSettings()
		{
			_settingsPopup.PopupCentered();
		}
		
		public void OpenHelpPopup()
		{
			_helpInfoPopup.PopupCentered();
		}
		
		private void LoadSettings()
		{
			File file = new File();
			if( !file.FileExists( SETTINGS_SOURCE ) )return;

			GenericDataDictionary gda = new GenericDataDictionary();
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

				if( _recentFiles.Count > 0)
				{
					AddRecentFile( _recentFiles[0] );
				}
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

			if( _recentTemplates.Count > 0)
			{
				AddRecentTemplateFile( _recentTemplates[0] );
			}
		}
		#endregion

		#region Reactions
		public void UpdateTitle()
		{
			var title = "MetaMaker";
			if( !string.IsNullOrEmpty( SaveFilePath ) )
			{
				title += " - " + SaveFilePath;
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

		public async Task ClearData()
		{
			_model.values.Clear();
			generatedKeys.Clear();
			keySearch.Clear();

			for( int i = _mainView.nodes.Count-1; i >= 0; i-- )
			{
				_mainView.nodes[i].CloseRequest();
			}
			
			_mainView.CurrentParentIndex = -1;
			_nodeData.Clear();
			ClearBackup();
			HasUnsavedChanges = false;
			SaveFilePath = null;
			_exportRules = null;
			await Task.Delay(100);
		}

		public void SaveSettings()
		{
			GenericDataDictionary gda = new GenericDataDictionary();
			gda.AddValue( "RecentFiles", _recentFiles );
			gda.AddValue( "RecentTemplates", _recentTemplates );
			gda.AddValue( "backgroundColor", _mainView.Color );
			gda.AddValue( "gridMajorColor", _mainView.GridMajorColor );
			gda.AddValue( "gridMinorColor", _mainView.GridMinorColor );
			gda.AddValue( "autoBackup", _autoBackup );
			gda.AddValue( "backupFrequency", BackupFrequency );
			
			SaveJsonFile( SETTINGS_SOURCE, gda.ToJson() );
		}

		public void AddNode(SlottedGraphNode node)
		{
			if( !node.LinkedToParent )
			{
				_topModels.Add( node.Model );
				UpdateData();
			}
		}

		public void RemoveNode(SlottedGraphNode node)
		{
			if(_topModels.Contains(node.Model))
			{
				_topModels.Remove(node.Model);
				UpdateData();
			}
		}
		
		public void CatchException( Exception ex )
		{
			if(ex is PromiseExpectedError) return;
			
			Log.Except( ex );
			_mainView.DisplayErrorPopup(ex);
		}
		
		private void UpdateData()
		{
			GenericDataDictionary gda;

			if( _topModels.Count == 1 )
			{
				gda = _topModels[0];
			}
			else
			{
				gda = new GenericDataDictionary();

				gda.AddValue( _defaultListing, _topModels );
			}

			_model.AddValue(GRAPH_DATA_KEY, gda);
		}

		public void ClearBackup()
		{
			if( string.IsNullOrEmpty( SaveFilePath ) ) return;
			
			File file = new File();
			if( !file.FileExists( SaveFilePath + ".back" ) ) return;

			System.IO.File.Delete(SaveFilePath + ".back");
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
				middlePress = async () => { 
					await ClearData();
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
				leftPress = async () => { 
						try
						{
							await ClearData();
							string json = LoadJsonFile( path + ".back" );

							LoadTemplate( json );

							var graph = new GenericDataDictionary();
							graph.FromJson( json );
							graph.GetValue( GRAPH_DATA_KEY, out GenericDataDictionary data );
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
				middlePress = async () => { 
					SaveFilePath = path;
					await ClearData();
					promise.Resolve();
					},
				showRight = true,
				rightText = "Cancel",
				rightPress = () => promise.Reject(new PromiseExpectedError("cancel"))
			});
			
			return promise;
		}
		#endregion

		#region Get Data
		public GenericDataDictionary GetNodeData(string index)
		{
			return _nodeData[index];
		}

		public SlottedGraphNode LoadNode( GenericDataDictionary nodeContent )
		{
			if( _nodeData.Count == 0 ) throw new KeyNotFoundException("No Nodes currently loaded");

			nodeContent.GetValue( NODE_NAME_KEY, out string nodeName );
			//Log.Error($"Node name {nodeName}");

			if( !_nodeData.ContainsKey( nodeName ) )
			{
				throw new ArgumentOutOfRangeException($"'{nodeName}' not found in loaded data");
			}
			
			GenericDataDictionary nodeData = _nodeData[nodeName];
			
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
		#endregion

		#region Key links
		public string GenerateKey(int keySize, string keyPrefix)
		{
			return Tool.GetUniqueKey( keySize, generatedKeys.ContainsKey, 5, keyPrefix );
		}

		public void AddKey(KeyAbstraction slot)
		{
			generatedKeys[slot.GetKey] = slot;

			if(!keySearch.ContainsKey(slot.LinkType))
			{
				keySearch[slot.LinkType] = new List<string>();
			}

			keySearch[slot.LinkType].Add(slot.GetKey);
		}

		public void RemoveKey(KeyAbstraction slot)
		{
			if(string.IsNullOrEmpty(slot.GetKey)) return;
			if( !generatedKeys.ContainsKey( slot.GetKey ) ) return;
			
			generatedKeys.Remove( slot.GetKey );
			keySearch[slot.LinkType].Remove(slot.GetKey);
		}

		public bool ContainsKey(string key)
		{
			return generatedKeys.ContainsKey(key);
		}

		public List<string> AvailableKeys(int slotType)
		{
			if(keySearch.ContainsKey(slotType))
			{
				return keySearch[slotType];
			}
			return new List<string>();
		}
 		#endregion

		#region Save Flow
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

			try
			{
				SaveJsonFile( SaveFilePath + ".back", _model.DataCopy().ToJson() );
			}	
			catch(Exception e)
			{
				CatchException(e);
			}
		}

		public void SaveGraph( string path )
		{
			try
			{
				SaveFilePath = path;
				
				SaveJsonFile( SaveFilePath, _model.DataCopy().ToJson() );

				HasUnsavedChanges = false;
				ClearBackup();
			}
			catch( Exception e )
			{
				CatchException( e );
			}
		}

		public void ExportAll()
		{
			foreach(string exportName in _exportRules.ExportTargets.Keys)
			{
				ExportSet(exportName);
			}
		}

		public void ExportSet(string exportName)
		{
			if(string.IsNullOrEmpty(SaveFilePath)) throw new Exception("Relative export requires the graph to have been saved first.");

			_exportRules.graphVersionKey = GRAPH_VERSION_KEY;
			_exportRules.metaMakerVersion = Version;

			ExportSet exportSet = _exportRules.ExportTargets[exportName];

			if (!(_model.GetRelativeGdo(exportSet.gdoExportTarget) is GenericDataDictionary data)) 
			{
				throw new Exception($"Object at path {exportSet.gdoExportTarget} is not an array");
			}

			data = data.DataCopy();

			string path = exportSet.relativeSavePath.Replace("$name", SaveFilePath.PathGetFileName());
			path = path.Replace("$export",exportName);
			path = SaveFilePath.PathGetContainingFolder() + '/' + path;

			int fileCount = 1;
			int itemCount = exportSet.childCount;
			int current = 0;
			if(exportSet.childCount > 0)
			{
				fileCount = (int)Math.Ceiling((float)data.values.Count / exportSet.childCount);
			}
			else
			{
				itemCount = data.values.Count;
			}

			var items = new List<GenericDataDictionary>();
			var graph = new GenericDataDictionary(){type = data.type};

			foreach (var pair in data.values)
			{
				if(current == itemCount)
				{
					items.Add(graph);
					graph = new GenericDataDictionary(){type = data.type};
					current = 0;
				}
				graph.AddValue(pair.Key, pair.Value);
				current++;
			}
			items.Add(graph);

			for (int i = 0; i < fileCount; i++)
			{
				string myPath = path.Replace("$index",$"{i+1}");
				myPath = System.IO.Path.GetFullPath(myPath);
				if(myPath.Contains(".."))
				{
					CatchException(new Exception($"Export failed at path '{myPath}'. Check the export rules and try again."));
				}
				
				List<string> keys = new List<string>
				{
					NODE_NAME_KEY,
					NODE_POSITION_KEY,
					NODE_SIZE_KEY
				};

				_exportRules.PreprocessExportGdo(items[i], keys, exportName, myPath, i+1);

				if(keys != null)
				{
					items[i].RecursivelyRemoveKeys(keys);
				}
				
				string json = items[i].ToJson();

				json = _exportRules.PostprocessExportJSON(json, exportName, myPath, i+1);
				
				SaveJsonFile( myPath, json );
			}
		}

		public void SaveJsonFile( string path, string json )
		{
			try
			{
				if(!path.Contains("user:")&&!path.Contains("res:"))
				{
					Serializer.CreatePathIfMissing( path.PathGetContainingFolder() );
				}

				File file = new File();
				file.Open( path, File.ModeFlags.Write );
				
				file.StoreString( json );
				file.Close();

				
				if(path.Contains(".tmplt"))
				{
					AddRecentTemplateFile( path );
				}
				if(!path.Contains(".back") && path.Contains(".ngmap"))
				{
					AddRecentFile( path );
				}
			}
			catch(Exception ex)
			{
				CatchException(ex);
			}
		}
		#endregion

		#region Load Flow
		public void LoadDefaultTemplate()
		{
			LoadTemplate( LoadJsonFile( DEFAULT_TEMPLATE_SOURCE ) );

			HasUnsavedChanges = false;
			SaveFilePath = null;
		}

		public void LoadGraph( string path )
		{
			CheckIfSavedFirst()
			.Then(() => {
				CheckIfShouldRestoreFirst(path)
				.Then(async () =>{
					try
					{
						await ClearData();
						string json = LoadJsonFile( path );

						LoadTemplateInternal(json);

						var graph = new GenericDataDictionary();
						graph.FromJson( json );
						graph.GetValue( GRAPH_DATA_KEY, out GenericDataDictionary data );
						LoadData( data );

						SaveFilePath = path;
						HasUnsavedChanges = false;
						AddRecentFile( path );
					}
					catch(Exception ex)
					{
						CatchException(ex);
					}
				});
			});
		}

		public void LoadData(GenericDataDictionary data)
		{
			if(data == null)
			{
				throw new Exception("Data in file was corrupted or missing.");
			}

			if( data.values.ContainsKey( NODE_NAME_KEY ) )
			{
				LoadNode( data );
			}
			else if(data.values.Count == 1)
			{
				string key = data.values.Keys.First();
				
				data.GetValue( key, out List<GenericDataDictionary> items );

				foreach( GenericDataDictionary item in items )
				{
					if( !item.values.ContainsKey( NODE_NAME_KEY ) )
					{
						item.AddValue( NODE_NAME_KEY, _explicitNode );
					}
					LoadNode( item );
				}
			}
			else
			{
				data.AddValue( NODE_NAME_KEY, _explicitNode );
				LoadNode( data );
			}

			foreach( Tuple<KeyLinkSlot,string> loadingLink in _mainView.loadingLinks )
			{
				_mainView.LinkToKey( loadingLink.Item1, loadingLink.Item2 );
			}
			
			_mainView.loadingLinks.Clear();
		}

		public void ShiftTemplateUnderData(string path)
		{
			CheckIfSavedFirst()
			.Then(async () => {
				try
				{
					_model.GetValue( GRAPH_DATA_KEY, out GenericDataDictionary data );
					data = data.DataCopy();
					await ClearData();
					LoadTemplate( LoadJsonFile( path ) )
					.Then(() => {
						LoadData( data );
						AddRecentTemplateFile( path );
					})
					.Catch(CatchException);

					
				}
				catch( Exception ex )
				{
					CatchException( ex );
				}
			});
		}
		
		public string LoadJsonFile( string path )
		{
			File file = new File();
			if( !file.FileExists( path ) )
			{
				throw new FileNotFoundException($"File '{path}' not found!");
			}

			_loadingPath = path;
			
			file.Open( path, File.ModeFlags.Read );
			
			string json = file.GetAsText();
			file.Close();

			return json;
		}

		public IPromise LoadTemplate( string json )
		{
			var promise = new Promise();
			CheckIfSavedFirst()
			.Then(async () => {
				try
				{
					await ClearData();
					LoadTemplateInternal(json);
					promise.Resolve();
				}
				catch(Exception ex)
				{
					CatchException(ex);
					promise.Reject(ex);
				}
			});
			return promise;
		}

		private void LoadTemplateInternal( string json )
		{
			GenericDataDictionary gda = new GenericDataDictionary();
			gda.FromJson( json );

			gda.GetValue( GRAPH_VERSION_KEY, out string targetVersion );

			if( targetVersion != _version )
			{
				_mainView.ShowNotice($"File version is '{targetVersion}' which does not match app version '{_version}'");
			}

			gda.GetValue( GRAPH_LISTING_KEY, out _defaultListing );
			gda.GetValue( GRAPH_EXPLICIT_KEY, out _explicitNode );
			
			if( string.IsNullOrEmpty( _defaultListing ) )
			{
				_defaultListing = "listing";
			}
			gda.GetValue( GRAPH_NODE_LIST_KEY, out GenericDataList listing);

			gda.GetValue( GRAPH_NESTED_COLORS_KEY, out List<Color> nestingColors );
			gda.GetValue( GRAPH_KEY_COLORS_KEY, out List<Color> keyColors );
			gda.GetValue( GRAPH_EXPORT_DLL_KEY, out bool reuseDll );
			gda.GetValue( GRAPH_EXPORT_SCRIPT_KEY, out string exportScript );
			
			List<GenericDataObject> dataList = listing.values;
			if( dataList == null )
			{
				throw new NullReferenceException("LoadTemplate does not accept a null dataList");
			}

			_model.AddValue( GRAPH_VERSION_KEY, _version );
			_model.AddValue( GRAPH_LISTING_KEY, _defaultListing );
			_model.AddValue( GRAPH_EXPLICIT_KEY, _explicitNode );
			_model.AddValue( GRAPH_EXPORT_DLL_KEY, reuseDll );
			_model.AddValue( GRAPH_EXPORT_SCRIPT_KEY, exportScript );
			_model.AddValue( GRAPH_NESTED_COLORS_KEY, nestingColors );
			_model.AddValue( GRAPH_KEY_COLORS_KEY, keyColors );

			if(_loadingPath.Contains("Resources/TEMPLATE.tmplt"))
			{
				_exportRules = new TemplateExportRules();
			}
			else
			{
				LoadAssembly(exportScript, reuseDll);
			}

			if(_exportRules == null)
			{
				_exportRules = new DefaultExportRules();
			}

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
				GenericDataDictionary item = gdo as GenericDataDictionary;
				
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

			List<GenericDataDictionary> nodeList = _nodeData.Values.ToList();
			_model.AddValue( GRAPH_NODE_LIST_KEY, nodeList );

			_mainView.ResetCreateMenu(_nodeData.Keys.ToList());
			_mainView.ResetExportMenu(_exportRules.ExportTargets);
		}

		private void LoadAssembly(string exportScript, bool reuseDll)
		{
			System.Reflection.Assembly assembly = null;

			string dllPath = _loadingPath.Substring(0,_loadingPath.LastIndexOf('.')) + ".dll";

			File file = new File();
			if(reuseDll && file.FileExists( dllPath ) )
			{
				try
				{
					assembly = System.Reflection.Assembly.LoadFile(dllPath);
				}
				catch(Exception ex)
				{
					CatchException(ex);
				}
			}

			if(assembly == null && !string.IsNullOrEmpty(exportScript))
			{
				try
				{
					var args = new AssemblyCompileArgs();
					if(reuseDll)
					{
						args.exportPath = dllPath;
					}
					args.references.Add(SharpMods.GetPortableReferenceToType(typeof(Dictionary<,>)));
					args.references.Add(SharpMods.GetPortableReferenceToType(typeof(ExportRulesAbstract)));
					args.references.Add(SharpMods.GetPortableReferenceToType(typeof(GenericDataDictionary)));
					args.code = exportScript;
					assembly = SharpMods.RoslynRuntimeCompile(args);
				}
				catch(Exception ex)
				{
					CatchException(ex);
				}
			}

			if(assembly != null)
			{
				var type = assembly.GetType("Rules.ExportRules");

				var obj = Activator.CreateInstance(type, null);
				_exportRules = obj as ExportRulesAbstract;
			}
		}
		#endregion

	}
}