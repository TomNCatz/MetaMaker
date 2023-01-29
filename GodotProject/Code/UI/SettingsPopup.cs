using Godot;
using LibT;
using LibT.Services;
using MetaMaker.Services;

namespace MetaMaker.UI;

public partial class SettingsPopup : ConfirmationDialog
{
	[Export] public NodePath _backColorRectPath;
	private ColorPickerButton _backColorRect;
	[Export] public NodePath _majorColorRectPath;
	private ColorPickerButton _majorColorRect;
	[Export] public NodePath _minorColorRectPath;
	private ColorPickerButton _minorColorRect;
	[Export] public NodePath _graphNavTogglePath;
	private CheckBox _graphNavToggle;
	[Export] public NodePath _autoTogglePath;
	private CheckBox _autoToggle;
	[Export] public NodePath _autoDelayPath;
	private SpinBox _autoDelay;

	[Injectable] private AppSettings _settings;

	public override void _Ready()
	{
		_backColorRect = this.GetNodeFromPath<ColorPickerButton>(_backColorRectPath);
		_majorColorRect = this.GetNodeFromPath<ColorPickerButton>(_majorColorRectPath);
		_minorColorRect = this.GetNodeFromPath<ColorPickerButton>(_minorColorRectPath);
		_graphNavToggle = this.GetNodeFromPath<CheckBox>(_graphNavTogglePath);
		_autoToggle = this.GetNodeFromPath<CheckBox>(_autoTogglePath);
		_autoDelay = this.GetNodeFromPath<SpinBox>(_autoDelayPath);

		_backColorRect.Connect("color_changed", new Callable(this, nameof(GetBackgroundColor)));
		_majorColorRect.Connect("color_changed", new Callable(this, nameof(GetMajorColor)));
		_minorColorRect.Connect("color_changed", new Callable(this, nameof(GetMinorColor)));
		_graphNavToggle.Connect("toggled", new Callable(this, nameof(GetNavToggle)));
		_autoToggle.Connect("toggled", new Callable(this, nameof(GetAutoToggle)));
		_autoDelay.Connect("value_changed", new Callable(this, nameof(GetAutoDelay)));

		Connect("about_to_popup", new Callable(this, nameof(OnPrep)));
		Connect("confirmed", new Callable(this, nameof(OnConfirm)));
		Connect("cancelled", new Callable(this, nameof(OnCancel)));
	}

	private void GetBackgroundColor(Color color)
	{
		_settings.BackgroundColor = color;
	}

	private void GetMajorColor(Color color)
	{
		_settings.GridMajor = color;
	}

	private void GetMinorColor(Color color)
	{
		_settings.GridMinor = color;
	}

	private void GetNavToggle(bool toggle)
	{
		_settings.ShowGraphNav = toggle;
	}

	private void GetAutoToggle(bool toggle)
	{
		_settings.AutoBackup = toggle;
	}

	private void GetAutoDelay(float amount)
	{
		_settings.BackupFrequency = amount * 60;
	}

	public void OnPrep()
	{
		_backColorRect.Color = _settings.BackgroundColor;
		_majorColorRect.Color = _settings.GridMajor;
		_minorColorRect.Color = _settings.GridMinor;
		_graphNavToggle.SetPressedNoSignal(_settings.ShowGraphNav);
		_autoToggle.SetPressedNoSignal(_settings.AutoBackup);
		_autoDelay.Value = _settings.BackupFrequency / 60;
	}

	public void OnConfirm()
	{
		_settings.SaveSettings();
	}

	public void OnCancel()
	{
		_settings.LoadSettings();
	}
}
