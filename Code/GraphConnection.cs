namespace MetaMaker
{
	public class GraphConnection
	{
		public bool goingRight;
		public string from;
		public int fromPort;
		public string to;
		public int toPort;

		public override string ToString()
		{
			return $"{@from}:{fromPort}->{to}:{toPort}";
		}
	}
}