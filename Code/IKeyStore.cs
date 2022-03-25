namespace MetaMaker
{
	public interface IKeyStore
	{
		string TargetKey { get; set; }
		int LinkType { get; }
	}
}