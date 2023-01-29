namespace MetaMaker
{
	public interface ITextSearchable
	{
		string Text { get; }
		T GetParent<T>() where T : class;
	}
}