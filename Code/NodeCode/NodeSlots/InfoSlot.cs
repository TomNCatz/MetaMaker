using Godot;
using LibT.Serialization;

namespace LibT
{
	public class InfoSlot : Container, IGdaLoadable
	{
		[Export] private NodePath _labelPath;
		private RichTextLabel _label;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<RichTextLabel>( _labelPath );
		}
		
		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "minHeight", out float height );
			_label.RectMinSize = new Vector2(0,height);
		}
	}
}