using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace coke_beach_reportGenerator_api.Services
{
    public class LeftPanelMapping:ILeftPanelMapping
    {
        private DataSet leftPanelData = null;
        public DataSet SetLeftPanelData { set {
                this.leftPanelData = value;
            } }

        public bool CheckLeftPanelData{ get {
                return this.leftPanelData != null;
            } }

        public void SetLeftPanel(DataSet dset)
        {
            this.SetLeftPanelData = dset;
        }
        public bool CheckLeftPanel()
        {
            return this.CheckLeftPanelData;
        }
        public DataTable GetFilterMapping()
        {
            return leftPanelData.Tables[3];
        }

        public DataTable GetGeographyMapping()
        {
            return leftPanelData.Tables[0];
        }

        public DataTable GetProductMapping()
        {
            return leftPanelData.Tables[2];
        }

        public DataTable GetTimeperiodMapping()
        {
            return leftPanelData.Tables[1];
        }
        public DataTable GetSlideMapping()
        {
            return leftPanelData.Tables[4];
        }
    }
}
