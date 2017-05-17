using System.Collections.Generic;

namespace AFPParser.StructuredFields
{
	public class OCD : StructuredField
	{
		private static string _abbr = "OCD";
		private static string _title = "Object Container Data";
		private static string _desc = "The Object Container Data structured field contains the data for an object carried in an object container.See “Object Type Identifiers” on page 623 for the list of object types that may be carried in an object container.";
		private static List<Offset> _oSets = new List<Offset>();

		public override string Abbreviation => _abbr;
		public override string Title => _title;
		protected override string Description => _desc;
		protected override bool IsRepeatingGroup => false;
		protected override int RepeatingGroupStart => 0;
		protected override List<Offset> Offsets => _oSets;

		public OCD(int length, string hex, byte flag, int sequence) : base (length, hex, flag, sequence) { }
	}
}