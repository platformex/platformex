using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{

    public interface IGraphQueryHandler
    {
        FieldType GetFieldType(Boolean isInput);
        IGraphType GetQueryItemType(Type modelType, Boolean isInput);
        Task<Object> ExecuteQuery(Dictionary<String, Object> arguments);
    }

    public interface IGraphQueryHandler<in TQuery, TResult> : IGraphQueryHandler where TQuery : IQuery<TResult>
    {
    }

}
