using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace coke_beach_reportGenerator_api.Models.LeftPanelModel
{
    public class PPTResponse
    {
        public Guid id { get; set; }
        public bool isCompleted { get; set; }
        public string errorMessage { get; set; }
        public bool isError { get; set; }
        public byte[] data { get; set; }
    }
}
