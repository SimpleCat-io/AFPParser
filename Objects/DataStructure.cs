﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace AFPParser
{
    public abstract class DataStructure
    {
        public const string EBCDIC = "IBM037";

        // Properties directly converted from raw hex data
        public int Length { get; private set; }
        public string ID { get; private set; }
        public byte[] Data { get; private set; }
        public SemanticsInfo Semantics { get; protected set; }

        // Dynamically calculated properties
        public string DataHex { get { return BitConverter.ToString(Data).Replace("-", " "); } }
        public string DataEBCDIC { get { return Encoding.GetEncoding(EBCDIC).GetString(Data); } }
        protected string SpacedClassName
        {
            get
            {
                // The name of the class but with spaces (add a space before each new uppercased character)
                string className = GetType().Name;
                string spacedName = string.Empty;
                for (int i = 0; i < className.Length; i++)
                    spacedName += i > 0 && char.IsUpper(className[i]) && !char.IsUpper(className[i - 1])
                        ? $" {className[i]}"
                        : className[i].ToString();

                return spacedName;
            }
        }

        protected abstract string StructureName { get; }

        public DataStructure(int length, string id, int introducerLength)
        {
            Length = length;
            ID = id;
            Data = new byte[Length - introducerLength];
        }

        public virtual string GetFullDescription()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{Semantics.Title} ({StructureName} 0x{ID})");
            sb.Append(GetOffsetDescriptions());

            return sb.ToString();
        }

        // Override this to custom handle all offsets at once
        protected virtual string GetOffsetDescriptions()
        {
            StringBuilder sb = new StringBuilder();

            if (Semantics.Offsets.Any())
                // Write out each implemented offset's description
                foreach (Offset oSet in Semantics.Offsets)
                {
                    // Get sectioned data
                    IEnumerable<Offset> nextOffsets = Semantics.Offsets.Where(o => o.StartingIndex > oSet.StartingIndex);
                    int nextIndex = nextOffsets.Any() ? nextOffsets.Min(o => o.StartingIndex) : 0;
                    if (nextIndex == 0) nextIndex = Data.Length;
                    int bytesToTake = nextIndex - oSet.StartingIndex;
                    byte[] sectionedData = GetSectionedData(oSet.StartingIndex, bytesToTake);

                    sb.Append(GetSingleOffsetDescription(oSet, sectionedData));
                }
            else
                sb.AppendLine($"Not yet implemented...{Environment.NewLine}Raw Data: {DataHex}");

            return sb.ToString();
        }

        // Override this to custom handle one or more offsets at a time.
        protected virtual string GetSingleOffsetDescription(Offset oSet, byte[] sectionedData)
        {
            if (string.IsNullOrWhiteSpace(oSet.Description)) return string.Empty;
            StringBuilder sb = new StringBuilder(oSet.DisplayDataByType(sectionedData));
            sb.AppendLine();
            return sb.ToString();
        }

        protected byte[] GetSectionedData(int startIndex, int length)
        {
            byte[] sectionedData = new byte[length];
            for (int i = 0; i < length; i++)
                sectionedData[i] = Data[startIndex + i];

            return sectionedData;
        }

        protected string GetReadableDataPiece(int startIndex, int length)
        {
            return Encoding.GetEncoding(EBCDIC).GetString(GetSectionedData(startIndex, length));
        }

        // Returns a proper Endian array of booleans. 8 bits per byte
        public static bool[] GetBitArray(params byte[] bytes)
        {
            bool[] allBits = new bool[8 * bytes.Length];

            // Append all 8 bits from each byte to our boolean array
            for (int i = 0; i < bytes.Length; i++)
            {
                IEnumerable<bool> curArray = new BitArray(new[] { bytes[i] }).Cast<bool>();
                if (BitConverter.IsLittleEndian) curArray = curArray.Reverse();
                Array.ConstrainedCopy(curArray.ToArray(), 0, allBits, 8 * i, 8);
            }

            return allBits;
        }
        
        // Returns the correct numeric value for an array of bytes
        public static long GetNumericValue(byte[] bytes, bool isSigned)
        {
            // Currently only support up to 4 byte integers
            if (bytes.Length > 4) return 0;

            // If there is an extra byte, strip it out
            if (bytes.Length == 3)
            {
                if (BitConverter.IsLittleEndian)
                    bytes = bytes.Skip(1).ToArray();
                else
                    bytes = bytes.Take(bytes.Length - 1).ToArray();
            }

            // Use correct Endianness
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            // Return signed/unsigned int16/int32 based on array length and parameter
            return bytes.Length == 1 && isSigned ? Convert.ToInt16(bytes[0])
                : bytes.Length == 1 && !isSigned ? Convert.ToUInt16(bytes[0])
                : bytes.Length == 2 && isSigned ? BitConverter.ToInt16(bytes, 0)
                : bytes.Length == 2 && !isSigned ? BitConverter.ToUInt16(bytes, 0)
                : bytes.Length == 4 && isSigned ? BitConverter.ToInt32(bytes, 0)
                : bytes.Length == 4 && !isSigned ? (long)BitConverter.ToUInt32(bytes, 0)
                : 0;
        }
    }
}