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

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutBound
{
    public class ReturnPickDetailBillNo : AbstractWebApiBusinessService
    {
        public ReturnPickDetailBillNo(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取未完成拣货通知和部分拣货的拣货通知
        /// </summary>
        ///参数为三个：仓库FID ，库区FID，货主FID
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string whid,string areaid,string ownerid, string trackno)
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
                                                     SELECT t0.FBILLNO,t0.FCREATEDATE AS FCREATEDATE,t4.FID as FWHID,T4.FNUMBER AS FWHNUMBER,T5.FNAME AS FWHNAME,
 T6.FID AS FOWNERID,T6.FNUMBER AS FOWNERNUMBER,T7.FNAME AS FOWNERNAME ,
 T8.FID AS FCONTACTID,T8.FNUMBER AS FCONTACTNUMBER,T9.FNAME AS FCONTACTNAME,T8.fformid AS FCONTACTFORMID,'未发货' AS FStatus, COUNT(t.FENTRYID) as COUNT
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                WHERE t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{1}%' AND T30.FAREAID LIKE '%{2}%' AND T2.FJOINMQTY = 0 AND t.FTOTRACKNO LIKE '%{3}%'
                group by t0.FBILLNO,T0.FCREATEDATE,t4.FID,T4.FNUMBER,T5.FNAME,T6.FID,T6.FNUMBER,T7.FNAME,T8.FID,T8.FNUMBER,T9.FNAME,T8.fformid 
                UNION ALL 
                 SELECT t0.FBILLNO,t0.FCREATEDATE AS FCREATEDATE,t4.FID as FWHID,T4.FNUMBER AS FWHNUMBER,T5.FNAME AS FWHNAME,
 T6.FID AS FOWNERID,T6.FNUMBER AS FOWNERNUMBER,T7.FNAME AS FOWNERNAME ,
 T8.FID AS FCONTACTID,T8.FNUMBER AS FCONTACTNUMBER,T9.FNAME AS FCONTACTNAME,T8.fformid AS FCONTACTFORMID,'部分发货' AS FStatus, COUNT(t.FENTRYID) as COUNT
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{1}%' AND T30.FAREAID LIKE '%{2}%' AND T2.FJOINMQTY < t.FTOMQTY AND T2.FJOINMQTY > 0 AND t.FTOTRACKNO LIKE '%{3}%'
                group by t0.FBILLNO,T0.FCREATEDATE,t4.FID,T4.FNUMBER,T5.FNAME,T6.FID,T6.FNUMBER,T7.FNAME,T8.FID,T8.FNUMBER,T9.FNAME,T8.fformid   
                order by FCREATEDATE                            


                   
                 ;", whid,ownerid,areaid,trackno);// or a.num is null
                string sqlSelect1 = string.Format(@"/*dialect*/
                                                     SELECT t0.FBILLNO,t0.FCREATEDATE AS FCREATEDATE,t4.FID as FWHID,T4.FNUMBER AS FWHNUMBER,T5.FNAME AS FWHNAME,
 T6.FID AS FOWNERID,T6.FNUMBER AS FOWNERNUMBER,T7.FNAME AS FOWNERNAME ,
 T8.FID AS FCONTACTID,T8.FNUMBER AS FCONTACTNUMBER,T9.FNAME AS FCONTACTNAME,T8.fformid AS FCONTACTFORMID,'未发货' AS FStatus, COUNT(t.FENTRYID) as COUNT
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                WHERE t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{1}%' AND T2.FJOINMQTY = 0 AND t.FTOTRACKNO LIKE '%{2}%'
                group by t0.FBILLNO,T0.FCREATEDATE,t4.FID,T4.FNUMBER,T5.FNAME,T6.FID,T6.FNUMBER,T7.FNAME,T8.FID,T8.FNUMBER,T9.FNAME,T8.fformid 
                UNION ALL 
                 SELECT t0.FBILLNO,t0.FCREATEDATE AS FCREATEDATE,t4.FID as FWHID,T4.FNUMBER AS FWHNUMBER,T5.FNAME AS FWHNAME,
 T6.FID AS FOWNERID,T6.FNUMBER AS FOWNERNUMBER,T7.FNAME AS FOWNERNAME ,
 T8.FID AS FCONTACTID,T8.FNUMBER AS FCONTACTNUMBER,T9.FNAME AS FCONTACTNAME,T8.fformid AS FCONTACTFORMID,'部分发货' AS FStatus, COUNT(t.FENTRYID) as COUNT
                FROM  dbo.BAH_T_WMS_PICKUP t0
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_B t ON t0.FID = t.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t1 ON t.FTOLOCID = t1.FID
	            LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W t2 ON t.FENTRYID = t2.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t3 ON t.FTOLOCID = t3.FID
                LEFT JOIN dbo.BAH_T_BD_LOCBASE T30 ON T3.FID = T30.FID
	            LEFT JOIN dbo.BAH_T_BD_WAREHOUSE t4 on t0.FTOWHID = t4.FID
	            INNER JOIN dbo.BAH_T_BD_WAREHOUSE_L t5 on t4.FID = t5.FID
	            LEFT JOIN dbo.BAH_V_BD_OWNER t6 ON t0.FTOOWNERID = t6.FID
	            INNER JOIN dbo.BAH_V_BD_OWNER_L T7 ON T6.FID = T7.FID
	            LEFT JOIN dbo.BAH_V_BD_CONTACT T8 ON T0.FCONTACTID = T8.FID
	            INNER JOIN dbo.BAH_V_BD_CONTACT_L T9 ON T8.FID = T9.FID
                WHERE  t0.FDOCUMENTSTATUS = 'C'
                AND T0.FTOWHID LIKE '%{0}%' AND T0.FTOOWNERID LIKE '%{1}%'  AND T2.FJOINMQTY < t.FTOMQTY AND T2.FJOINMQTY > 0 AND t.FTOTRACKNO LIKE '%{2}%'
                group by t0.FBILLNO,T0.FCREATEDATE,t4.FID,T4.FNUMBER,T5.FNAME,T6.FID,T6.FNUMBER,T7.FNAME,T8.FID,T8.FNUMBER,T9.FNAME,T8.fformid   
                order by FCREATEDATE                            


                   
                 ;", whid, ownerid,trackno);// or a.num is null
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
                      

                           
                      

                            data.Add("FBillNo", dataObject["FBILLNO"]);
                            data.Add("FDate", Convert.ToDateTime(dataObject["FCREATEDATE"].ToString()).ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            //表头仓库信息
                            data.Add("FBatchWHId", dataObject["FWHID"]);
                            data.Add("FBatchWHNumber", dataObject["FWHNUMBER"]);
                            data.Add("FBatchWHName", dataObject["FWHNAME"]);
                            //表头货主信息
                            data.Add("FBatchOwnerId", dataObject["FOWNERID"]);
                            data.Add("FBatchOwnerNumber", dataObject["FOWNERNUMBER"]);
                            data.Add("FBatchOwnerName", dataObject["FOWNERNAME"]);
                            //表头供应商信息
                            data.Add("FContactId", dataObject["FCONTACTID"]);
                            data.Add("FContactNumber", dataObject["FCONTACTNUMBER"]);
                            data.Add("FContactName", dataObject["FCONTACTNAME"]);
                            data.Add("FContactFormName", dataObject["FCONTACTFORMID"]);
                            data.Add("FStatus", dataObject["FStatus"]);
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
