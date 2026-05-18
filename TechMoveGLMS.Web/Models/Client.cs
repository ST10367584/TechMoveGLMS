using System.ComponentModel.DataAnnotations;

namespace TechMoveGLMS.Web.Models;

public class Client
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(150)]
    [Display(Name = "Client Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact person is required.")]
    [StringLength(150)]
    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required.")]
    [StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}