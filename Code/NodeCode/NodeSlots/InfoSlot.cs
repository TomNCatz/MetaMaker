using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class InfoSlot : Container, IField
	{
		[Export] private readonly NodePath _labelPath;
		private RichTextLabel _label;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<RichTextLabel>( _labelPath );
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "minHeight", out float height );
			_label.RectMinSize = new Vector2(0,height);
		}
	}
}