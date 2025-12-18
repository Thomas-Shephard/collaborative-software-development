using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class ForgotPasswordRequestModel : IEmailRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
