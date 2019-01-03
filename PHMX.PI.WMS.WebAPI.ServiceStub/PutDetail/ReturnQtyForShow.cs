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

namespace PHMX.PI.WMS.WebAPI.ServiceStub.PutDetail
{
    public class ReturnQtyForShow : AbstractWebApiBusinessService
    {
        public ReturnQtyForShow(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 输入收货明细fid,entryid，输出收货明细详情
        /// </summary>
        /// <param name="billno">收货明细编号</param>
        /// <returns>返回服务结果。</returns>
        public ServiceResult ExecuteService(string SourceBillId, string SourceEntryId)
        {
            
            var result = new ServiceResult<JSONObject>();
            var ctx = this.KDContext.Session.AppContext;
            // 检查上下文对象
            if (this.IsContextExpired(result)) return result;
            // 检查传入参数
            //if (string.IsNullOrWhiteSpace(billno))
            //{
            //    result.Code = (int)ResultCode.Fail;
            //    result.Message = "收货明细编号不能为空！";
            //    return result;
            //}
            //获取相关信息 
            try
            {
                //TODO:通过平台动态引擎获取数据
              

                JSONObject return_data = new JSONObject();
              
                //客户编码和物料编码

                string sqlSelect = string.Format(@"/*dialect*/
               
               SELECT t.FQty, t.FUnitId,  t4.FNAME AS FUnitName,t.FMQty,T.FMUNITID,t9.FNAME AS FMUnitName, t.FPackageId,
	            t5.FHasJoinMQty,(t.FMQTY - t5.FHasJoinMQty) AS FNeedINBOUNDMQTY,
	            t.FAVGCTY,t.FCTY
                FROM dbo.BAH_T_WMS_INBOUNDENTRY t
	            LEFT JOIN dbo.BAH_V_BD_UNIT_L t4 ON t.FUNITID = t4.fid
	            LEFT JOIN dbo.BAH_T_WMS_INBOUNDENTRY_W t5 ON t.FENTRYID = t5.FENTRYID
                LEFT JOIN dbo.BAH_V_BD_UNIT_L t9 ON t.FMUNITID = t9.fid
                WHERE  t5.FJOINSTATUS = 'A'
	            AND t.FID =  '{0}' 
	            AND T.FENTRYID =  '{1}'
                order by t.FSEQ
                   
                 ;", SourceBillId, SourceEntryId);// or a.num is null

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
                        String FHASMQTYForShow;
                        String FNeedMQTYForShow;
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
                            var Marray = pkgService.Expand(ctx, objs, decimal.Parse(data["FMQty"].ToString()));
                            var Harray = pkgService.Expand(ctx, objs, decimal.Parse(data["FHasJoinMQty"].ToString()));
                            var Narray = pkgService.Expand(ctx, objs, decimal.Parse(data["FNeedINBOUNDMQTY"].ToString()));
                            FMQtyForShow = string.Join("", Marray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FHASMQTYForShow = string.Join("", Harray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
                            FNeedMQTYForShow = string.Join("", Narray.Select(item => string.Concat(item.Qty.ToTrimEndZeroString(), item.Name.Value(ctx))).ToArray());
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
                        each_detail.Add("FHASMQTYForShow", FHASMQTYForShow);
                        each_detail.Add("FNeedMQTYForShow", FNeedMQTYForShow);
                        each_detail.Add("FHASPutMQTY", data["FHasJoinMQty"]);
                        each_detail.Add("FNeedPutMQTY", data["FNeedINBOUNDMQTY"]);
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
