using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using LibT.Serialization;
using LibT.Services;
using MetaMaker.UI;

namespace MetaMaker.Services;

public class TemplateService
{
	public const string GRAPH_TEMPLATE_KEY = "template";
	public const string GRAPH_VERSION_KEY = "metaMakerVersion";
	public const string GRAPH_LISTING_KEY = "defaultListing";
	public const string GRAPH_EXPLICIT_KEY = "explicitNode";
	public const string GRAPH_NESTED_COLORS_KEY = "nestingColors";
	public const string GRAPH_KEY_COLORS_KEY = "keyColors";
	public const string GRAPH_EXPORT_DLL_KEY = "reuseDll";
	public const string GRAPH_EXPORT_SCRIPT_KEY = "exportScript";
	public const string GRAPH_NODE_LIST_KEY = "nodeList";
	
	private readonly Dictionary<string, GenericDataDictionary> _nodeData = new Dictionary<string, GenericDataDictionary>();
	private readonly List<Color> _parentChildColors = new List<Color> ();
	private readonly List<Color> _keyColors = new List<Color>();

	public GenericDataDictionary Template { get; private set; }

	[Injectable]
	private MetaMakerApp _app;
	[Injectable]
	private MainMenuBar _mainMenuBar;

	public async Task<bool> LoadTemplate(GenericDataDictionary template)
	{
		try
		{
			await LoadTemplateInternal(template);
			Template = template;
			return true;
		}
		catch (Exception e)
		{
			await _app.CatchException(e);
			// todo return to previous template
			return false;
		}
	}

	private async Task LoadTemplateInternal(GenericDataDictionary template)
	{
		_nodeData.Clear();
		_parentChildColors.Clear();
		_keyColors.Clear();
		
		template.GetValue( GRAPH_VERSION_KEY, out string targetVersion );

		if( targetVersion != _app.VersionDisplay )
		{
			await _app.ShowNotice($"File version is '{targetVersion}' which does not match app version '{_app.VersionDisplay}'");
		}

		template.GetValue( GRAPH_LISTING_KEY, out string defaultListing );
		template.GetValue( GRAPH_EXPLICIT_KEY, out string explicitNode );
		
		if( string.IsNullOrEmpty( defaultListing ) )
		{
			defaultListing = "listing";
		}
		template.GetValue( GRAPH_NODE_LIST_KEY, out GenericDataList listing);

		template.GetValue( GRAPH_NESTED_COLORS_KEY, out List<Color> nestingColors );
		template.GetValue( GRAPH_KEY_COLORS_KEY, out List<Color> keyColors );
		template.GetValue( GRAPH_EXPORT_DLL_KEY, out bool reuseDll );
		template.GetValue( GRAPH_EXPORT_SCRIPT_KEY, out string exportScript );
		
		List<GenericDataObject> dataList = listing.values;
		if( dataList == null )
		{
			throw new NullReferenceException("LoadTemplate does not accept a null dataList");
		}

		template.AddValue( GRAPH_VERSION_KEY, _app.VersionDisplay );

		// if(_loadingPath.Contains("Resources/TEMPLATE.tmplt"))
		// {
		// 	_exportRules = new TemplateExportRules();
		// }
		// else
		// {
		// 	LoadAssembly(exportScript, reuseDll);
		// }
		//
		// if(_exportRules == null)
		// {
		// 	_exportRules = new DefaultExportRules();
		// }

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
			_keyColors.Add( Colors.LightYellow );
			_keyColors.Add( Colors.Cornsilk );
			_keyColors.Add( Colors.Indigo );
			_keyColors.Add( Colors.HotPink );
		}
		
		foreach( GenericDataObject gdo in dataList )
		{
			GenericDataDictionary item = gdo as GenericDataDictionary;
			
			// todo nodes should be tracked by ID, but right now they don't even have IDs
			item.GetValue( "title", out string title );
		
			if( _nodeData.ContainsKey( title ) )
			{
				await _app.ShowNotice( $"Duplicate node title '{title}' found in the template.\nNodes must all have unique titles" );
			}
			else
			{
				_nodeData[title] = item;
			}
		}

		_mainMenuBar.UpdateCreateMenu(_nodeData.Keys.ToList());
		// _mainMenuBar.ResetExportMenu(_exportRules.ExportTargets);
	}
}