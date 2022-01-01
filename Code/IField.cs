using System;
using LibT.Serialization;

namespace MetaMaker
{
	public interface IField
	{
		string Label{get;set;}
		event Action OnValueUpdated;
		void Init(GenericDataDictionary template, GenericDataObject parentModel);
	}
}