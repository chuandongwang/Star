using PHMX.PI.WMS.Core.Connector;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace PHMX.PI.WMS.Contracts
{
    /// <summary>
    /// 同步服务接口。
    /// </summary>
    [RpcServiceError, ServiceContract]
    public interface IConnectorService
    {
        /// <summary>
        /// 从单据获取生成目标单据的数据源。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="sql">SQL语句。</param>
        /// <param name="billIds">单据主键。</param>
        /// <returns>返回查询的结果集。</returns>
        [FaultContract(typeof(ServiceFault)), OperationContract]
        GenTargetArgs[] GetGenTargetSource(Context ctx, GenTargetSourceSql sql, string objectTypeId, object[] billIds);

        /// <summary>
        /// 执行下推生成目标单据。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="args">生成目标单据参数。</param>
        /// <returns>返回操作结果。</returns>
        [FaultContract(typeof(ServiceFault)), OperationContract]
        IOperationResult Push(Context ctx, GenTargetArgs[] args);

        /// <summary>
        /// 执行上查删除目标单据。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="genArgs">生成目标单据参数。</param>
        /// <returns>返回操作结果。</returns>
        [FaultContract(typeof(ServiceFault)), OperationContract]
        IOperationResult Pull(Context ctx, GenTargetArgs[] genArgs);

        /// <summary>
        /// 标记状态。
        /// </summary>
        /// <param name="ctx">上下文对象。</param>
        /// <param name="businessInfo">业务对象。</param>
        /// <param name="dataEntities">数据包。</param>
        /// <param name="statusValue">状态值。</param>
        /// <param name="fieldKey">状态字段。</param>
        [FaultContract(typeof(ServiceFault)), OperationContract]
        void MarkGenStatus(Context ctx, BusinessInfo businessInfo, DynamicObject[] dataEntities, string statusValue, string fieldKey = "FPHMXGenTargetStatus");
    }
}
