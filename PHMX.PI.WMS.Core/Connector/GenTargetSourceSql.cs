using BAH.BOS.Core.Const.BillStatus;
using BAH.BOS.Pattern;
using BAH.PI.WMS.Core.Const;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHMX.PI.WMS.Core.Connector
{
    /// <summary>
    /// 生成目标单据查询数据源的SQL。
    /// </summary>
    public abstract class GenTargetSourceSql
    {
        /// <summary>
        /// KSQL语句字符串。
        /// </summary>
        public string SqlString { get; set; }

        /// <summary>
        /// 单据主键参数名称。
        /// </summary>
        public string BillIdsParamName { get; set; }

        /// <summary>
        /// 构造方法。
        /// </summary>
        public GenTargetSourceSql()
        {
            this.BillIdsParamName = "@BillIds";
        }
    }//end class

    /// <summary>
    /// 从收货明细获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromInbound : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromInbound instance = new GenTargetSourceSqlFromInbound();

        public static GenTargetSourceSqlFromInbound Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromInbound()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("INBOUND.FID AS FBILLID,");
            sql.AppendLine("INBOUND.FBILLNO AS FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.InNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(INBOUNDENTRY.FMQTY) AS FQTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FPHMXWgt)AS FPHMXWgt,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_INBOUNDENTRY AS INBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUNDENTRY_W AS INBOUNDENTRYW ON INBOUNDENTRY.FENTRYID = INBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY AS INNOTICEENTRY ON INNOTICEENTRY.FENTRYID = INBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY_W AS INNOTICEENTRYW ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_INNOTICEENTRY_LK AS INNOTICEENTRYLK ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUND AS INBOUND ON INBOUNDENTRY.FID = INBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICE AS INNOTICE ON INNOTICEENTRY.FID = INNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON INBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON INBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendLine("(INNOTICE.FPHMXSourceFormId = INNOTICEENTRYW.FSOURCEFORMID OR INNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("INNOTICE.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendFormat("INBOUND.FOBJECTTYPEID = '{0}' AND", PIWMSFormPrimaryKey.Instance.Inbound());
            sql.AppendLine();
            sql.AppendFormat("INBOUNDENTRY.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("INBOUND.FID,");
            sql.AppendLine("INBOUND.FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(INBOUNDENTRY.FMQTY) > 0 AND SUM(INBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

    /// <summary>
    /// 从收货通知获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromInNotice : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromInNotice instance = new GenTargetSourceSqlFromInNotice();

        public static GenTargetSourceSqlFromInNotice Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromInNotice()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("INNOTICE.FID AS FBILLID,");
            sql.AppendLine("INNOTICE.FBILLNO AS FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.InNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(INBOUNDENTRY.FMQTY)AS FQTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FPHMXWgt)AS FPHMXWgt,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_INBOUNDENTRY AS INBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUNDENTRY_W AS INBOUNDENTRYW ON INBOUNDENTRY.FENTRYID = INBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY AS INNOTICEENTRY ON INNOTICEENTRY.FENTRYID = INBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY_W AS INNOTICEENTRYW ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_INNOTICEENTRY_LK AS INNOTICEENTRYLK ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUND AS INBOUND ON INBOUNDENTRY.FID = INBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICE AS INNOTICE ON INNOTICEENTRY.FID = INNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON INBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON INBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendLine("(INNOTICE.FPHMXSourceFormId = INNOTICEENTRYW.FSOURCEFORMID OR INNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("INBOUND.FOBJECTTYPEID = '{0}' AND", PIWMSFormPrimaryKey.Instance.Inbound());
            sql.AppendLine();
            sql.AppendFormat("INBOUND.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendFormat("INNOTICE.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("INNOTICE.FID,");
            sql.AppendLine("INNOTICE.FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(INBOUNDENTRY.FMQTY) > 0 AND SUM(INBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

    /// <summary>
    /// 从收货明细按收货通知汇总获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromInBoundGroupByInNotice : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromInBoundGroupByInNotice instance = new GenTargetSourceSqlFromInBoundGroupByInNotice();

        public static GenTargetSourceSqlFromInBoundGroupByInNotice Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromInBoundGroupByInNotice()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("INNOTICE.FID AS FBILLID,");
            sql.AppendLine("INNOTICE.FBILLNO AS FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.InNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(INBOUNDENTRY.FMQTY)AS FQTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("SUM(INBOUNDENTRY.FPHMXWgt)AS FPHMXWgt,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_INBOUNDENTRY AS INBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUNDENTRY_W AS INBOUNDENTRYW ON INBOUNDENTRY.FENTRYID = INBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY AS INNOTICEENTRY ON INNOTICEENTRY.FENTRYID = INBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICEENTRY_W AS INNOTICEENTRYW ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_INNOTICEENTRY_LK AS INNOTICEENTRYLK ON INNOTICEENTRY.FENTRYID = INNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INBOUND AS INBOUND ON INBOUNDENTRY.FID = INBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_INNOTICE AS INNOTICE ON INNOTICEENTRY.FID = INNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON INBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON INBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendLine("(INNOTICE.FPHMXSourceFormId = INNOTICEENTRYW.FSOURCEFORMID OR INNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("INBOUND.FOBJECTTYPEID = '{0}' AND", PIWMSFormPrimaryKey.Instance.Inbound());
            sql.AppendLine();
            sql.AppendFormat("INBOUND.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendFormat("INBOUND.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("INNOTICE.FID,");
            sql.AppendLine("INNOTICE.FBILLNO,");
            sql.AppendLine("INNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("INNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("INNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(INBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(INBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FENTRYID ELSE ISNULL(INNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN INNOTICE.FPHMXSourceFormId = INBOUNDENTRYW.FORIGINFORMID THEN INNOTICEENTRY.FID ELSE ISNULL(INNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN INBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN INBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(INBOUNDENTRY.FMQTY) > 0 AND SUM(INBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

    /// <summary>
    /// 从发货明细获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromOutbound : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromOutbound instance = new GenTargetSourceSqlFromOutbound();

        public static GenTargetSourceSqlFromOutbound Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromOutbound()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("OUTBOUND.FID AS FBILLID,");
            sql.AppendLine("OUTBOUND.FBILLNO AS FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.OutNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(OUTBOUNDENTRY.FMQTY) AS FQTY,");
            sql.AppendLine("SUM(OUTBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("SUM(OUTBOUNDENTRY.FPHMXWgt) AS FPHMXWgt,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_OUTBOUNDENTRY AS OUTBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUNDENTRY_W AS OUTBOUNDENTRYW ON OUTBOUNDENTRY.FENTRYID = OUTBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY AS OUTNOTICEENTRY ON OUTNOTICEENTRY.FENTRYID = OUTBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY_W AS OUTNOTICEENTRYW ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_OUTNOTICEENTRY_LK AS OUTNOTICEENTRYLK ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUND AS OUTBOUND ON OUTBOUNDENTRY.FID = OUTBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICE AS OUTNOTICE ON OUTNOTICEENTRY.FID = OUTNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON OUTBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON OUTBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendLine("(OUTNOTICE.FPHMXSourceFormId = OUTNOTICEENTRYW.FSOURCEFORMID OR OUTNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("OUTNOTICE.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendFormat("OUTBOUNDENTRY.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("OUTBOUND.FID,");
            sql.AppendLine("OUTBOUND.FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(OUTBOUNDENTRY.FMQTY) > 0 AND SUM(OUTBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

    /// <summary>
    /// 从发货通知获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromOutNotice : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromOutNotice instance = new GenTargetSourceSqlFromOutNotice();

        public static GenTargetSourceSqlFromOutNotice Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromOutNotice()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("OUTNOTICE.FID AS FBILLID,");
            sql.AppendLine("OUTNOTICE.FBILLNO AS FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.OutNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(OUTBOUNDENTRY.FMQTY)AS FQTY,");
            sql.AppendLine("SUM(OUTBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_OUTBOUNDENTRY AS OUTBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUNDENTRY_W AS OUTBOUNDENTRYW ON OUTBOUNDENTRY.FENTRYID = OUTBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY AS OUTNOTICEENTRY ON OUTNOTICEENTRY.FENTRYID = OUTBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY_W AS OUTNOTICEENTRYW ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_OUTNOTICEENTRY_LK AS OUTNOTICEENTRYLK ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUND AS OUTBOUND ON OUTBOUNDENTRY.FID = OUTBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICE AS OUTNOTICE ON OUTNOTICEENTRY.FID = OUTNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON OUTBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON OUTBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendFormat("OUTBOUND.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendLine("(OUTNOTICE.FPHMXSourceFormId = OUTNOTICEENTRYW.FSOURCEFORMID OR OUTNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("OUTNOTICE.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("OUTNOTICE.FID,");
            sql.AppendLine("OUTNOTICE.FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(OUTBOUNDENTRY.FMQTY) > 0 AND SUM(OUTBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

    /// <summary>
    /// 从发货通知明细按发货通知汇总获取生成来源的SQL。
    /// </summary>
    public class GenTargetSourceSqlFromOutboundGroupByOutNotice : GenTargetSourceSql
    {
        #region 单例定义

        private static readonly GenTargetSourceSqlFromOutboundGroupByOutNotice instance = new GenTargetSourceSqlFromOutboundGroupByOutNotice();

        public static GenTargetSourceSqlFromOutboundGroupByOutNotice Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region 构造方法

        public GenTargetSourceSqlFromOutboundGroupByOutNotice()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("SELECT");
            sql.AppendLine("OUTNOTICE.FID AS FBILLID,");
            sql.AppendLine("OUTNOTICE.FBILLNO AS FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId AS FSOURCEFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId AS FTARGETFORMID,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId AS FCONVERTRULEID,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))) AS FDATE,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END AS FSID,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END AS FSBILLID,");
            sql.AppendFormat("'{0}' AS FNOTICEFORMID,", PIWMSFormPrimaryKey.Instance.OutNotice());
            sql.AppendLine();
            sql.AppendLine("SUM(OUTBOUNDENTRY.FMQTY)AS FQTY,");
            sql.AppendLine("SUM(OUTBOUNDENTRY.FCTY)AS FCTY,");
            sql.AppendLine("SUM(OUTBOUNDENTRY.FPHMXWgt) AS FPHMXWgt,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END AS FLOTNO,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END AS FPRODUCEDATE,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END AS FEXPIRYDATE");
            sql.AppendLine("FROM BAH_T_WMS_OUTBOUNDENTRY AS OUTBOUNDENTRY");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUNDENTRY_W AS OUTBOUNDENTRYW ON OUTBOUNDENTRY.FENTRYID = OUTBOUNDENTRYW.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY AS OUTNOTICEENTRY ON OUTNOTICEENTRY.FENTRYID = OUTBOUNDENTRYW.FORIGINID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICEENTRY_W AS OUTNOTICEENTRYW ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYW.FENTRYID");
            sql.AppendLine("LEFT JOIN BAH_T_WMS_OUTNOTICEENTRY_LK AS OUTNOTICEENTRYLK ON OUTNOTICEENTRY.FENTRYID = OUTNOTICEENTRYLK.FENTRYID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTBOUND AS OUTBOUND ON OUTBOUNDENTRY.FID = OUTBOUND.FID");
            sql.AppendLine("INNER JOIN BAH_T_WMS_OUTNOTICE AS OUTNOTICE ON OUTNOTICEENTRY.FID = OUTNOTICE.FID");
            sql.AppendLine("INNER JOIN T_BD_MATERIALSTOCK AS MATERIALSTOCK ON OUTBOUNDENTRY.FMIRRORMTLID = MATERIALSTOCK.FMATERIALID");
            sql.AppendLine("INNER JOIN BAH_T_BD_MATWAREHOUSE AS MATWAREHOUSE ON OUTBOUNDENTRY.FMATERIALID = MATWAREHOUSE.FID");
            sql.AppendLine("WHERE");
            sql.AppendFormat("OUTBOUND.FPHMXGenTargetStatus <> '{0}' AND", LogicStatus.Instance.Yes());
            sql.AppendLine();
            sql.AppendLine("(OUTNOTICE.FPHMXSourceFormId = OUTNOTICEENTRYW.FSOURCEFORMID OR OUTNOTICEENTRYW.FSOURCEFORMID = '') AND");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId <> '' AND");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId <> '' AND");
            sql.AppendFormat("OUTBOUND.FID IN (SELECT FID FROM TABLE(fn_StrSplit({0},',',1)))", this.BillIdsParamName);
            sql.AppendLine();
            sql.AppendLine("GROUP BY");
            sql.AppendLine("OUTNOTICE.FID,");
            sql.AppendLine("OUTNOTICE.FBILLNO,");
            sql.AppendLine("OUTNOTICE.FPHMXSourceFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXTargetFormId,");
            sql.AppendLine("OUTNOTICE.FPHMXConvertRuleId,");
            sql.AppendLine("CONVERT(DATETIME,CONVERT(VARCHAR(4),YEAR(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),MONTH(OUTBOUND.FDATETIME)) || '-' || CONVERT(VARCHAR(2),DAYOFMONTH(OUTBOUND.FDATETIME))),");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FENTRYID ELSE ISNULL(OUTNOTICEENTRYLK.FSId, 0) END,");
            sql.AppendLine("CASE WHEN OUTNOTICE.FPHMXSourceFormId = OUTBOUNDENTRYW.FORIGINFORMID THEN OUTNOTICEENTRY.FID ELSE ISNULL(OUTNOTICEENTRYLK.FSBillId, 0) END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISBATCHMANAGE = 1 THEN OUTBOUNDENTRY.FLOTNO ELSE '' END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FPRODUCEDATE ELSE NULL END,");
            sql.AppendLine("CASE WHEN MATERIALSTOCK.FISKFPERIOD = 1 THEN OUTBOUNDENTRY.FEXPIRYDATE ELSE NULL END");
            sql.AppendLine("HAVING SUM(OUTBOUNDENTRY.FMQTY) > 0 AND SUM(OUTBOUNDENTRY.FCTY) >= 0");

            this.SqlString = sql.ToString();
        }//end constructor

        #endregion

    }//end class

}//end namespace
