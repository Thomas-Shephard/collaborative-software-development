using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class AssignLecturerRoleRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
    
    public bool IsAdmin { get; init; }
}
