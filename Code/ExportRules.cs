using System.Collections.Generic;
using LibT.Serialization;

namespace MetaMaker
{
	public struct ExportSet
	{
		public string relativeSavePath;
		public string gdoExportTarget;
	}

	public class ExportRules
	{
		public Dictionary<string, ExportSet> ExportTargets => _exportTargets;
		protected Dictionary<string, ExportSet> _exportTargets = new Dictionary<string, ExportSet>();

		public virtual string PostprocessExportJSON(string json, string exportName)
		{
			return json;
		}

		public virtual void PreprocessExportGDA(GenericDataArray genericDataArray, string exportName)
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
	}
}