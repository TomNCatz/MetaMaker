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

        [Injectable] private MainView _mainView;
        [Injectable] private App _app;

		public override void _Ready()
        {
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
                _mainView.GetColorFromUser( _backColorRect.Color )
                    .Then(color => _backColorRect.Color = color);
            }
        }

        private void GetMajorColor( InputEvent @event )
        {
            if (@event is InputEventMouseButton mouseButton 
                && mouseButton.ButtonIndex == 1 
                && !mouseButton.Pressed)
			{
                _mainView.GetColorFromUser( _majorColorRect.Color )
                    .Then(color => _majorColorRect.Color = color);
            }
        }

        private void GetMinorColor( InputEvent @event )
        {
            if (@event is InputEventMouseButton mouseButton 
                && mouseButton.ButtonIndex == 1 
                && !mouseButton.Pressed)
			{
                _mainView.GetColorFromUser( _minorColorRect.Color )
                    .Then(color => _minorColorRect.Color = color);
            }
        }

        public void OnPrep()
        {
            _backColorRect.Color = _mainView.Color;
            _majorColorRect.Color = _mainView.GridMajorColor;
            _minorColorRect.Color = _mainView.GridMinorColor;
            _graphNavToggle.Pressed = _mainView.SettingGraphButtons;
            _autoToggle.Pressed = _app.AutoBackup;
            _autoDelay.Value = _app.BackupFrequency / 60;
        }

        public void OnConfirm()
        {
            _mainView.Color = _backColorRect.Color;
            _mainView.GridMajorColor = _majorColorRect.Color;
            _mainView.GridMinorColor = _minorColorRect.Color;
            _mainView.SettingGraphButtons = _graphNavToggle.Pressed;
            _app.AutoBackup = _autoToggle.Pressed;
            _app.BackupFrequency = (float)_autoDelay.Value * 60;
            _app.SaveSettings();
        }
    }
}