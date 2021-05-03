using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class AutoSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "defaultValue", out string text );
			_field.Text = text;
			
			parentModel.TryAddValue(_label.Text, text);
		}
	}
}