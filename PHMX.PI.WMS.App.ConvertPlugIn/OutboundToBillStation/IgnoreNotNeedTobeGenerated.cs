using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ConvertPlugIn.OutboundToBillStation
{
    [Description("发货明细至单据中转站，忽略无需生成目标单据的数据源，单据转换插件。")]
    public class IgnoreNotNeedTobeGenerated : AbstractConvertPlugIn
    {
        public override void OnInSelectedRow(InSelectedRowEventArgs e)
        {
            base.OnInSelectedRow(e);

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT OUTBOUNDENTRY.FID");
            sql.AppendLine("FROM BAH_T_WMS_OUTBOUNDENTRY AS OUTBOUNDENTRY");
            sql.AppendFormat("INNER JOIN BAH_T_WMS_OUTBOUNDENTRY_W AS OUTBOUNDENTRYW ON OUTBOUNDENTRY.FENTRYID = OUTBOUNDENTRYW.FENTRYID AND OUTBOUNDENTRY.{0}", e.InSelectedRowsSQL);
            sql.AppendLine();
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY AS OUTNOTICEENTRY ON OUTBOUNDENTRYW.FORIGINID = OUTNOTICEENTRY.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICE AS OUTNOTICE ON OUTNOTICEENTRY.FID = OUTNOTICE.FID AND OUTNOTICE.FPHMXGenTargetStatus <> 'C'");

            StringBuilder filter = new StringBuilder();
            filter.AppendLine(e.InSelectedRowsSQL);
            filter.AppendLine(" AND ");
            filter.AppendFormat("{0} IN ({1})", e.PkKey, sql.ToString());

            e.InSelectedRowsSQL = filter.ToString();
        }
    }
}
