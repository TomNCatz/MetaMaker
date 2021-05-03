using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class ColorSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _colorRectPath;
		private ColorRect _colorRect;
		
		private readonly ServiceInjection<MainView> _builder = new ServiceInjection<MainView>();
		private bool asHtml;
		private GenericDataObject _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_colorRect = this.GetNodeFromPath<ColorRect>( _colorRectPath );
			_colorRect.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == 1 && !mouseButton.Pressed)
			{
				_builder.Get.GetColorFromUser(_colorRect.Color)
					.Then(color =>
					{
						_colorRect.Color = color;
						SetColor();
					});
			}
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue( "asHtml", out asHtml );

			GenericDataObject gdo = parentModel.TryGetRelativeGdo(_label.Text);
			if(gdo != null)
			{
				Color color;
				if(gdo.GetValue(out string colorData))
				{
					color = new Color(colorData);
					if(asHtml)
					{
						_model = gdo;
					}
					else
					{
						_model = new GenericDataDictionary();
					}
				}
				else if(gdo.GetValue(out color))
				{
					if(asHtml)
					{
						_model = new GenericDataObject<string>();
					}
					else
					{
						_model = gdo;
					}
				}
				_colorRect.Color = color;
				SetColor();
			}
			else
			{
				template.GetValue( "defaultValue", out Color value );
				_colorRect.Color = value;
				if( asHtml )
				{
					_model = parentModel.TryAddValue( _label.Text, $"#{_colorRect.Color.ToHtml()}" );
				}
				else
				{
					_model = parentModel.TryAddValue( _label.Text, _colorRect.Color );
				}
			}
		}

		private void SetColor()
		{
			if( asHtml )
			{
				_model.CopyFrom(GenericDataObject.CreateGdo($"#{_colorRect.Color.ToHtml()}"));
			}
			else
			{
				_model.CopyFrom(GenericDataObject.CreateGdo(_colorRect.Color));
			}
			OnValueUpdated?.Invoke();
		}
	}
}