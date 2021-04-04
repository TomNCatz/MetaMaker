using Godot;
using System;
using LibT;

public class AreYouSurePopup : WindowDialog
{
	[Export] private NodePath _infoLabelPath;
	private Label _infoLabel;
	[Export] private NodePath _leftButtonPath;
	private Button _leftButton;
	[Export] private NodePath _middleButtonPath;
	private Button _middleButton;
	[Export] private NodePath _rightButtonPath;
	private Button _rightButton;

	private event Action LeftPressed;
	private event Action MiddlePressed;
	private event Action RightPressed;

	public class AreYouSureArgs
	{
		public string title = "Proceed?";
		public string info = "Are You Sure?";
		public int width = 270;
		public int height = 100;
		public bool showLeft = false;
		public bool showMiddle = false;
		public bool showRight = false;
		public string leftText = "OK";
		public string middleText = "No";
		public string rightText = "Cancel";
		public Action leftPress = null;
		public Action middlePress = null; 
		public Action rightPress = null;
	}

	public override void _Ready()
    {
	    _infoLabel = this.GetNodeFromPath<Label>( _infoLabelPath );

	    _leftButton = this.GetNodeFromPath<Button>( _leftButtonPath );
	    _leftButton.Connect( "pressed", this, nameof(LeftPress) );
	    
	    _middleButton = this.GetNodeFromPath<Button>( _middleButtonPath );
	    _middleButton.Connect( "pressed", this, nameof(MiddlePress) );
	    
	    _rightButton = this.GetNodeFromPath<Button>( _rightButtonPath );
	    _rightButton.Connect( "pressed", this, nameof(RightPress) );
    }

	public void Display( AreYouSureArgs args )
	{
		SetSize(new Vector2(args.width, args.height));
		
		WindowTitle = args.title;
		_infoLabel.Text = args.info;

		LeftPressed = args.leftPress;
		MiddlePressed = args.middlePress;
		RightPressed = args.rightPress;

		if( args.showLeft )
		{
			_leftButton.Text = args.leftText;
			_leftButton.Show();
		}
		else
		{
			_leftButton.Hide();
		}
		
		if( args.showMiddle )
		{
			_middleButton.Text = args.middleText;
			_middleButton.Show();
		}
		else
		{
			_middleButton.Hide();
		}
		
		if( args.showRight )
		{
			_rightButton.Text = args.rightText;
			_rightButton.Show();
		}
		else
		{
			_rightButton.Hide();
		}
		
		PopupCentered();
	}

	private void LeftPress()
    {
		Action current = LeftPressed;
	    Clear();
		current?.Invoke();
    }
    
    private void MiddlePress()
    {
		Action current = MiddlePressed;
	    Clear();
		current?.Invoke();
    }
    
    private void RightPress()
    {
		Action current = RightPressed;
	    Clear();
		current?.Invoke();
    }

    private void Clear()
    {
	    Hide();
	    LeftPressed = null;
	    MiddlePressed = null;
	    RightPressed = null;
    }
}
