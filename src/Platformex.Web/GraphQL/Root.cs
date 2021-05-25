using System;
using System.Linq;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{
    internal sealed class Root : ObjectGraphType<Object>
    {
        public Root(IPlatform platform, IServiceProvider provider)
        {
            var graphDomain = new GraphDomain(platform , provider);
            AddField(graphDomain.GetFieldType());
        }
    }
}