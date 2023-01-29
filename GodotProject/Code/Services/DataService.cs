using LibT.Serialization;
using LibT.Services;
using MetaMaker.UI;

namespace MetaMaker.Services;

public class DataService
{
	public const string GRAPH_DATA_KEY = "data";
	
	public GenericDataDictionary Data { get; private set; }
	
	
	[Injectable]
	private Inspector _inspector;
	[Injectable]
	private TreeView _treeView;

	public void LoadData(GenericDataDictionary data)
	{
		Data = data;
		data.GetValue("ngMapNodeName", out string nodename);
		_inspector.headerLabel.Text = nodename;
		
		_treeView.Display(data);
	}
}