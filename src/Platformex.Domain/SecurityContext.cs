using System;
using System.Linq;
using System.Reflection;

namespace Platformex.Domain
{
    public class SecurityContext
    {
#pragma warning disable 649
        private string[] _roles;
#pragma warning restore 649

        public bool IsAuthorized => UserId != null;

        public string UserId { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string UserName { get; private set; }

        internal SecurityContext(ICommonMetadata metadata)
        {
            LoadMetadata(metadata);
        }

        //Загрузка информации о пользователе изх метаданных
        private void LoadMetadata(ICommonMetadata metadata)
        {
            /*//Пока тестовые данные
            UserId = Guid.NewGuid().ToString();
            _roles = new[] { "admin", "user" };
            UserName = "test_user";*/

            UserId = metadata.UserId;
            UserName = metadata.UserName;
            _roles = metadata.Roles?.Split(",") ?? Array.Empty<string>();
        }

        public void HasRoles(params string[] roles)
        {
            if (!CheckRoles(roles))
                throw new ForbiddenException();
        }

        public bool CheckRoles(params string[] roles) => 
            // ReSharper disable once AssignNullToNotNullAttribute
            _roles == null || roles?.Length == 0 || _roles.Intersect(roles).Count() == roles.Length;

        internal static string[] GetRolesFrom(object obj)
        {
            var art = obj?.GetType().GetCustomAttribute<HasRolesAttribute>();
            return art != null ? art.Roles ?? Array.Empty<string>() : Array.Empty<string>();
        }
        internal static bool IsUserRequiredFrom(object obj)
            => obj?.GetType().GetCustomAttribute<AuthorizedAttribute>() != null;
    }

}