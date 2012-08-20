﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebTranscoderProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasVideoStream { get; set; }
        public string MIME { get; set; }
        public int MaxOutputWidth { get; set; }
        public int MaxOutputHeight { get; set; }
        public IList<string> Targets { get; set; }
        public int Bandwidth { get; set; }
        public string Transport { get; set; }
    }
}
