using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.App.Data;

namespace PHMX.PI.WMS.WebAPI.ServiceStub
{
    public class ReturnBillNoCount : AbstractWebApiBusinessService
    {
        public ReturnBillNoCount(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取各未完成单据个数
        /// </summary>
        ///参数为三个：仓库FID ，库区FID，货主FID
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string whid,string areaid,string ownerid )
        {
            DynamicObjectCollection dataObjectCollection;
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
          
          
            //获取相关信息 
            try
            {
                JSONObject Finaldata = new JSONObject();
                List<JSONObject> return_data = new List<JSONObject>();
                // TODO: 通过平台动态引擎获取数据
                string sqlSelect = string.Format(@"/*dialect*/
                                                                    SELECT COUNT(T.FBILLNO) AS COUNT,'待收货数量' AS FName FROM 
               (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_INNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_INNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_INNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
	          
                WHERE t.FMANUALCLOSE = 'A' AND t2.FJOINSTATUS = 'A' AND t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' AND T2.FHASINBOUNDMQTY >= 0 AND T2.FHASINBOUNDMQTY <= T.FMQTY
                ) T
                UNION ALL
                
                SELECT COUNT(T1.FBILLNO) AS COUNT,'待上架数量' AS FName FROM
                (
                SELECT distinct t0.FBILLNO
                FROM  dbo.BAH_T_WMS_INBOUND t0
                LEFT JOIN dbo.BAH_T_WMS_INBOUNDENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_INBOUNDENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE t2.FJOINSTATUS = 'A' AND t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' AND T2.FJoinMQty < t.FMQTY AND T2.FJOINMQTY >= 0
                ) T1
                UNION ALL
                SELECT COUNT(T2.FBILLNO) AS COUNT,'待拣货数量' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' AND T2.FHasPickupMQty < t.FMQTY AND T2.FHasPickupMQty >= 0
                )T2
                UNION ALL
                SELECT COUNT(T3.FBILLNO) AS COUNT,'待发货数量(按跟踪号)' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FTOLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' AND T2.FJOINMQTY < t.FTOMQTY AND T2.FJOINMQTY >= 0
              
                 )    T3   
                 UNION ALL
                 SELECT COUNT(T4.FBILLNO) AS COUNT,'待发货数量(按单)' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{2}%' AND T30.FAREAID LIKE '%{1}%' AND T2.FHasPickupMQty > 0
                 AND t0.FBILLNO NOT IN
                (
                select FORIGINBILLNO from dbo.BAH_T_WMS_OUTBOUNDENTRY_W
                 )
                 ) T4             
                   
                 ;", whid,areaid,ownerid);// or a.num is null


                string sqlSelect1 = string.Format(@"/*dialect*/
                                                                    SELECT COUNT(T.FBILLNO) AS COUNT,'待收货数量' AS FName FROM 
               (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_INNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_INNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_INNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
	          
                WHERE t.FMANUALCLOSE = 'A' AND t2.FJOINSTATUS = 'A' AND t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{1}%'  AND T2.FHASINBOUNDMQTY >= 0 AND T2.FHASINBOUNDMQTY <= T.FMQTY
                ) T
                UNION ALL
                
                SELECT COUNT(T1.FBILLNO) AS COUNT,'待上架数量' AS FName FROM
                (
                SELECT distinct t0.FBILLNO
                FROM  dbo.BAH_T_WMS_INBOUND t0
                LEFT JOIN dbo.BAH_T_WMS_INBOUNDENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_INBOUNDENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE t2.FJOINSTATUS = 'A' AND t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{1}%'  AND T2.FJoinMQty < t.FMQTY AND T2.FJOINMQTY >= 0
                ) T1
                UNION ALL
                SELECT COUNT(T2.FBILLNO) AS COUNT,'待拣货数量' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{1}%' AND T2.FHasPickupMQty < t.FMQTY AND T2.FHasPickupMQty >= 0
                )T2
                UNION ALL
                SELECT COUNT(T3.FBILLNO) AS COUNT,'待发货数量(按跟踪号)' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FTOLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{1}%' AND T2.FJOINMQTY < t.FTOMQTY AND T2.FJOINMQTY >= 0
              
                 )    T3   
                 UNION ALL
                 SELECT COUNT(T4.FBILLNO) AS COUNT,'待发货数量(按单)' AS FName FROM
                (
                SELECT DISTINCT t0.FBILLNO
                FROM  dbo.BAH_T_WMS_OUTNOTICE t0
                LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_WMS_OUTNOTICEENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON t.FLOCID = T30.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FWHID LIKE '%{0}%' AND T0.FOWNERID LIKE '%{1}%'  AND T2.FHasPickupMQty > 0
                 AND t0.FBILLNO NOT IN
                (
                select FORIGINBILLNO from dbo.BAH_T_WMS_OUTBOUNDENTRY_W
                 )
                 ) T4             
                   
                 ;", whid, ownerid);// or a.num is null
                if (areaid == "")
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect1, null, null);
                }
                else
                {
                    dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
                }
                if (dataObjectCollection.Count == 0)
                {
                    result.Code = (int)ResultCode.Fail;
                    result.Message = "未检索到对应信息！";
                }
                else
                {
                   
                    foreach (DynamicObject dataObject in dataObjectCollection)
                    {
                        JSONObject data = new JSONObject();
                      

                           
                      

                      
                            data.Add("FName", dataObject["FName"]);
                        
                            data.Add("FCOUNT", dataObject["COUNT"]);
                        return_data.Add(data);
                    }
                }

                //返回数据
                result.Code = (int)ResultCode.Success;
                result.Data = return_data;
                result.Message = "成功返回数据！";
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
