using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TypeSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _fieldPath;
		private Label _field;
		[Export] public NodePath _popupPath;
		private PopupDialog _popup;
		[Export] public NodePath _assemblyLabelPath;
		private Label _assemblyLabel;
		[Export] public NodePath _typeLabelPath;
		private Label _typeLabel;

		private string assembly;
		private string type;
		public event System.Action OnValueUpdated;
		
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
			if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
			{
				_popup.Popup_();
				_popup.RectPosition = GetGlobalMousePosition();
			}
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "Class Assembly", out assembly );
			_assemblyLabel.Text = assembly;
			
			template.GetValue( "Class Type", out type );
			
			_field.Text = type.Substring( type.LastIndexOf( '.' ) + 1 );
			_typeLabel.Text = type;

			parentModel.AddValue( "Class Assembly", assembly );
			parentModel.AddValue( "Class Type", type );
		}

		public void GetObjectData( GenericDataArray objData )
		{
		}

		public void SetObjectData( GenericDataArray objData )
		{
		}
	}
}