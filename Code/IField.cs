using System;
using LibT.Serialization;

namespace MetaMaker
{
	public interface IField
	{
		event Action OnValueUpdated;
		void Init(GenericDataArray template, GenericDataArray parentModel);
	}
}