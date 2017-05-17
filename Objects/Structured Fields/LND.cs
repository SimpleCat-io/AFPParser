using System.Collections.Generic;

namespace AFPParser.StructuredFields
{
	public class LND : StructuredField
	{
		private static string _abbr = "LND";
		private static string _title = "Line Descriptor";
		private static string _desc = "The Line Descriptor structured field contains information, such as line position, text orientation, font selection, field selection, and conditional processing identification, used to format line data. Note: The LNDs in a Data Map are numbered sequentially, starting with 1. Values from 1 through the number of LNDs are allowed.";
		private static List<Offset> _oSets = new List<Offset>();

		public override string Abbreviation => _abbr;
		public override string Title => _title;
		protected override string Description => _desc;
		protected override bool IsRepeatingGroup => false;
		protected override int RepeatingGroupStart => 0;
		protected override List<Offset> Offsets => _oSets;

		public LND(int length, string hex, byte flag, int sequence) : base (length, hex, flag, sequence) { }
	}
}