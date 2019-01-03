using System;
using System.Linq;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using BAH.BOS.WebAPI.ServiceStub;
using PHMX.PI.WMS.WebAPI.ServiceStub.OutboundDetailLinkInDetailDto;
using Nelibur.ObjectMapper;
using Kingdee.BOS.Log;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata;
using System.Collections.Generic;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Bill;
using System.Web.Script.Serialization;
using Kingdee.BOS.App.Data;

namespace PHMX.PI.WMS.WebAPI.ServiceStub.OutBound
{
    public class UploadPickDetailDataByBillNo : AbstractWebApiBusinessService
    {
        public UploadPickDetailDataByBillNo(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 整单上传PDA拣货信息 参数为发货通知单号

        public ServiceResult ExecuteService(string billno)
        {
            var result = new ServiceResult();

            //检查上下文对象。
            var ctx = this.KDContext.Session.AppContext;
            if (this.IsContextExpired(result)) return result;

        

            //检查输入参数。
            if (billno == null)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = "发货明细单号参数不能为空！";
                return result;
            }//end if

            try
            {
                string sqlSelect = string.Format(@"/*dialect*/
SELECT T.FID AS SourceBillId, T.FENTRYID AS SourceEntryId,T1.FSOURCEBILLNO
                FROM  dbo.BAH_T_WMS_PICKUPENTRY T
                LEFT JOIN dbo.BAH_T_WMS_PICKUPENTRY_W T1 ON T.FENTRYID = T1.FENTRYID
                WHERE  T1.FSOURCEBILLNO = '{0}'
                AND T1.FJOINSTATUS = 'A'
  
                 ;", billno);

                DynamicObjectCollection data = DBUtils.ExecuteDynamicObject(ctx, sqlSelect, null, null);//获取上传需要的entryID和Fid

                var op = CreateNewBillsFromInNoticeEntities(ctx, data);
                result.Code = op.IsSuccess ? (int)ResultCode.Success : (int)ResultCode.Fail;
                result.Message = op.GetResultMessage();
            }
            catch (Exception ex)
            {
                result.Code = (int)ResultCode.Fail;
                result.Message = ex.Message;
                Logger.Error(this.GetType().AssemblyQualifiedName, ex.Message, ex);
            }

            return result;
        }
        /// <summary>
        /// 关联发货明细批量新增发货明细。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="dataArray">发货明细关联发货通知数据实体数组。</param>
        /// <returns>返回新建保存事务结果。</returns>
        public IOperationResult CreateNewBillsFromInNoticeEntities(Context ctx, DynamicObjectCollection dataArray)
        {

            //取默认转换规则。
            var rule = ConvertServiceHelper.GetConvertRules(ctx, "BAH_WMS_Pickup", "BAH_WMS_Outbound")
                                     .Where(element => element.IsDefault)
                                     .FirstOrDefault();
            if (rule == null)
            {
                throw new KDBusinessException("RuleNotFound", "未找到拣货明细至发货明细之间，启用的转换规则，无法自动下推！");
            }

            ListSelectedRowCollection listSelectedRowCollection = new ListSelectedRowCollection();
            foreach (var data in dataArray)
            {
                var row = new ListSelectedRow(data["SourceBillId"].ToString(), data["SourceEntryId"].ToString(), 0, rule.SourceFormId) { EntryEntityKey = "FEntity" };
                listSelectedRowCollection.Add(row);
            }//end foreach

            //将需要传入的数量作为参数传递进下推操作，并执行下推操作。
            PushArgs args = new PushArgs(rule, listSelectedRowCollection.ToArray());
            var inDetailDataObjects = ConvertServiceHelper.Push(ctx, args)
                                                    .Adaptive(result => result.ThrowWhenUnSuccess(op => op.GetResultMessage()))
                                                    .Adaptive(result => result.TargetDataEntities.Select(entity => entity.DataEntity).ToArray());

            //修改明细行数据包。
            var inDetailMetadata = FormMetaDataCache.GetCachedFormMetaData(ctx, rule.TargetFormId);
            var inDetailBillView = inDetailMetadata.CreateBillView(ctx);
            var inDetailDynamicFormView = inDetailBillView as IDynamicFormViewService;
            var inDetailBusinessInfo = inDetailMetadata.BusinessInfo;
            var inDetailEntryEntity = inDetailBusinessInfo.GetEntity("FEntity");
            var inDetailEntryLink = inDetailBusinessInfo.GetForm().LinkSet.LinkEntitys.FirstOrDefault();

           

            //调用上传操作，将暂存、保存、提交、审核操作放置在同一事务中执行。
            return inDetailDataObjects.DoNothing(ctx, inDetailBusinessInfo, "Upload");
        }//end method
    }
}
