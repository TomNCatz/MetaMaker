using System;
using Godot;
using LibT.Serialization;
using LibT.Services;

namespace LibT
{
	public class KeyLinkSlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _titlePath;
		private Label _title;
		[Export] private NodePath _fieldPath;
		private Label _field;
		private ServiceInjection<JsonBuilder> _builder = new ServiceInjection<JsonBuilder>();

		
		public override void _Ready()
		{
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
			if( mouseButton != null && !mouseButton.Pressed )
			{
				if( string.IsNullOrEmpty( _field.Text ) ) return;

				_builder.Get.CenterViewOnKeyedNode( _field.Text );
			}
		}

		public bool AddKey(string link)
		{
			if( !string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = link;
			
			return true;
		}

		public bool RemoveKey(string link)
		{
			if( string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = string.Empty;
			
			return true;
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_title.Text = label;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _title.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _title.Text, out string key );

			if( string.IsNullOrEmpty( key ) ) return;
			
			_builder.Get._loadingLinks.Add( new Tuple<KeyLinkSlot, string>( this, key ) );
		}
	}
}