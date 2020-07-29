using Godot;
using LibT.Serialization;

namespace LibT
{
	public class TextAreaSlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private TextEdit _field;
		
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<TextEdit>( _fieldPath );
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "defaultValue", out string text );
			_field.Text = text;
			
			data.GetValue( "minHeight", out float height );
			_field.RectMinSize = new Vector2(0,height);
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out string text );
			_field.Text = text;
		}
	}
}