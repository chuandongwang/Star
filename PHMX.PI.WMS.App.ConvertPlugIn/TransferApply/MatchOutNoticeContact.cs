﻿using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ConvertPlugIn.TransferApply
{
    [Description("源单是调拨申请单，至发货通知时匹配联系对象，单据转换插件。")]
    public class MatchOutNoticeContact : AbstractConvertPlugIn
    {
        public override void OnQueryBuilderParemeter(QueryBuilderParemeterEventArgs e)
        {
            base.OnQueryBuilderParemeter(e);
            e.SelectItems.Add(new SelectorItemInfo("FTRANSTYPE"));
            e.SelectItems.Add(new SelectorRefItemInfo("FStockInId.FPHMXContactId.Id").Adaptive(info => { info.PropertyName = "FPHMXStockContactId"; }));
            e.DicFieldAlias.Add("FStockInId.FPHMXContactId.Id", "FPHMXStockContactId");
        }

        public override void OnGetSourceData(GetSourceDataEventArgs e)
        {
            base.OnGetSourceData(e);
            foreach (var data in e.SourceData)
            {
                //组织内调拨
                if (data.Property<string>("FTRANSTYPE").EqualsIgnoreCase("InnerOrgTransfer"))
                {
                    //将调入组织的联系对象改为调入仓库的联系对象，直接对接后续的字段映射。
                    data[e.DicFieldAlias["FStockOrgInId.FPHMXContactId.Id"]] = data.Property<string>(e.DicFieldAlias["FStockInId.FPHMXContactId.Id"]);
                }//end if
            }//end foreach
        }//end method

    }//end class
}//end namespace
