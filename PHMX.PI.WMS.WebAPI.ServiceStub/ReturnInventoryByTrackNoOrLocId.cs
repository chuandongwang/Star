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
using BAH.PI.BD.Contracts;

namespace PHMX.PI.WMS.WebAPI.ServiceStub
{
    public class ReturnInventoryByTrackNoOrLocId : AbstractWebApiBusinessService
    {
        public ReturnInventoryByTrackNoOrLocId(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 获取库存明细
        /// </summary>
        ///参数为跟踪号或库位编码
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string TrackNoOrLocId)
        {
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
                SELECT DISTINCT t.FMATERIALID,T3.FNAME AS FMATERIALNAME,T2.FNUMBER AS FMATERIALNUMBER ,t3.FSPECIFICATION,
      CASE T10.FIGNOREINVENTORYTRACKNO WHEN 1 then 'True' ELSE 'False' END AS FIGNOREINVENTORYTRACKNO,
      CASE T11.FENABLELOT WHEN 1 then 'True' ELSE 'False' END AS FENABLELOT ,
	            CASE T11.FENABLEEXPIRY WHEN 1 then 'True' ELSE 'False' END AS FENABLEEXPIRY,
	            CASE T11.FENABLECAPACITY WHEN 1 then 'True' ELSE 'False' END AS FENABLECAPACITY,
	            t11.FCAPACITYSCALE,t12.FCAPACITYUNIT,
      t.FBILLNO AS FTrackNo,T.FLOTNO,FPRODUCEDATE,FEXPIRYDATE,FMQTY,FMAINUNITID, t8.FNAME AS FMainUnitNAME,
      t.FEXPPERIOD, 'D' AS FEXPUNIT,T.FAVGCTY,T.FCTY,
      FLOCID,t6.FNUMBER AS FLocNumber,t5.FNAME AS FLocName, t.FPACKAGEID,t4.FNUMBER AS FPackageNumber ,t7.FNAME AS FPackageName,t.FCOMBINEID
                FROM dbo.BAH_V_WMS_INVENTORY t
                LEFT JOIN dbo.BAH_T_BD_MATERIAL t2 ON t.FMATERIALID = t2.FID
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL_L t3 ON t.FMATERIALID = t3.FID
	            LEFT JOIN dbo.BAH_T_BD_PACKAGE T4 ON T.FPACKAGEID = T4.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION_L t5 ON t.FLOCID = t5.FID
	            LEFT JOIN dbo.BAH_T_BD_LOCATION t6 ON t.FLOCID = t6.FID
	            LEFT JOIN dbo.BAH_T_BD_PACKAGE_L T7 ON T.FPACKAGEID = T7.FID
	            LEFT JOIN dbo.BAH_T_BD_PKGUOM_L T8 ON T4.FMAINUNITID = t8.FENTRYID
	            LEFT JOIN dbo.BAH_T_BD_LOCBASE T9 ON T6.FID = T9.FID
                INNER JOIN BAH_T_BD_LOCCONTROL T10 ON T6.FID = T10.FID
               INNER JOIN dbo.BAH_T_BD_MATWAREHOUSE T11 ON T2.FID = T11.FID
               LEFT JOIN dbo.BAH_T_BD_MATWAREHOUSE_L T12 ON T11.FENTRYID = T12.FENTRYID
	            where  (t.FBILLNO = '{0}' OR t6.FNUMBER = '{0}') AND (t9.FUSE = 'Storage' or t9.FUSE = 'Pick')
                 ;", TrackNoOrLocId);// or a.num is null

                DynamicObjectCollection dataObjectCollection = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
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

                        IPackageService pkgService = null;
                        String FMQtyForShow;
                     try
                        {
                            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BAH_BD_Package") as FormMetadata;
                            QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
                            queryParam.FormId = "BAH_BD_Package";
                            queryParam.BusinessInfo = meta.BusinessInfo;
                            queryParam.FilterClauseWihtKey = " FID ='" + dataObject["FPackageId"].ToString() + "' ";
                            var objs = BusinessDataServiceHelper.Load(ctx,
                                meta.BusinessInfo.GetDynamicObjectType(),
                                queryParam).FirstOrDefault();

                            pkgService = PIBDServiceFactory.Instance.GetService<IPackageService>(ctx);
                            var Marray = pkgService.Expand(ctx, objs, decimal.Parse(dataObject["FMQty"].ToString()));
                            FMQtyForShow = string.Join("", Marray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                        }
                        finally
                        {
                            PIBDServiceFactory.Instance.CloseService(pkgService);
                        }

                        data.Add("FMATERIALID", dataObject["FMATERIALID"].ToString());
                        data.Add("FMATERIALNAME", dataObject["FMATERIALNAME"]);
                        data.Add("FMATERIALNUMBER", dataObject["FMATERIALNUMBER"]);
                        data.Add("FSPECIFICATION", dataObject["FSPECIFICATION"]);

                        data.Add("FIGNOREINVENTORYTRACKNO", dataObject["FIGNOREINVENTORYTRACKNO"]);
                        data.Add("FENABLELOT", dataObject["FENABLELOT"]);
                        data.Add("FENABLEEXPIRY", dataObject["FENABLEEXPIRY"]);
                        data.Add("FENABLECAPACITY", dataObject["FENABLECAPACITY"]);

                        data.Add("FCAPACITYSCALE", dataObject["FCAPACITYSCALE"]);
                        data.Add("FCAPACITYUNIT", dataObject["FCAPACITYUNIT"]);

                        data.Add("FTrackNo", dataObject["FTrackNo"]);
                        data.Add("FLotNo", dataObject["FLotNo"].ToString());

                        if (dataObject["FProduceDate"].ToString() == "0001-01-01 00:00:00")
                        {
                            data.Add("FProduceDate", "");
                        }
                        else
                        {
                            data.Add("FProduceDate", Convert.ToDateTime(dataObject["FProduceDate"].ToString()).ToString("yyyy-MM-dd"));
                        }

                        if (dataObject["FExpiryDate"].ToString() == "0001-01-01 00:00:00")
                        {
                            data.Add("FExpiryDate", "");
                        }
                        else
                        {
                            data.Add("FExpiryDate", Convert.ToDateTime(dataObject["FExpiryDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }

                        data.Add("FMQty", dataObject["FMQty"].ToString());
                        data.Add("FMainUnitId", dataObject["FMainUnitId"]);
                        data.Add("FMainUnitNAME", dataObject["FMainUnitNAME"]);

                        data.Add("FAVGCTY", dataObject["FAVGCTY"]);
                        data.Add("FCTY", dataObject["FCTY"]);
                        data.Add("FEXPPERIOD", dataObject["FEXPPERIOD"]);
                        data.Add("FEXPUNIT", dataObject["FEXPUNIT"]);

                        data.Add("FLocId", dataObject["FLocId"]);
                        data.Add("FLocNumber", dataObject["FLocNumber"]);
                        data.Add("FLocName", dataObject["FLocName"]);

                        data.Add("FPackageId", dataObject["FPackageId"]);
                        data.Add("FPackageNumber", dataObject["FPackageNumber"]);
                        data.Add("FPackageName", dataObject["FPackageName"]);
                        data.Add("FMQtyForShow", FMQtyForShow);
                        data.Add("FCOMBINEID", dataObject["FCOMBINEID"]);
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
