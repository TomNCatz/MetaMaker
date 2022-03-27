using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeyManualSlot : KeyAbstraction
	{
		[Export] public NodePath _prefixLabelPath;
		private Label _prefixLabel;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private GenericDataObject<string> _model;
		
		[Injectable] private App _app;
		[Injectable] private MainView _mainView;

		public override string GetKey => _keyPrefix + _key;
		private string _key;
		private string _keyPrefix;
		public override int LinkType => _linkType;
		private int _linkType;
		private string _lastGoodKey;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_prefixLabel = this.GetNodeFromPath<Label>( _prefixLabelPath );
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnChanged));
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Doubleclick)
			{
				OnChanged(_field.Text);
			}
		}

		public override void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_field.HintTooltip = info;
			
			template.GetValue( "keyPrefix", out _keyPrefix );
			_prefixLabel.Text = _keyPrefix;

			template.GetValue( "slotType", out _linkType );

			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;
				_field.Text = _model.value.Substring(_keyPrefix.Length, _model.value.Length - _keyPrefix.Length);
				OnChanged(_field.Text);
			}
			else
			{
				template.GetValue( "key", out string defaultKey );
				_field.Text = defaultKey;
				_model = parentModel.TryAddValue(_label.Text, string.Empty) as GenericDataObject<string>;
				OnChanged(_field.Text);
			}
		}

		private void OnChanged( string text )
		{
			var problem = TroubleCheck(text);
			_app.RemoveKey(this);

			if (problem)
			{
				_key = text;
				_model.value = GetKey;
				_key = string.Empty;
				return;
			}

			_key = text;
			_model.value = GetKey;
			UpdateLinked();
			_lastGoodKey = GetKey;
			_app.AddKey(this);
			ValueUpdate();
		}

		public override void _ExitTree()
		{
			_app.RemoveKey(this);
			base._ExitTree();
		}

		private void UpdateLinked()
		{
			if (string.IsNullOrEmpty(_lastGoodKey)) return;

			string newKey = GetKey;
			foreach (var keyStore in _mainView.keyStores)
			{
				if(keyStore.LinkType != LinkType) continue;
				if(keyStore.TargetKey != _lastGoodKey) continue;

				keyStore.TargetKey = newKey;
			}
		}
		
		private bool TroubleCheck(string text)
		{
			var problem = string.IsNullOrEmpty(text);
			problem |= _app.ContainsKey(_keyPrefix + text);
			
			IndicateTrouble(problem);
			
			return problem;
		}

		private void IndicateTrouble(bool trouble = true)
		{
			var color = new Color(1,1,1);
			if(trouble)
			{
				color = new Color(1,0,0,0.5f);
			}

			_label.Modulate = color;
			_field.Modulate = color;
		}
	}
}