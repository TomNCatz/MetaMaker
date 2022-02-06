using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextAreaSlot : Container, IField, ITextSearchable
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private TextEdit _field;
		private GenericDataObject<string> _model;

		public string Text => _field.Text;
		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<TextEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnTextChanged));
			_field.Connect("mouse_entered",this,nameof(OnEnter));
			_field.Connect("mouse_exited",this,nameof(OnExit));
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

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
			
			template.GetValue( "minHeight", out float height );
			_field.RectMinSize = new Vector2(0,height);
		}

		private void OnTextChanged()
		{
			_model.value = _field.Text;
			OnValueUpdated?.Invoke();
		}

		private void OnEnter()
		{
			MainView.ScrollLock = true;
		}

		private void OnExit()
		{
			MainView.ScrollLock = false;
		}
	}
}