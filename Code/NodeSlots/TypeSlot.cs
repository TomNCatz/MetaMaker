using System.Collections.Generic;
using System.Linq;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class TypeSlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _fieldPath;
		private Label _field;
		[Export] private NodePath _popupPath;
		private PopupDialog _popup;
		[Export] private NodePath _assemblyLabelPath;
		private Label _assemblyLabel;
		[Export] private NodePath _typeLabelPath;
		private Label _typeLabel;

		private string assembly;
		private string type;
		
		
		public override void _Ready()
		{
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
			
			
			_popup = this.GetNodeFromPath<PopupDialog>( _popupPath );
			_assemblyLabel = this.GetNodeFromPath<Label>( _assemblyLabelPath );
			_typeLabel = this.GetNodeFromPath<Label>( _typeLabelPath );
		}

		public override void _GuiInput( InputEvent @event )
		{
			InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
			if( mouseButton != null && !mouseButton.Pressed )
			{
				_popup.Popup_();
				_popup.RectPosition = GetGlobalMousePosition();
			}
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "Class Assembly", out assembly );
			_assemblyLabel.Text = assembly;
			
			data.GetValue( "Class Type", out type );
			
			_field.Text = type.Substring( type.LastIndexOf( '.' ) + 1 );
			_typeLabel.Text = type;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( "Class Assembly", assembly );
			objData.AddValue( "Class Type", type );
		}

		public void SetObjectData( GenericDataArray objData )
		{
		}
	}
}