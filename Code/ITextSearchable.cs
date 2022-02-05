namespace MetaMaker
{
	public interface ITextSearchable
	{
		bool ContainsText(string text);
		T GetParent<T>() where T : class;
	}
}