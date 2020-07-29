using System.Linq;
using Godot;
using LibT.Serialization;

namespace LibT
{
	public class TextLineSlot : Container, IGdaLoadable, IGdoConvertible
	{
		[Export] private NodePath _labelPath;
		private Label _label;
		[Export] private NodePath _fieldPath;
		private LineEdit _field;
		
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "defaultValue", out string text );
			_field.Text = text;
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