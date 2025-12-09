using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class CreateSubjectRequestModel
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }
}
