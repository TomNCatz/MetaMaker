using Godot;

namespace MetaMaker
{
	public class LinkToParentSlot : Label
	{
		public bool IsLinked { get; private set; }
		
		public void Link()
		{
			IsLinked = true;
		}

		public void Unlink()
		{
			IsLinked = false;
		}
	}
}