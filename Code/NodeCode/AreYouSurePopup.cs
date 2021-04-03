using Godot;
using System;
using LibT;

namespace MetaMaker
{
	public class AreYouSurePopup : WindowDialog
	{
		[Export] private readonly NodePath _leftButtonPath;
		private Button _leftButton;
		[Export] private readonly NodePath _middleButtonPath;
		private Button _middleButton;
		[Export] private readonly NodePath _rightButtonPath;
		private Button _rightButton;

		private event Action LeftPressed;
		private event Action MiddlePressed;
		private event Action RightPressed;

		public override void _Ready()
		{
			_leftButton = this.GetNodeFromPath<Button>( _leftButtonPath );
			_leftButton.Connect( "pressed", this, nameof(LeftPress) );
			
			_middleButton = this.GetNodeFromPath<Button>( _middleButtonPath );
			_middleButton.Connect( "pressed", this, nameof(MiddlePress) );
			
			_rightButton = this.GetNodeFromPath<Button>( _rightButtonPath );
			_rightButton.Connect( "pressed", this, nameof(RightPress) );
		}

		public void Display( bool showLeft = true, bool showMiddle = true, bool showRight = true, Action leftPress = null, Action middlePress = null, Action rightPress = null )
		{
			LeftPressed = leftPress;
			MiddlePressed = middlePress;
			RightPressed = rightPress;

			if( showLeft )
			{
				_leftButton.Show();
			}
			else
			{
				_leftButton.Hide();
			}
			
			if( showMiddle )
			{
				_middleButton.Show();
			}
			else
			{
				_middleButton.Hide();
			}
			
			if( showRight )
			{
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
			LeftPressed?.Invoke();
			Clear();
		}
		
		private void MiddlePress()
		{
			MiddlePressed?.Invoke();
			Clear();
		}
		
		private void RightPress()
		{
			RightPressed?.Invoke();
			Clear();
		}

		private void Clear()
		{
			Hide();
			LeftPressed = null;
			MiddlePressed = null;
			RightPressed = null;
		}
	}
}