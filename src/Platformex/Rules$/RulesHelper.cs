using System;
using System.Reflection;
using FluentValidation.Results;

namespace Platformex
{
    public static class RulesHelper
    {
        public static ValidationResult ProcessRules(object command)
        {
            var rulesAttribute = command.GetType().GetCustomAttribute<RulesAttribute>();
            if (rulesAttribute == null) return null;

            var rules = (IRules) Activator.CreateInstance(rulesAttribute.RulesType);
            return rules.Validate(command);

        }
    }
}