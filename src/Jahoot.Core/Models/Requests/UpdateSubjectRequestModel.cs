using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class UpdateSubjectRequestModel
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; init; }

    [Required]
    public bool IsActive { get; init; }
}
