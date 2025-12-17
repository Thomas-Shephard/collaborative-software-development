using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class EnsureUniqueElementsAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not IEnumerable list)
        {
            return true;
        }

        HashSet<object?> seen = [];
        return list.Cast<object?>().All(item => seen.Add(item));
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The list {name} must not contain duplicate elements.";
    }
}
