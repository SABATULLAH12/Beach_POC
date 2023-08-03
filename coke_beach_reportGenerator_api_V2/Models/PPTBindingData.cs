using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class PPTBindingData
    {
        public string SelectionType { get; set; }
        public string MetricType { get; set; }
        public string Metric { get; set; }
        public string SelectionName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Significance { get; set; }
        public int GroupSort { get; set; }
        public int SortId { get; set; }
        public int SlideNumber { get; set; }
        public int SelectionId { get; set; }
        public int IsCategory { get; set; }
        public int ConsumptionID { get; set; }
        public string ConsumptionName { get; set; }
    }
    public class ImagesList
    {
        public string SelectionName { get; set; }
        public string ImageName { get; set; }
        public bool HasImage { get; set; }
        public bool IsText { get; set; }
        public Image Image { get; set; }
    }
}
