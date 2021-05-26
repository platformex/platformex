using System;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{
    internal sealed class Root : ObjectGraphType<object>
    {
        public Root(IPlatform platform, IServiceProvider provider)
        {
            var graphDomain = new GraphDomain(platform , provider);
            AddField(graphDomain.GetFieldType());
        }
    }
}