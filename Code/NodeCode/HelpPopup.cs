using Godot;
using System.Collections.Generic;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class HelpPopup : WindowDialog, IGdaLoadable
	{
		[Export] public NodePath _treePath;
		private Tree _tree;
		[Export] public NodePath _infoPath;
		private Label _info;

		public string Version
		{
			get => _version;
			set
			{
				_version = value;
				WindowTitle = $"Help and Info - V-{_version}_{( OS.IsDebugBuild() ? "DEBUG" : string.Empty )}";
			}
		}

		private string _version;
		
		private readonly Dictionary<string,string> infoLookup = new Dictionary<string, string>();
		
		public override void _Ready()
		{
			_tree = this.GetNodeFromPath<Tree>( _treePath );
			_info = this.GetNodeFromPath<Label>( _infoPath );

			_tree.HideRoot = true;
			_tree.Connect( "item_selected", this, nameof(HandleSelect) );

	//	    var export_config = new ConfigFile();
	//	    var config_error = export_config.Load( "res://export_presets.cfg" );
	//	    object version = export_config.GetValue("preset.0.options","version/name", "failed");
			//object version = ProjectSettings.GetSetting( "version" );
			
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "helpInfo", out List<FaqData> infoData );

			TreeItem root = _tree.CreateItem();
			
			FillTree( root, infoData );
		}

		private void FillTree(TreeItem root, List<FaqData> infoData)
		{
			foreach( var item in infoData )
			{
				TreeItem top = _tree.CreateItem(root);
				top.SetText( 0, item.Entry );
				infoLookup[item.Entry] = item.Info;
				
				FillTree( top, item.Branches );

				top.Collapsed = true;
			}
		}
		
		private void HandleSelect()
		{
			string key = _tree.GetSelected().GetText( 0 );
			if( infoLookup.ContainsKey( key ) )
			{
				_info.Text = infoLookup[key];
			}
			else
			{
				_info.Text = string.Empty;
			}
		}
		
		private class FaqData : IGdoConvertible
		{
			public string Entry;
			public string Info;
			public List<FaqData> Branches;

			public void GetObjectData( GenericDataArray objData )
			{
				throw new System.NotImplementedException();
			}

			public void SetObjectData( GenericDataArray objData )
			{
				objData.GetValue( "Entry", out Entry );
				objData.GetValue( "Info", out Info );
				objData.GetValue( "Branches", out Branches );
			}
		}
	}
}