using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using BAH.PI.BD.Contracts;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.WareHouse
{
    public class ReturnQtyForShow : AbstractWebApiBusinessService
    {
        public ReturnQtyForShow(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入fid,entryid
        /// </summary>
        /// <param name="billno">收货通知编号</param>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string SourceBillId, string SourceEntryId)
        {
            JSONObject return_data = new JSONObject();
            var result = new ServiceResult<JSONObject>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            if (string.IsNullOrWhiteSpace(SourceBillId))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "收货通知单据头主键不能为空！";
                return result;
            }
            if (string.IsNullOrWhiteSpace(SourceEntryId))
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "收货通知单据体主键不能为空！";
                return result;
            }
            //获取相关信息 
            try
            {
              
                //客户编码和物料编码

                string sqlSelect = string.Format(@"/*dialect*/
                SELECT t.FQty, t.FUnitId, t4.FNAME AS FUnitName,t.FMQty, T.FMUNITID,t9.FNAME AS FMUnitName, t.FPackageId,
                t5.FHASINBOUNDMQTY,(t.FMQTY - t5.FHASINBOUNDMQTY) AS FNeedINBOUNDMQTY,
                t.FAVGCTY,t.FCty
                FROM dbo.BAH_T_WMS_INNOTICEENTRY t          
	            LEFT JOIN dbo.BAH_T_BD_MATERIAL_L t3 ON t.FMATERIALID = t3.FID
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t4 ON t.FUNITID = t4.fid
	            LEFT JOIN dbo.BAH_T_WMS_INNOTICEENTRY_W t5 ON t.FENTRYID = t5.FENTRYID
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t9 ON t.FMUNITID = t9.fid
                WHERE t.FMANUALCLOSE = 'A'
	            AND t5.FJOINSTATUS = 'A'
	            AND t.FID =  '{0}' 
	            AND T.FENTRYID =  '{1}'
                order by t.FSEQ
                 ;", SourceBillId,SourceEntryId );// or a.num is null

               

                DynamicObjectCollection mat_objc = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);
                if (mat_objc.Count == 0)
                {
                    result.Code = (int)ResultCode.Fail;
                    result.Message = "未检索到对应信息！";
                }
                else
                {
                    List<JSONObject> detail_list = new List<JSONObject>(); //明细信息
                    foreach (DynamicObject data in mat_objc)
                    {
                        JSONObject each_detail = new JSONObject();
                        IPackageService pkgService = null;
                        String FMQtyForShow;
                        String FHASINBOUNDMQTYForShow;
                        String FNeedINBOUNDMQTYForShow;
                        try
                        {
                            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BAH_BD_Package") as FormMetadata;
                            QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
                            queryParam.FormId = "BAH_BD_Package";
                            queryParam.BusinessInfo = meta.BusinessInfo;

                            queryParam.FilterClauseWihtKey = " FID ='" + data["FPackageId"].ToString() + "' ";

                            var objs = BusinessDataServiceHelper.Load(ctx,
                                meta.BusinessInfo.GetDynamicObjectType(),
                                queryParam).FirstOrDefault();

                            pkgService = PIBDServiceFactory.Instance.GetService<IPackageService>(ctx);
                            var Marray = pkgService.Expand(ctx, objs, decimal.Parse( data["FMQty"].ToString()));
                            var Harray = pkgService.Expand(ctx, objs, decimal.Parse(data["FHASINBOUNDMQTY"].ToString()));
                            var Narray = pkgService.Expand(ctx, objs, decimal.Parse(data["FNeedINBOUNDMQTY"].ToString()));
                            FMQtyForShow = string.Join("", Marray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FHASINBOUNDMQTYForShow = string.Join("", Harray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FNeedINBOUNDMQTYForShow = string.Join("", Narray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                        }
                        finally
                        {
                            PIBDServiceFactory.Instance.CloseService(pkgService);
                        }

                        each_detail.Add("FQty", data["FQty"]);
                        each_detail.Add("FUnitId", data["FUnitId"]);

                        each_detail.Add("FUnitName", data["FUnitName"]);
                        each_detail.Add("FMQty", data["FMQty"]);
                        each_detail.Add("FMUNITID", data["FMUNITID"]);
                        each_detail.Add("FMUnitName", data["FMUnitName"]);

                        each_detail.Add("FMQtyForShow", FMQtyForShow);
                        each_detail.Add("FHASINBOUNDMQTYForShow", FHASINBOUNDMQTYForShow);
                        each_detail.Add("FNeedINBOUNDMQTYForShow", FNeedINBOUNDMQTYForShow);
                        each_detail.Add("FHASINBOUNDMQTY", data["FHASINBOUNDMQTY"]);
                        each_detail.Add("FNeedINBOUNDMQTY", data["FNeedINBOUNDMQTY"]);
                        each_detail.Add("FAVGCTY", data["FAVGCTY"]);
                        each_detail.Add("FCTY", data["FCTY"]);
                        detail_list.Add(each_detail);
                    }
                    return_data.Add("DetailList", detail_list);
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
