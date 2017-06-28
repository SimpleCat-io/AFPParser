﻿using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using AFPParser.Triplets;
using System.Diagnostics;
using AFPParser.Containers;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections.Generic;
using AFPParser.StructuredFields;
using AFPParser.PTXControlSequences;

namespace AFPParser.UI
{
    public class PrintParser
    {
        private AFPFile afpFile;
        private List<Container> pageContainers;
        private int curPageIndex = 0;

        // Raster pattern cache for text
        List<FontCache> fontCaches;

        // PTX Storage
        private Container aeContainer = null;
        private float curXPosition = 0;
        private float curYPosition = 0;
        private int xUnitsPerBase = 0;
        private int yUnitsPerBase = 0;
        private float interCharAdjInch = 0;
        private float varSpaceCharInch = 0;
        private Converters.eMeasurement measurement = Converters.eMeasurement.Inches;
        private string curCodePage = string.Empty;
        private string curFontCharSet = string.Empty;
        private AFPFile.Resource curFontCharSetResource = null;
        private Color curColor = Color.Black;

        public PrintParser(AFPFile file)
        {
            afpFile = file;

            // Capture all pages' containers
            pageContainers = afpFile.Fields.OfType<BPG>().Select(p => p.LowestLevelContainer).ToList();
            if (pageContainers.Count == 0) pageContainers = new List<Container>() { afpFile.Fields[0].LowestLevelContainer };

            CacheGraphicCharacters();
        }

        private void CacheGraphicCharacters()
        {
            curCodePage = "";
            curFontCharSet = "";
            fontCaches = new List<FontCache>();

            // On every page, store a unique list of every used code point, as well as associated font information
            foreach (Container pc in pageContainers)
            {
                foreach (PTX ptx in pc.GetStructures<PTX>())
                {
                    foreach (PTXControlSequence cs in ptx.CSIs)
                    {
                        // Store active code page and coded font
                        if (cs.GetType() == typeof(SCFL))
                        {
                            // Lookup info from MCF1 data on this page
                            MCF1.MCF1Data mcfData = pc.GetStructure<MCF1>().MappedData.First(m => m.ID == ((SCFL)cs).FontId);

                            // If we have a coded font, we need to load that resource. Otherwise, get the values from here
                            if (mcfData != null)
                            {
                                AFPFile.Resource codedFontResource = afpFile.Resources.OfTypeAndName(AFPFile.Resource.eResourceType.CodedFont, mcfData.CodedFontName);
                                CFI resourceCFI = codedFontResource != null && codedFontResource.IsLoaded ? codedFontResource.Fields.OfType<CFI>().FirstOrDefault() : null;

                                if (resourceCFI != null && resourceCFI.FontInfoList.Any())
                                {
                                    // Get the code page and font char set from the resource
                                    curCodePage = resourceCFI.FontInfoList[0].CodePageName;
                                    curFontCharSet = resourceCFI.FontInfoList[0].FontCharacterSetName;
                                }
                                else
                                {
                                    // Store mcf's code page and font char set specifiers
                                    curCodePage = mcfData.CodePageName;
                                    curFontCharSet = mcfData.FontCharacterSetName;
                                }
                            }
                        }
                        else if (cs.GetType() == typeof(TRN))
                            foreach (byte b in cs.Data)
                                // Add a new font cache item if this exact combo (byte/code page/char set) does not yet exist
                                if (!fontCaches.Any(f => f.CodePoint == b && f.CodePage == curCodePage && f.FontCharSet == curFontCharSet))
                                    fontCaches.Add(new FontCache(b, curCodePage, curFontCharSet));
                    }
                }
            }

            // For each byte/code point/font, generate a bitmap by looking up its resource
            curCodePage = "";
            curFontCharSet = "";
            Dictionary<byte, string> cpMappings = CodePages.C1252;
            AFPFile.Resource fontResource = null;
            FontObjectContainer foc = null;
            float emInchSize = 0;
            byte vsc = 0;
            foreach (FontCache fc in fontCaches.OrderBy(f => f.CodePage).ThenBy(f => f.FontCharSet))
            {
                // Generate new code page mappings if needed
                if (fc.CodePage != curCodePage)
                {
                    cpMappings = new Dictionary<byte, string>();

                    // If the code page is a resource, generate from scratch
                    AFPFile.Resource cp = afpFile.Resources.OfTypeAndName(AFPFile.Resource.eResourceType.CodePage, fc.CodePage);
                    if (cp != null && cp.IsLoaded)
                    {
                        foreach (CPI.Info info in cp.Fields.OfType<CPI>().First().CPIInfos)
                            cpMappings.Add(info.CodePoints[0], info.GID);

                        // Is variable space char if the byte equals the one specified by the code page descriptor
                        vsc = cp.Fields.OfType<CPC>().First().VariableSpaceCharacter;
                    }
                    // Else, get by looking up last 4 page digits
                    else
                    {
                        // Get probably name of static field in Code Pages lookups
                        string sectionedCodePage = string.Empty;
                        if (fc.CodePage.Length >= 4) sectionedCodePage = $"C{fc.CodePage.Substring(fc.CodePage.Length - 4)}";

                        // Find the matching lookup method in our code page helper class
                        FieldInfo field = typeof(CodePages).GetField(sectionedCodePage);
                        if (field != null) cpMappings = (Dictionary<byte, string>)field.GetValue(null);

                        // Is variable space character if the byte GID equals SP010000
                        vsc = cpMappings.First(c => c.Value == "SP010000").Key;
                    }

                    curCodePage = fc.CodePage;
                }

                // Lookup new font character set if needed
                if (fc.FontCharSet != curFontCharSet)
                {
                    fontResource = afpFile.Resources.OfTypeAndName(AFPFile.Resource.eResourceType.FontCharacterSet, fc.FontCharSet);
                    curFontCharSet = fc.FontCharSet;
                    foc = (FontObjectContainer)fontResource.Fields[0].LowestLevelContainer;
                    emInchSize = foc.GetStructure<FND>().EmInches;
                }

                // Generate a bitmap for this code point by looking up the raster pattern of the FCS by current code page
                string gid = cpMappings.ContainsKey(fc.CodePoint) ? cpMappings[fc.CodePoint] : string.Empty;
                if (!string.IsNullOrEmpty(gid) && fontResource != null && fontResource.IsLoaded)
                {
                    // Get raster pattern info of GID
                    KeyValuePair<FNI.Info, bool[,]> pattern = foc.RasterPatterns.FirstOrDefault(p => p.Key.GCGID == gid);

                    // Build bitmap
                    if (pattern.Key != null)
                    {
                        // Set font and variable space info here
                        fc.FontInfo = pattern.Key;
                        fc.IsVariableSpaceChar = fc.CodePoint == vsc;
                        fc.EmInchSize = emInchSize;

                        Bitmap bmp = new Bitmap(pattern.Value.GetUpperBound(0) + 1, pattern.Value.GetUpperBound(1) + 1);
                        for (int y = 0; y < bmp.Height; y++)
                            for (int x = 0; x < bmp.Width; x++)
                                if (pattern.Value[x, y])
                                    bmp.SetPixel(x, y, Color.Black); // Set to black - we remap colors later

                        // Since we know how many inches 1 em is, we can determine inch width and height of each character
                        float heightInches = emInchSize * ((pattern.Key.AscenderHeight + pattern.Key.DescenderDepth) / 1000f);
                        float dpi = (float)Math.Round(bmp.Height / heightInches);
                        bmp.SetResolution(dpi, dpi);

                        // Assign to font cache
                        fc.Pattern = bmp;
                    }
                }
            }
        }

        public void BuildPrintPage(object sender, PrintPageEventArgs e)
        {
            // Fancy filtering
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // Draw each embedded IM image in the current page. Positional information is stored inside the IID field of each image container
            foreach (IMImageContainer imc in pageContainers[curPageIndex].Structures.Select(f => f.LowestLevelContainer).Distinct().OfType<IMImageContainer>())
                DrawIMImage(imc, 0, 0, e);

            // Draw each embedded IOCA image in the current page. Positional information is stored inside of the OBD/OBP fields of each image container
            foreach (IOCAImageContainer ioc in pageContainers[curPageIndex].Structures.Select(f => f.LowestLevelContainer).Distinct().OfType<IOCAImageContainer>())
                DrawIOCAImage(ioc, 0, 0, e);

            // Include each IM and IOCA image in IPS by looking up the loaded resource
            foreach (IPS pageSegment in pageContainers[curPageIndex].GetStructures<IPS>())
            {
                // Find the first loaded image resource of the indicated name
                AFPFile.Resource loadedResource = afpFile.Resources.FirstOrDefault(r => r.ResourceName == pageSegment.SegmentName.ToUpper().Trim()
                     && r.IsLoaded && r.ResourceType == AFPFile.Resource.eResourceType.PageSegment);

                if (loadedResource != null)
                {
                    // Get starting positional information by querying the active environment group
                    Container aegContainer = pageContainers[curPageIndex].GetStructure<BAG>().LowestLevelContainer;
                    PGD pageDescriptor = aegContainer.GetStructure<PGD>();
                    float xInch = (float)Converters.GetInches(pageSegment.XOrigin, pageDescriptor.UnitsPerYBase, pageDescriptor.BaseUnit) * 100;
                    float yInch = (float)Converters.GetInches(pageSegment.YOrigin, pageDescriptor.UnitsPerYBase, pageDescriptor.BaseUnit) * 100;

                    if (loadedResource.Fields.Any(f => f.LowestLevelContainer.GetType() == typeof(IMImageContainer)))
                    {

                        IMImageContainer imc = loadedResource.Fields.Select(f => f.LowestLevelContainer).OfType<IMImageContainer>().FirstOrDefault();
                        DrawIMImage(imc, xInch, yInch, e);
                    }
                    else if (loadedResource.Fields.Any(f => f.LowestLevelContainer.GetType() == typeof(IOCAImageContainer)))
                    {
                        IOCAImageContainer ioc = loadedResource.Fields.Select(f => f.LowestLevelContainer).OfType<IOCAImageContainer>().FirstOrDefault();
                        DrawIOCAImage(ioc, xInch, yInch, e);
                    }
                }
            }

            DrawPresentationText(e);

            // Increment the current page index and check to see if there are any more
            e.HasMorePages = ++curPageIndex < pageContainers.Count;

            if (!e.HasMorePages) curPageIndex = 0;
        }

        private void DrawIMImage(IMImageContainer imc, float xStartingInch, float yStartingInch, PrintPageEventArgs e)
        {
            // Build a bitmap out of the 2 dimensional array of booleans
            Bitmap bmp = new Bitmap(imc.ImageData.GetUpperBound(0) + 1, imc.ImageData.GetUpperBound(1) + 1);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    if (imc.ImageData[x, y])
                        bmp.SetPixel(x, y, Color.Black);

            // Get positional/scaling information from min offsets and IID field
            IID imDescriptor = imc.GetStructure<IID>();
            int xPos = imc.Cells.Min(c => c.CellPosition.XOffset);
            int yPos = imc.Cells.Min(c => c.CellPosition.YOffset);
            float xInchPos = xStartingInch + (float)(Converters.GetInches(xPos, imDescriptor.XUnitsPerBase, imDescriptor.BaseUnit) * 100);
            float yInchPos = yStartingInch + (float)(Converters.GetInches(yPos, imDescriptor.YUnitsPerBase, imDescriptor.BaseUnit) * 100);
            double heightInches = Converters.GetInches(bmp.Height, imDescriptor.YUnitsPerBase, imDescriptor.BaseUnit);

            float dpi = (float)Math.Round(bmp.Height / heightInches);
            bmp.SetResolution(dpi, dpi);
            e.Graphics.DrawImage(bmp, xInchPos, yInchPos);
        }

        private void DrawIOCAImage(IOCAImageContainer imc, float xStartingInch, float yStartingInch, PrintPageEventArgs e)
        {
            // Each container may hold one image or several, as tiles. Draw each one with consideration to its offset from the original draw point
            foreach (ImageContentContainer.ImageInfo image in imc.Images)
            {
                // Get the positioning and scaling info based on the current object environment container
                OBD oaDescriptor = imc.GetStructure<OBD>();
                OBP oaPosition = imc.GetStructure<OBP>();
                if (oaDescriptor != null && oaPosition != null)
                {
                    // Get sizing info triplets
                    ObjectAreaSize oaSize = oaDescriptor.GetTriplet<ObjectAreaSize>();
                    MeasurementUnits mu = oaDescriptor.GetTriplet<MeasurementUnits>();

                    // Get inch origins based on unit scaling
                    int xUnitOrigin = oaPosition.XOrigin + oaPosition.XContentOrigin;
                    int yUnitOrigin = oaPosition.YOrigin + oaPosition.YContentOrigin;
                    float xInchOrigin = xStartingInch + (float)(Converters.GetInches(xUnitOrigin, mu.XUnitsPerBase, mu.BaseUnit) * 100);
                    float yInchOrigin = yStartingInch + (float)(Converters.GetInches(yUnitOrigin, mu.YUnitsPerBase, mu.BaseUnit) * 100);

                    // Get inch scaling values
                    double heightInches = Converters.GetInches(oaSize.YExtent, mu.YUnitsPerBase, mu.BaseUnit);

                    // We have the inch value and number of pixels, so set DPI based on those values
                    Bitmap bmp = new Bitmap(new MemoryStream(image.Data));
                    float dpi = (float)Math.Round(bmp.Height / heightInches);
                    bmp.SetResolution(dpi, dpi);

                    e.Graphics.DrawImage(bmp, xInchOrigin, yInchOrigin);
                }
                else
                {
                    throw new NotImplementedException("Image could not be displayed - no OBD/OBP fields found.");
                }
            }
        }

        private void DrawPresentationText(PrintPageEventArgs e)
        {
            // Store presentation text/page descriptor information
            GetDescriptorInfo();

            // Parse each PTX's control sequences
            foreach (PTX text in pageContainers[curPageIndex].GetStructures<PTX>())
            {
                // Reset PTX variables at the beginning of each PTX field
                curXPosition = 0;
                curYPosition = 0;
                interCharAdjInch = 0;
                varSpaceCharInch = 0;
                curCodePage = string.Empty;
                curFontCharSet = string.Empty;
                curFontCharSetResource = null;
                curColor = Color.Black;

                foreach (PTXControlSequence sequence in text.CSIs)
                {
                    Type sequenceType = sequence.GetType();

                    if (sequenceType == typeof(SCFL)) SetCodePageAndFont((SCFL)sequence);
                    else if (sequenceType == typeof(AMI)) curXPosition = (float)Converters.GetInches(((AMI)sequence).Displacement, xUnitsPerBase, measurement) * 100;
                    else if (sequenceType == typeof(AMB)) curYPosition = (float)Converters.GetInches(((AMB)sequence).Displacement, yUnitsPerBase, measurement) * 100;
                    else if (sequenceType == typeof(RMI)) curXPosition += (float)Converters.GetInches(((RMI)sequence).Increment, xUnitsPerBase, measurement) * 100;
                    else if (sequenceType == typeof(RMB)) curYPosition += (float)Converters.GetInches(((RMB)sequence).Increment, yUnitsPerBase, measurement) * 100;
                    else if (sequenceType == typeof(STC)) curColor = ((STC)sequence).TextColor;
                    else if (sequenceType == typeof(SEC)) curColor = ((SEC)sequence).TextColor;
                    else if (sequenceType == typeof(SIA)) interCharAdjInch = ((((SIA)sequence).Adjustment * (((SIA)sequence).Forward ? 1 : -1)) / 1440f) * 100;
                    else if (sequenceType == typeof(SVI)) varSpaceCharInch = (((SVI)sequence).Increment / 1440f) * 100;
                    else if (sequenceType == typeof(DIR) || sequenceType == typeof(DBR)) DrawLine(sequence, e);
                    else if (sequenceType == typeof(TRN)) DrawStringAsImage(sequence.Data, e);
                }
            }
        }

        private void DrawLine(PTXControlSequence sequence, PrintPageEventArgs e)
        {
            // Inline (horizontal) or Baseline (vertical)?
            bool isInline = sequence.GetType() == typeof(DIR);

            // Get the width of the line first (1/1440 of an inch)
            int width = (int)sequence.GetType().GetProperty("RuleWidth").GetValue(sequence);

            // One dot will be visible if the width is greater than....?
            float widthInches = (width / 1440f) * 100;
            float dotInches = (1 / (float)(isInline ? e.PageSettings.PrinterResolution.X : e.PageSettings.PrinterResolution.Y)) * 100;

            // Prepare X,Y origin/destination
            int length = (int)sequence.GetType().GetProperty("RuleLength").GetValue(sequence);
            float xOrig = curXPosition, yOrig = curYPosition, xDest = curXPosition, yDest = curYPosition;

            // If the width > 1 dot, shift the line points by half the width to un-center it
            if (widthInches > dotInches)
            {
                if (isInline)
                {
                    // Shift Y
                    yOrig -= widthInches / 2f;
                    yDest -= widthInches / 2f;
                }
                else
                {
                    // Shift X
                    xOrig -= widthInches / 2f;
                    xDest -= widthInches / 2f;
                }
            }

            // Set destination
            if (isInline)
                xDest += (length / 1440f) * 100;
            else
                yDest += (length / 1440f) * 100;

            e.Graphics.DrawLine(new Pen(curColor), xOrig, yOrig, xDest, yDest);
        }

        private void GetDescriptorInfo()
        {
            BAG activeEnvironmentGroup = pageContainers[curPageIndex].GetStructure<BAG>();

            if (activeEnvironmentGroup != null)
            {
                aeContainer = activeEnvironmentGroup.LowestLevelContainer;

                // Grab the Presentation Text Descriptor and store the units per base and base unit
                PGD pageDescriptor = pageContainers[curPageIndex].GetStructure<PGD>();
                PTD1 descriptor1 = pageContainers[curPageIndex].GetStructure<PTD1>();
                PTD2 descriptor2 = pageContainers[curPageIndex].GetStructure<PTD2>();

                if (descriptor2 != null)
                {
                    xUnitsPerBase = descriptor2.UnitsPerXBase;
                    yUnitsPerBase = descriptor2.UnitsPerYBase;
                    measurement = descriptor2.BaseUnit;
                }
                else if (descriptor1 != null)
                {
                    xUnitsPerBase = descriptor1.UnitsPerXBase;
                    yUnitsPerBase = descriptor1.UnitsPerYBase;
                    measurement = descriptor1.BaseUnit;
                }
                else
                {
                    xUnitsPerBase = pageDescriptor.UnitsPerXBase;
                    yUnitsPerBase = pageDescriptor.UnitsPerYBase;
                    measurement = pageDescriptor.BaseUnit;
                }
            }
            else
            {
                throw new NotImplementedException("Presentation text could not be displayed - no active environment group found.");
            }
        }

        private void DrawStringAsImage(byte[] data, PrintPageEventArgs e)
        {
            // Find the matching cached item that matches each byte, code page, and font character set
            foreach (byte b in data)
            {
                FontCache fc = fontCaches.First(f => f.CodePoint == b && f.CodePage == curCodePage && f.FontCharSet == curFontCharSet);

                // If this byte is a space character, just increment our x position
                if (fc.IsVariableSpaceChar)
                    curXPosition += GetVariableSpaceIncrementInch();
                else if (fc.Pattern != null)
                {
                    // If BMP is null, no graphic character was found. Skip these entirely
                    float aSpaceInches = fc.EmInchSize * (fc.FontInfo.ASpace / 1000f) * 100;
                    float cSpaceInches = fc.EmInchSize * (fc.FontInfo.CSpace / 1000f) * 100;
                    float baselineOffsetInches = fc.EmInchSize * (fc.FontInfo.BaselineOffset / 1000f) * 100;
                    float charIncrement = fc.EmInchSize * (fc.FontInfo.CharIncrement / 1000f) * 100;

                    // Change color if needed
                    ImageAttributes imAttr = new ImageAttributes();
                    if (curColor != Color.Black)
                    {
                        ColorMap[] map = new ColorMap[1] { new ColorMap() { OldColor = Color.Black } };
                        map[0].NewColor = curColor;
                        imAttr.SetRemapTable(map);
                    }

                    // Draw image
                    float leftX = curXPosition - aSpaceInches;
                    float rightX = leftX + ((fc.Pattern.Width / fc.Pattern.HorizontalResolution) * 100);
                    float topY = curYPosition - baselineOffsetInches;
                    float bottomY = topY + ((fc.Pattern.Height / fc.Pattern.VerticalResolution) * 100);
                    e.Graphics.DrawImage(
                        fc.Pattern,
                        new PointF[] { new PointF(leftX, topY), new PointF(rightX, topY), new PointF(leftX, bottomY) },
                        new RectangleF(0, 0, fc.Pattern.Width, fc.Pattern.Height),
                        GraphicsUnit.Pixel,
                        imAttr);
                    //e.Graphics.DrawImage(fc.Pattern, curXPosition - aSpaceInches, curYPosition - baselineOffsetInches);

                    // Increment our spacing by our character increment (+- adjustment) for this byte
                    curXPosition += charIncrement + interCharAdjInch;
                }
            }
        }

        private float GetVariableSpaceIncrementInch()
        {
            // 1 - The current variable space character increment
            if (varSpaceCharInch != 0) return varSpaceCharInch;

            if (curFontCharSetResource != null)
            {
                float emInches = curFontCharSetResource.Fields.OfType<FND>().First().EmInches;

                // 2 - The default variable space character increment of the active coded font
                int fontCharInc = curFontCharSetResource.Fields.OfType<FNO>().First().FNOInfo[0].SpaceCharIncrement;
                if (fontCharInc != 0) return emInches * (fontCharInc / 1000f) * 100;

                // 3 - The character increment of the default variable space character code point
                FNI.Info info = curFontCharSetResource.Fields.OfType<FNI>().First().InfoList.FirstOrDefault(i => i.GCGID == "SP010000");
                if (info != null) return emInches * (info.CharIncrement / 1000f) * 100;
            }

            return 0;
        }

        private void SetCodePageAndFont(SCFL sfcl)
        {
            curCodePage = string.Empty;
            curFontCharSet = string.Empty;

            MCF1 map1 = aeContainer.GetStructure<MCF1>();

            // MCF2 is not supported yet...
            if (map1 != null)
            {
                // Get mapping info with the ID specified in the SCFL field
                MCF1.MCF1Data mcfData = map1.MappedData.FirstOrDefault(m => m.ID == sfcl.FontId);

                if (mcfData != null)
                {
                    // If it already has a code page/font character set specified, use that.
                    if (!string.IsNullOrWhiteSpace(mcfData.CodePageName) && !string.IsNullOrWhiteSpace(mcfData.FontCharacterSetName))
                    {
                        curCodePage = mcfData.CodePageName;
                        curFontCharSet = mcfData.FontCharacterSetName;
                    }
                    else
                    {
                        // Otherwise, we need to load it from the coded font resource
                        AFPFile.Resource codedFont = afpFile.Resources.OfTypeAndName(AFPFile.Resource.eResourceType.CodedFont, mcfData.CodedFontName);

                        if (codedFont.IsLoaded)
                        {
                            CFI cfi = codedFont.Fields.OfType<CFI>().FirstOrDefault();
                            if (cfi != null && cfi.FontInfoList.Any())
                            {
                                curCodePage = cfi.FontInfoList[0].CodePageName;
                                curFontCharSet = cfi.FontInfoList[0].FontCharacterSetName;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(curFontCharSet))
                curFontCharSetResource = afpFile.Resources.OfTypeAndName(AFPFile.Resource.eResourceType.FontCharacterSet, curFontCharSet);
        }

        [DebuggerDisplay("0x{CodePoint.ToString(\"X\"),nq} / {CodePage,nq} / {FontCharSet,nq}")]
        private class FontCache
        {
            public byte CodePoint { get; private set; }
            public string CodePage { get; private set; }
            public string FontCharSet { get; private set; }

            public FNI.Info FontInfo { get; set; }
            public float EmInchSize { get; set; }
            public bool IsVariableSpaceChar { get; set; }
            public Bitmap Pattern { get; set; }

            public FontCache(byte code, string page, string font)
            {
                CodePoint = code;
                CodePage = page;
                FontCharSet = font;
            }
        }
    }
}