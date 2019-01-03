using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.PutDetailLinkInDetailDto
{
    public class PutDetailEntryLinkInNotice
    {



        public long SourceEntryId { get; set; }
        public long SourceBillId { get; set; }
        public string ToPackageId { get; set; }
        public decimal ToQty { get; set; }
        public string ToUnitId { get; set; }
        public string FromTrackNo { get; set; }
        public string ToTrackNo { get; set; }
        public string ToLocId { get; set; }
        public decimal ToCty { get; set; }
        public decimal ToAvgCty { get; set; }



    }
}
