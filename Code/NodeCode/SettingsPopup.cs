using Godot;
using LibT;
using LibT.Services;

namespace MetaMaker
{
    public class SettingsPopup : ConfirmationDialog
    {
		[Export] public NodePath _backColorRectPath;
		private ColorRect _backColorRect;
		[Export] public NodePath _majorColorRectPath;
		private ColorRect _majorColorRect;
		[Export] public NodePath _minorColorRectPath;
		private ColorRect _minorColorRect;
        [Export] public NodePath _graphNavTogglePath;
        private CheckBox _graphNavToggle;
        [Export] public NodePath _autoTogglePath;
        private CheckBox _autoToggle;
        [Export] public NodePath _autoDelayPath;
        private SpinBox _autoDelay;
        
		private MainView _mainview;

        public override void _Ready()
        {
            _mainview = ServiceInjection<MainView>.Service;
            _backColorRect = this.GetNodeFromPath<ColorRect>( _backColorRectPath );
            _majorColorRect = this.GetNodeFromPath<ColorRect>( _majorColorRectPath );
            _minorColorRect = this.GetNodeFromPath<ColorRect>( _minorColorRectPath );
            _graphNavToggle = this.GetNodeFromPath<CheckBox>( _graphNavTogglePath );
            _autoToggle = this.GetNodeFromPath<CheckBox>( _autoTogglePath );
            _autoDelay = this.GetNodeFromPath<SpinBox>( _autoDelayPath );

			_backColorRect.Connect( "gui_input", this, nameof(GetBackgroundColor) );
			_majorColorRect.Connect( "gui_input", this, nameof(GetMajorColor) );
			_minorColorRect.Connect( "gui_input", this, nameof(GetMinorColor) );

			Connect( "about_to_show", this, nameof(OnPrep) );
			Connect( "confirmed", this, nameof(OnConfirm) );
        }

        private void GetBackgroundColor( InputEvent @event )
        {
            if (@event is InputEventMouseButton mouseButton 
                && mouseButton.ButtonIndex == 1 
                && !mouseButton.Pressed)
			{
                _mainview.GetColorFromUser( _backColorRect.Color )
                    .Then(color => _backColorRect.Color = color);
            }
        }

        private void GetMajorColor( InputEvent @event )
        {
            if (@event is InputEventMouseButton mouseButton 
                && mouseButton.ButtonIndex == 1 
                && !mouseButton.Pressed)
			{
                _mainview.GetColorFromUser( _majorColorRect.Color )
                    .Then(color => _majorColorRect.Color = color);
            }
        }

        private void GetMinorColor( InputEvent @event )
        {
            if (@event is InputEventMouseButton mouseButton 
                && mouseButton.ButtonIndex == 1 
                && !mouseButton.Pressed)
			{
                _mainview.GetColorFromUser( _minorColorRect.Color )
                    .Then(color => _minorColorRect.Color = color);
            }
        }

        public void OnPrep()
        {
            _backColorRect.Color = _mainview.Color;
            _majorColorRect.Color = _mainview.GridMajorColor;
            _minorColorRect.Color = _mainview.GridMinorColor;
            _graphNavToggle.Pressed = _mainview.SettingGraphButtons;
            _autoToggle.Pressed = ServiceInjection<App>.Service.AutoBackup;
            _autoDelay.Value = ServiceInjection<App>.Service.BackupFrequency / 60;
        }

        public void OnConfirm()
        {
            _mainview.Color = _backColorRect.Color;
            _mainview.GridMajorColor = _majorColorRect.Color;
            _mainview.GridMinorColor = _minorColorRect.Color;
            _mainview.SettingGraphButtons = _graphNavToggle.Pressed;
            ServiceInjection<App>.Service.AutoBackup = _autoToggle.Pressed;
            ServiceInjection<App>.Service.BackupFrequency = (float)_autoDelay.Value * 60;
			ServiceInjection<App>.Service.SaveSettings();
        }
    }
}