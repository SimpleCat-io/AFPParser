﻿using System.Linq;
using System.Collections.Generic;
using AFPParser.ImageSelfDefiningFields;

namespace AFPParser.Containers
{
    public class ImageContentContainer : Container
    {
        public IReadOnlyList<ImageInfo> ImageList { get; private set; }

        public ImageContentContainer()
        {
            ImageList = new List<ImageInfo>();
        }

        public override void ParseContainerData()
        {
            List<Container> allContainers = new List<Container>() { this };
            List<ImageInfo> infoList = new List<ImageInfo>();

            // Along with ourself, add any tiles to the list of containers
            if (Structures.Any(s => s.GetType() == typeof(BeginTile)))
                allContainers.AddRange(Structures.OfType<BeginTile>().Select(s => s.LowestLevelContainer));
            
            // For each container, get image and transparency bytes
            foreach (Container c in allContainers)
            {
                ImageInfo info = new ImageInfo();
                info.Data = c.DirectGetStructures<ImageData>().SelectMany(s => s.Data).ToArray();

                // Add transparency data if needed
                ImageSelfDefiningField BTM = c.GetStructure<BeginTransparencyMask>();
                if (BTM != null)
                    info.TransparencyMask = BTM.LowestLevelContainer.GetStructures<ImageData>().SelectMany(s => s.Data).ToArray();

                infoList.Add(info);
            }
            
            ImageList = infoList;
        }

        public class ImageInfo
        {
            public byte[] Data { get; set; }
            public byte[] TransparencyMask { get; set; }

            public ImageInfo()
            {
                Data = new byte[0];
                TransparencyMask = new byte[0];
            }
        }
    }
}