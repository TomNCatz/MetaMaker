using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public partial class TextAreaRichSlot : Container, IField, ITextSearchable
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private TextEdit _field;
		[Export] public NodePath _displayPath;
		private RichTextLabel _display;
		private GenericDataObject<string> _model;

		public string Text => _field.Text;
		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<TextEdit>( _fieldPath );
			_display = this.GetNodeFromPath<RichTextLabel>( _displayPath );

			_field.Connect("text_changed",new Callable(this,nameof(OnTextChanged)));
			_field.Connect("mouse_entered",new Callable(this,nameof(OnEnter)));
			_field.Connect("mouse_exited",new Callable(this,nameof(OnExit)));
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.TooltipText = info;
			_field.TooltipText = info;
			
			template.GetValue( "minHeight", out float height );
			_field.CustomMinimumSize = new Vector2(0,height);
			_display.CustomMinimumSize = new Vector2(0,height);

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
			_display.Text = _field.Text;
		}

		private void OnTextChanged()
		{
			_display.Text = _field.Text;
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

		public bool ContainsText(string text)
		{
			return _field.Text.Contains(text);
		}
	}
}