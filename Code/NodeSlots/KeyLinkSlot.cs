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
		private EmptyHandling emptyHandling;
		private ServiceInjection<JsonBuilder> _builder = new ServiceInjection<JsonBuilder>();
		private enum EmptyHandling
		{
			EMPTY_STRING,
			SKIP
		}
		
		public override void _Ready()
		{
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
			if( mouseButton != null && mouseButton.Doubleclick )
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
			
			if( data.values.ContainsKey( "emptyHandling" ) )
			{
				data.GetValue( "emptyHandling", out emptyHandling );
			}
		}

		public void GetObjectData( GenericDataArray objData )
		{
			if( string.IsNullOrEmpty( _field.Text ) )
			{
				switch(emptyHandling)
				{
					case EmptyHandling.EMPTY_STRING :
						objData.AddValue( _title.Text, _field.Text );
						break;
					case EmptyHandling.SKIP : 
						break;
					default : throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				objData.AddValue( _title.Text, _field.Text );
			}
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _title.Text, out string key );

			if( string.IsNullOrEmpty( key ) ) return;
			
			_builder.Get.loadingLinks.Add( new Tuple<KeyLinkSlot, string>( this, key ) );
		}
	}
}