using LibT.Serialization;

namespace MetaMaker
{
	public interface IField
	{
		void Init(GenericDataArray template, GenericDataArray parentModel);
	}
}