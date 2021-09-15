using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;

namespace Platformex.Web
{
    public static class UseExtensions
    {
        private static readonly List<Action<IApplicationBuilder>> PreUseActions = new List<Action<IApplicationBuilder>>();
        private static readonly List<Action<IApplicationBuilder>> PostUseActions = new List<Action<IApplicationBuilder>>();

        public static void AddPreUseAction(Action<IApplicationBuilder> action) => PreUseActions.Add(action);
        public static void AddPostUseAction(Action<IApplicationBuilder> action) => PostUseActions.Add(action);

        public static IApplicationBuilder UsePlatformex(this IApplicationBuilder app)
        {
            foreach (var action in PreUseActions)
            {
                action(app);
            }
            app.UseMiddleware<PlatformexMiddleware>();

            foreach (var action in PostUseActions)
            {
                action(app);
            }
            return app;
        }
    }
}