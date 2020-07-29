using LibT.Serialization;

namespace LibT
{
	public interface IGdaLoadable
	{
		void LoadFromGda( GenericDataArray data );
	}
}