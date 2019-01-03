using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.InDetailLinkInNoticeDto
{
    public class InDetailEntryLinkInNotice
    {
        public long SourceEntryId { get; set; }
        public long SourceBillId { get; set; }
        public string PackageId { get; set; }
        public decimal Qty { get; set; }
        public string UnitId { get; set; }
        public string TrackNo { get; set; }
        public string BatchNo { get; set; }
        public DateTime? KFDate { get; set; }
        public string LocId { get; set; }
        public decimal Cty { get; set; }
        public decimal AvgCty { get; set; }
        public int ExpPeriod { get; set; }
        public string ExpUnit { get; set; }
        public decimal PHMXWgt { get; set; }

    }
}
