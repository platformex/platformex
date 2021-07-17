using System;
using System.Linq;
using System.Reflection;

namespace Platformex.Domain
{
    public class SecurityContext
    {
        private string[] _roles;

        public bool IsAuthorized => UserId != null;

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        internal SecurityContext(ICommonMetadata metadata)
        {
            LoadMetadata(metadata);
        }

        //Загрузка информации о пользователе изх метаданных
        // ReSharper disable once UnusedParameter.Local
        private void LoadMetadata(ICommonMetadata metadata)
        {
            //TODO: доделать при реализации интеграции с IdentityServer4
            //Пока тестовые данные
            UserId = Guid.NewGuid().ToString();
            _roles = new[] { "admin", "user" };
            UserName = "test_user";
        }

        public void HasRoles(params string[] roles)
        {
            if (!CheckRoles(roles))
                throw new UnauthorizedAccessException();
        }

        public bool CheckRoles(params string[] roles) => roles.Length == 0 || _roles.Intersect(roles).Count() != roles.Length;
        
        internal static string[] GetRolesFrom(object obj)
        {
            var art = obj?.GetType().GetCustomAttribute<HasRolesAttribute>();
            return art?.Roles ?? Array.Empty<string>();
        }
        internal static bool IsUserRequiredFrom(object obj)
        {
            return obj?.GetType().GetCustomAttribute<AuthorizedAttribute>() != null;
        }
    }
    
}