using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS;
using Kingdee.BOS.Orm;

namespace PHMX.PI.WMS.App.ServicePlugIn.InNotice
{
    [Description("收货通知保存时自动提交、审核")]
    public class Save : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 保存操作完毕，事务结束之前，进行自动提交、审核
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            // 取到需要自动提交、审核的单据内码
            
            if(e.DataEntitys[0]["PHMXTargetFormId_Id"].ToString() != "SP_InStock")
            {
                object[] pkArray = (from p in e.DataEntitys
                                    select p[0]).ToArray();
                // 设置提交参数
                // using Kingdee.BOS.Orm;
                OperateOption submitOption = OperateOption.Create();
                submitOption.SetIgnoreWarning(this.Option.GetIgnoreWarning());
                submitOption.SetInteractionFlag(this.Option.GetInteractionFlag());
                submitOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());

                // 创建提交服务：using Kingdee.BOS.Contracts; using Kingdee.BOS.App;
                ISubmitService submitService = ServiceHelper.GetService<ISubmitService>();
                IOperationResult submitResult = submitService.Submit(
                                this.Context, this.BusinessInfo,
                                pkArray, "Submit", submitOption);

                // 判断提交结果，如果失败，则内部会抛出错误，回滚代码
                if (CheckOpResult(submitResult) == false)
                {
                    return;
                }

                // 构建操作可选参数对象
                OperateOption auditOption = OperateOption.Create();
                auditOption.SetIgnoreWarning(this.Option.GetIgnoreWarning());
                auditOption.SetInteractionFlag(this.Option.GetInteractionFlag());
                auditOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());

                // 构建单据主键参数
                List<KeyValuePair<object, object>> pkEntityIds = new List<KeyValuePair<object, object>>();
                foreach (var pkValue in pkArray)
                {
                    pkEntityIds.Add(new KeyValuePair<object, object>(pkValue, ""));
                }

                List<object> paras = new List<object>();
                paras.Add("1");
                paras.Add("");

                // 调用审核操作
                ISetStatusService setStatusService = ServiceHelper.GetService<ISetStatusService>();

                // 如下调用方式，需显示交互信息
                IOperationResult auditResult = setStatusService.SetBillStatus(this.Context,
                            this.BusinessInfo,
                            pkEntityIds,
                            paras,
                            "Audit",
                            auditOption);

                // 判断审核结果，如果失败，则内部会抛出错误，回滚代码
                if (CheckOpResult(auditResult) == false)
                {
                    return;
                }
            }
            else
            {

            }
        }
        /// <summary>
        /// 判断操作结果是否成功，如果不成功，则直接抛错中断进程
        /// </summary>
        /// <param name="opResult"></param>
        /// <returns></returns>
        private bool CheckOpResult(IOperationResult opResult)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null
                    && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示

                    // 传出交互提示完整信息对象
                    this.OperationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    this.OperationResult.Sponsor = opResult.Sponsor;

                    // 抛出错误，终止本次操作
                    throw new KDBusinessException("", "本次操作需要用户确认是否继续，暂时中断");
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {// 未知原因导致提交失败
                        throw new KDBusinessException("", "未知原因导致自动提交、审核失败！");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("自动提交、审核失败，失败原因：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }
    }
}
