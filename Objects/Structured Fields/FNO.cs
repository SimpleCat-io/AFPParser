using System.Collections.Generic;

namespace AFPParser.StructuredFields
{
	public class FNO : StructuredField
	{
		private static string _abbr = "FNO";
		private static string _title = "Font Orientation";
		private static string _desc = "";
		private static List<Offset> _oSets = new List<Offset>();

		public override string Abbreviation => _abbr;
		public override string Title => _title;
		protected override string Description => _desc;
		protected override bool IsRepeatingGroup => false;
		protected override int RepeatingGroupStart => 0;
		protected override List<Offset> Offsets => _oSets;

		public FNO(int length, string hex, byte flag, int sequence) : base (length, hex, flag, sequence) { }
	}
}