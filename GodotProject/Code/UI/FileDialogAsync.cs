using System.Threading.Tasks;
using Godot;
using LibT;

namespace MetaMaker.UI;

public partial class FileDialogAsync : FileDialog
{
	private string _path;
	private AsyncTrigger _waitingForUser;
	public override void _Ready()
	{
		Connect("file_selected", new Callable(this, nameof(OnSelect)));
		Connect("canceled", new Callable(this, nameof(OnCancel)));
		Access = AccessEnum.Filesystem;
	}

	public async Task<string> Show(string[] filters, bool save, string title = null)
	{
		if (string.IsNullOrEmpty(title))
		{
			ModeOverridesTitle = true;
		}
		else
		{
			ModeOverridesTitle = false;
			Title = title;
		}

		if (save)
		{
			FileMode = FileModeEnum.SaveFile;
		}
		else
		{
			FileMode = FileModeEnum.OpenFile;
		}

		Filters = filters;

		PopupCentered();
		_path = null;
		_waitingForUser = new AsyncTrigger();

		await _waitingForUser.AwaitThis();

		return _path;
	}

	private void OnSelect(string path)
	{
		_path = path;
		_waitingForUser.Trigger();
	}

	private void OnCancel()
	{
		_waitingForUser.Trigger();
	}
}
