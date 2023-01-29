using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public partial class InfoSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private RichTextLabel _label;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<RichTextLabel>( _labelPath );
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "minHeight", out float height );
			_label.CustomMinimumSize = new Vector2(0,height);
		}
	}
}