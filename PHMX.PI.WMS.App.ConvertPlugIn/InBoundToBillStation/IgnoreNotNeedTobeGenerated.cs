using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.App.ConvertPlugIn.InBoundToBillStation
{
    [Description("收货明细至单据中转站，忽略无需生成目标单据的数据源，单据转换插件。")]
    public class IgnoreNotNeedTobeGenerated : AbstractConvertPlugIn
    {
        public override void OnInSelectedRow(InSelectedRowEventArgs e)
        {
            base.OnInSelectedRow(e);

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT INBOUNDENTRY.FID");
            sql.AppendLine("FROM BAH_T_WMS_INBOUNDENTRY AS INBOUNDENTRY");
            sql.AppendFormat("INNER JOIN BAH_T_WMS_INBOUNDENTRY_W AS INBOUNDENTRYW ON INBOUNDENTRY.FENTRYID = INBOUNDENTRYW.FENTRYID AND INBOUNDENTRY.{0}", e.InSelectedRowsSQL);
            sql.AppendLine();
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY AS INNOTICEENTRY ON INBOUNDENTRYW.FORIGINID = INNOTICEENTRY.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICE AS INNOTICE ON INNOTICEENTRY.FID = INNOTICE.FID AND INNOTICE.FPHMXGenTargetStatus <> 'C'");

            StringBuilder filter = new StringBuilder();
            filter.AppendLine(e.InSelectedRowsSQL);
            filter.AppendLine(" AND ");
            filter.AppendFormat("{0} IN ({1})", e.PkKey, sql.ToString());

            e.InSelectedRowsSQL = filter.ToString();
        }
    }
}
