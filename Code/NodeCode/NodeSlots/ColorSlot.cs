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

		private GenericDataArray _parentModel;
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
						SetColor(_parentModel);
					});
			}
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue( "asHtml", out asHtml );

			_parentModel = parentModel;
			if(parentModel.values.ContainsKey(_label.Text))
			{
				Color color;
				if (asHtml)
				{
					parentModel.GetValue(_label.Text, out string colorData);
					color = new Color(colorData);
				}
				else
				{
					parentModel.GetValue(_label.Text, out color);
				}
				_colorRect.Color = color;
			}
			else
			{
				template.GetValue( "defaultValue", out Color value );
				_colorRect.Color = value;
				_parentModel.AddValue(_label.Text, value);
				SetColor(_parentModel);
			}
		}

		private void SetColor(GenericDataArray parent)
		{
			if( asHtml )
			{
				parent.AddValue( _label.Text, $"#{_colorRect.Color.ToHtml()}" );
			}
			else
			{
				parent.AddValue( _label.Text, _colorRect.Color );
			}
			OnValueUpdated?.Invoke();
		}
	}
}