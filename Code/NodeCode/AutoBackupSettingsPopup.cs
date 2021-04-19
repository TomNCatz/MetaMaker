using Godot;
using LibT;
using LibT.Services;
using System;

namespace MetaMaker
{
    public class AutoBackupSettingsPopup : ConfirmationDialog
    {
        [Export] public NodePath _autoTogglePath;
        private CheckBox _autoToggle;
        [Export] public NodePath _autoDelayPath;
        private SpinBox _autoDelay;
        
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();

        public override void _Ready()
        {
            _autoToggle = this.GetNodeFromPath<CheckBox>( _autoTogglePath );
            _autoDelay = this.GetNodeFromPath<SpinBox>( _autoDelayPath );
			Connect( "about_to_show", this, nameof(OnPrep) );
			Connect( "confirmed", this, nameof(OnConfirm) );
        }

        public void OnPrep()
        {
            _autoToggle.Pressed = _app.Get.AutoBackup;
            _autoDelay.Value = _app.Get.BackupFrequency / 60;
        }

        public void OnConfirm()
        {
            _app.Get.AutoBackup = _autoToggle.Pressed;
            _app.Get.BackupFrequency = (float)_autoDelay.Value * 60;
			_app.Get.SaveSettings();
        }
    }
}