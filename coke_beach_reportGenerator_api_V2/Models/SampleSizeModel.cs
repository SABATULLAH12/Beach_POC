using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class SampleSizeModel
    {
        public int SelectionID { get; set; }
        public string SelectionName { get; set; }
        public string SelectionType { get; set; }
        public int ConsumptionId { get; set; }
        public int SampleSize { get; set; }
        public string DetailedText { get; set; }
    }
    public class NumSurveyResult
    {
        public string Country_ID { get; set; }
        public string Resp_Static_ID { get; set; }
        public double NumSurveys { get; set; }
    }
    public class NumSurveyWp
    {
        //public int SelectionID { get; set; }
        public string Selection { get; set; }
        public string Country_ID { get; set; }
        public string Resp_Static_ID { get; set; }
        public string Respondent_ID { get; set; }
        public int Freq_Numeric { get; set; }
        public double weight { get; set; }
        public int num_surveys { get; set; }
    }
    public class NumSurveyMaxWp
    {
        //public int SelectionID { get; set; }
        public string Selection { get; set; }
        public string Country_ID { get; set; }
        public string Resp_Static_ID { get; set; }
        public string Respondent_ID { get; set; }
        public int Freq_Numeric { get; set; }
        public double weight { get; set; }
        public int num_surveys { get; set; }
    }
    public class ObserveDrinkerWeightedModel
    {
        public string Selection { get; set; }
        public string Country_ID { get; set; }
        public double ObserveDrinkerWeighted { get; set; }
    }
}
