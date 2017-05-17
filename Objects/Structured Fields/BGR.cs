using System.Collections.Generic;

namespace AFPParser.StructuredFields
{
	public class BGR : StructuredField
	{
		private static string _abbr = "BGR";
		private static string _title = "Begin Graphics Object";
		private static string _desc = "The Begin Graphics Object structured field begins a graphics data object which becomes the current data object.";
		private static List<Offset> _oSets = new List<Offset>();

		public override string Abbreviation => _abbr;
		public override string Title => _title;
		protected override string Description => _desc;
		protected override bool IsRepeatingGroup => false;
		protected override int RepeatingGroupStart => 0;
		protected override List<Offset> Offsets => _oSets;

		public BGR(int length, string hex, byte flag, int sequence) : base (length, hex, flag, sequence) { }
	}
}