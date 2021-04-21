using System.Collections.Generic;
using LibT.Serialization;

namespace MetaMaker
{
	public struct ExportSet
	{
		public string relativeSavePath;
		public string gdoExportTarget;
		public int childCount;
	}

	public class ExportRules
	{
		public Dictionary<string, ExportSet> ExportTargets => _exportTargets;
		public string graphVersionKey;
		public string metaMakerVersion;

		protected Dictionary<string, ExportSet> _exportTargets = new Dictionary<string, ExportSet>();

		public virtual string PostprocessExportJSON(string json, string exportName, string exportLocation, int exportIndex)
		{
			return json;
		}

		public virtual void PreprocessExportGDA(GenericDataArray genericDataArray, string exportName, string exportLocation, int exportIndex)
		{
		}
	}

	public class DefaultExportRules : ExportRules
	{
		public DefaultExportRules()
		{
			_exportTargets["Default JSON"] = new ExportSet(){relativeSavePath = "Default.json", gdoExportTarget = "data" };
		}
	}

	public class TemplateExportRules : ExportRules
	{
		public TemplateExportRules()
		{
			_exportTargets["Template file"] = new ExportSet(){relativeSavePath = "$name.tmplt", gdoExportTarget = "data" };
		}

		public override void PreprocessExportGDA(GenericDataArray genericDataArray, string exportName, string exportLocation, int exportIndex)
		{
			genericDataArray.AddValue( graphVersionKey, metaMakerVersion );
		}
	}
}