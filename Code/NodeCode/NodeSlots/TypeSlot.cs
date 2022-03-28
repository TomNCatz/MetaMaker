using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TypeSlot : Container, IField
	{
		[Export] public NodePath _fieldPath;
		private Label _label;
		[Export] public NodePath _popupPath;
		private PopupDialog _popup;
		[Export] public NodePath _assemblyLabelPath;
		private Label _assemblyLabel;
		[Export] public NodePath _typeLabelPath;
		private Label _typeLabel;

		private string assembly;
		private string type;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _fieldPath );
			_label.Connect( "gui_input", this, nameof(_GuiInput) );
			
			
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

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "info", out string info );
			_label.HintTooltip = info;
			_assemblyLabel.HintTooltip = info;
			_typeLabel.HintTooltip = info;
			
			template.GetValue( "Class Assembly", out assembly );
			_assemblyLabel.Text = assembly;
			
			template.GetValue( "Class Type", out type );
			
			_label.Text = type.Substring( type.LastIndexOf( '.' ) + 1 );
			_typeLabel.Text = type;

			parentModel.TryAddValue("Class Assembly", assembly);
			parentModel.TryAddValue("Class Type", type);
		}
	}
}