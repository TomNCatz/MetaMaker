using Godot;
using LibT.Serialization;

namespace LibT
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