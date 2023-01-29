using Godot;
using System.Threading.Tasks;
using LibT;

namespace MetaMaker.UI;

public partial class AreYouSurePopup : Window
{
	[Export] public NodePath _infoLabelPath;
	private Label _infoLabel;
	[Export] public NodePath _leftButtonPath;
	private Button _leftButton;
	[Export] public NodePath _middleButtonPath;
	private Button _middleButton;
	[Export] public NodePath _rightButtonPath;
	private Button _rightButton;
	
	public enum Choice
	{
		LEFT,
		MIDDLE,
		RIGHT,
		CANCEL
	}

	private Choice _choice = Choice.CANCEL;
	private AsyncTrigger _waitForUser;
	
	public class AreYouSureArgs
	{
		public string title = "Proceed?";
		public string info = "Are You Sure?";
		public string leftText = null;
		public string middleText = null;
		public string rightText = null;
	}

	public override void _Ready()
	{
		Connect("close_requested", new Callable(this, nameof(CancelPress)));
		
		_infoLabel = this.GetNodeFromPath<Label>(_infoLabelPath);

		_leftButton = this.GetNodeFromPath<Button>(_leftButtonPath);
		_leftButton.Connect("pressed", new Callable(this, nameof(LeftPress)));

		_middleButton = this.GetNodeFromPath<Button>(_middleButtonPath);
		_middleButton.Connect("pressed", new Callable(this, nameof(MiddlePress)));

		_rightButton = this.GetNodeFromPath<Button>(_rightButtonPath);
		_rightButton.Connect("pressed", new Callable(this, nameof(RightPress)));
	}

	public async Task<Choice> Display(AreYouSureArgs args)
	{
		SetSize(new Vector2i(10, 10));

		Title = args.title;
		_infoLabel.Text = args.info;
		_choice = Choice.CANCEL;

		if (!string.IsNullOrEmpty(args.leftText))
		{
			_leftButton.Text = args.leftText;
			_leftButton.Show();
		}
		else
		{
			_leftButton.Hide();
		}

		if (!string.IsNullOrEmpty(args.middleText))
		{
			_middleButton.Text = args.middleText;
			_middleButton.Show();
		}
		else
		{
			_middleButton.Hide();
		}

		if (!string.IsNullOrEmpty(args.rightText))
		{
			_rightButton.Text = args.rightText;
			_rightButton.Show();
		}
		else
		{
			_rightButton.Hide();
		}

		PopupCentered();

		_waitForUser = new AsyncTrigger();
		await _waitForUser.AwaitThis();
		
		return _choice;
	}

	private void LeftPress()
	{
		_choice = Choice.LEFT;
		Hide();
		_waitForUser.Trigger();
	}

	private void MiddlePress()
	{
		_choice = Choice.MIDDLE;
		Hide();
		_waitForUser.Trigger();
	}

	private void RightPress()
	{
		_choice = Choice.RIGHT;
		Hide();
		_waitForUser.Trigger();
	}

	private void CancelPress()
	{
		_choice = Choice.RIGHT;
		Hide();
		_waitForUser.Trigger();
	}
}