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
    public class ReturnWareHouse : AbstractWebApiBusinessService
    {
        public ReturnWareHouse(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取仓库数据
        /// </summary>
    
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService()
        {
            var result = new ServiceResult<List<JSONObject>>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
           
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
                string sqlSelect = string.Format(@"/*dialect*/
                 SELECT T.FID,T.FNUMBER,T1.FNAME,
T3.FINLOCID AS FMulInLocId,T7.FNUMBER AS FMulInLocIdNumber,T5.FNAME AS FMulInLocIdName,
CASE WHEN T9.FIGNOREINVENTORYTRACKNO = 1 Then 'True' WHEN T9.FIGNOREINVENTORYTRACKNO = 0 THEN 'False' ELSE '' End AS FInIgnoreInventoryTrackNo,
T4.FOUTLOCID AS FMulOutLocId,T8.FNUMBER AS FMulOutLocIdNumber,T6.FNAME AS FMulOutLocIdName,
CASE WHEN T10.FIGNOREINVENTORYTRACKNO = 1 Then 'True' WHEN T10.FIGNOREINVENTORYTRACKNO = 0 THEN 'False' ELSE '' End AS FOutIgnoreInventoryTrackNo
FROM dbo.BAH_T_BD_WAREHOUSE T
LEFT JOIN dbo.BAH_T_BD_WAREHOUSE_L T1 ON T.FID = T1.FID
LEFT JOIN dbo.BAH_T_BD_WHBASE T2 ON T.FID = T2.FID
LEFT JOIN BAH_T_BD_WHINLOC T3 ON T2.FENTRYID = T3.FENTRYID
LEFT JOIN BAH_T_BD_WHOUTLOC T4 ON T2.FENTRYID = T4.FENTRYID
LEFT JOIN BAH_T_BD_LOCATION_L T5 ON T3.FINLOCID = T5.FID
LEFT JOIN BAH_T_BD_LOCATION_L T6 ON T4.FOUTLOCID = T6.FID
LEFT JOIN BAH_T_BD_LOCATION T7 ON T3.FINLOCID = T7.FID
LEFT JOIN BAH_T_BD_LOCATION T8 ON T4.FOUTLOCID = T8.FID
LEFT JOIN BAH_T_BD_LOCCONTROL T9 ON T3.FINLOCID = T9.FID
LEFT JOIN dbo.BAH_T_BD_LOCCONTROL T10 ON T4.FOUTLOCID = T10.FID
WHERE T2.FSHAPE = 'Physical' AND T.FDOCUMENTSTATUS = 'C' AND T.FFORBIDSTATUS = 'A'
;");// or a.num is null

                DynamicObjectCollection dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
                JSONObject Finaldata = new JSONObject();
                List<JSONObject> return_data = new List<JSONObject>();
                foreach (DynamicObject dataObject in dataObjectCollection)
                {
                    JSONObject data = new JSONObject();
                    data.Add("FID", dataObject["FID"].ToString());
                    data.Add("FNUMBER", dataObject["FNUMBER"].ToString());
                    data.Add("FName", dataObject["FNAME"].ToString());
                    if(dataObject["FMulInLocId"] != null)
                    {
                        data.Add("FMulInLocId", dataObject["FMulInLocId"].ToString());
                        data.Add("FMulInLocIdNumber", dataObject["FMulInLocIdNumber"].ToString());
                        data.Add("FMulInLocIdName", dataObject["FMulInLocIdName"].ToString());
                        data.Add("FInIgnoreInventoryTrackNo", dataObject["FInIgnoreInventoryTrackNo"].ToString());
                    }
                    else
                    {
                        data.Add("FMulInLocId", "");
                        data.Add("FMulInLocIdNumber", "");
                        data.Add("FMulInLocIdName", "");
                        data.Add("FInIgnoreInventoryTrackNo", "");
                    }
                    if(dataObject["FMulOutLocId"] != null)
                    {
                        data.Add("FMulOutLocId", dataObject["FMulOutLocId"].ToString());
                        data.Add("FMulOutLocIdNumber", dataObject["FMulOutLocIdNumber"].ToString());
                        data.Add("FMulOutLocIdName", dataObject["FMulOutLocIdName"].ToString());
                        data.Add("FOutIgnoreInventoryTrackNo", dataObject["FOutIgnoreInventoryTrackNo"].ToString());
                    }
                    else
                    {
                        data.Add("FMulOutLocId", "");
                        data.Add("FMulOutLocIdNumber", "");
                        data.Add("FMulOutLocIdName", "");
                        data.Add("FOutIgnoreInventoryTrackNo", "");
                    }



                    return_data.Add(data);
                }
                Finaldata.Add("WareHouse", return_data);
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
