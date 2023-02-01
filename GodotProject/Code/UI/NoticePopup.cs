using System.Threading.Tasks;
using Godot;

namespace MetaMaker.UI;

public partial class NoticePopup : AcceptDialog
{
	private bool isShowing;
	
	public override void _Ready()
	{
		AddButton("Copy", true, "Copy");
		
		Connect("custom_action", new Callable(this, nameof(CopyErrorInfo)));
		Connect("confirmed", new Callable(this, nameof(Close)));
		Connect("canceled", new Callable(this, nameof(Close)));
	}

	public async Task Show(string message)
	{
		while (isShowing)
		{
			await Task.Delay(600);
		}
		
		isShowing = true;
		DialogText = message;
		PopupCentered();

		while (isShowing)
		{
			await Task.Delay(500);
		}
	}
	
	public void Close()
	{
		isShowing = false;
	}
	
	public void CopyErrorInfo(string action)
	{
		DisplayServer.ClipboardSet(DialogText);
	}
}