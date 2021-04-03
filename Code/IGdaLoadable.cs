using LibT.Serialization;

namespace MetaMaker
{
	public interface IGdaLoadable
	{
		void LoadFromGda( GenericDataArray data );
	}
}