using System;

namespace Platformex
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HasRolesAttribute : Attribute
    {
        public string[] Roles { get; }

        public HasRolesAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}