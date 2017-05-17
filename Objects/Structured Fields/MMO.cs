using System.Collections.Generic;

namespace AFPParser.StructuredFields
{
	public class MMO : StructuredField
	{
		private static string _abbr = "MMO";
		private static string _title = "Map Medium Overlay";
		private static string _desc = "The Map Medium Overlay structured field maps one-byte medium overlay local identifiers that are specified by keywords in the Medium Modification Control (MMC) structured field to medium overlay names.";
		private static List<Offset> _oSets = new List<Offset>();

		public override string Abbreviation => _abbr;
		public override string Title => _title;
		protected override string Description => _desc;
		protected override bool IsRepeatingGroup => false;
		protected override int RepeatingGroupStart => 0;
		protected override List<Offset> Offsets => _oSets;

		public MMO(int length, string hex, byte flag, int sequence) : base (length, hex, flag, sequence) { }
	}
}