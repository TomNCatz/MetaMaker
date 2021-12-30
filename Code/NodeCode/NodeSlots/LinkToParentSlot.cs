using Godot;

namespace MetaMaker
{
	public class LinkToParentSlot : Label
	{
		public bool IsLinked => _links >0;
		private int _links = 0;
		
		public void Link()
		{
			_links++;
		}

		public void Unlink()
		{
			_links--;
		}
	}
}