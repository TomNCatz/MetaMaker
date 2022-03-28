using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextLineSlot : Container, IField, ITextSearchable
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private GenericDataObject<string> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		public string Text => _field.Text;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnChanged));
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.HintTooltip = info;
			_field.HintTooltip = info;
			
			template.GetValue( "expandedField", out bool expandedField );
			_label.SizeFlagsHorizontal = expandedField ? (int) SizeFlags.Fill : (int) SizeFlags.ExpandFill;

			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			parentModel.TryGetValue(_label.Text, out GenericDataObject nullToken);
			if(model != null)
			{
				_model = model;
				_field.Text = _model.value;
			}
			else if(nullToken is GenericDataNull)
			{
				parentModel.TryRemoveValue(_label.Text);
				_model = parentModel.TryAddValue(_label.Text, new GenericDataObject<string>()) as GenericDataObject<string>;
				_field.Text = string.Empty;
			}
			else
			{
				template.GetValue( "defaultValue", out string value );
				_model = parentModel.TryAddValue(_label.Text, value) as GenericDataObject<string>;
				_field.Text = value;
			}
		}

		private void OnChanged( string text )
		{
			_model.value = text;
			OnValueUpdated?.Invoke();
		}
	}
}