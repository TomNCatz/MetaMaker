using System.Collections.Generic;
using LibT.Serialization;
using MetaMakerLib;

namespace Rules
{
	/// <summary>
	/// Overwrite this file and build the DLL to make more complex export rules. Changing the name of the resultant DLL
	/// to the same name as the graph file being exported from will cause said DLL to be opened as the export rules for that graph.
	/// </summary>
	public class ExportRules : ExportRulesAbstract
	{
		public ExportRules()
		{
			// Adds one export file option that exports the top level node 'data' as a json
			_exportTargets["Default JSON"] = new ExportSet
			{
				// Relative path converts a few things for ease of use
				// $name is the name of the current graph file being exported minus the extention
				// $export is the string of the export name passed(here it would be 'Default JSON' since that is what this entry is stored as
				// $index is the base 1 index of the split of the file being exported. This will always be '1' unless you are using the childCount option
				// The relative path starts in the same folder as the graph file being exported. navigating up the directory chain can be done with '../'
				relativeSavePath = "$name.json",
				gdoExportTarget = "data"
			};
			
		}

		public override void PreprocessExportGdo(GenericDataObject genericDataObject, List<string> keysToRemove, string exportName, string exportLocation, int exportIndex)
		{
		}
		
		public override string PostprocessExportJSON(string json, string exportName, string exportLocation, int exportIndex)
		{
			return json;
		}
	}
}