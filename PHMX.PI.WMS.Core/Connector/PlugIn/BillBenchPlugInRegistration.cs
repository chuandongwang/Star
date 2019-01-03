using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector.PlugIn
{
    public class BillBenchPlugInRegistration : Dictionary<string, string>
    {
        public BillBenchPlugInRegistration()
        {
            this.Add("PRD_INSTOCK", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.PRDInStockBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("PRD_PickMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.PRDPickMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("PRD_FeedMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.PRDFeedMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("PRD_ReturnMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.PRDReturnMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("PUR_MRB", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.PURMRBBench,PHMX.PI.WMS.App.ConvertPlugIn");

            this.Add("SAL_OUTSTOCK", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SALOUTSTOCKBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SAL_RETURNSTOCK", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SALRETURNSTOCKBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("STK_InStock", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKInStockBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("STK_MISCELLANEOUS", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKMISCELLANEOUSBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("STK_MisDelivery", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKMisDeliveryBench,PHMX.PI.WMS.App.ConvertPlugIn");

            this.Add("STK_TransferDirect", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKTransferDirectBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("STK_TRANSFERIN", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKTRANSFERINBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("STK_TRANSFEROUT", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.STKTRANSFEROUTBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SUB_FEEDMTRL", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SUBFEEDMTRLBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SUB_PickMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SUBPickMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");

            this.Add("SUB_RETURNMTRL", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SUBRETURNMTRLBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SP_InStock", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SPInStockBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SP_ReturnMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SPReturnMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SP_PickMtrl", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SPPickMtrlBench,PHMX.PI.WMS.App.ConvertPlugIn");
            this.Add("SP_OUTSTOCK", "PHMX.PI.WMS.App.ConvertPlugIn.Connector.SPOUTSTOCKBench,PHMX.PI.WMS.App.ConvertPlugIn");
        }
    }
}
