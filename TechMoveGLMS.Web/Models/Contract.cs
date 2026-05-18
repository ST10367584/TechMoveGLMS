using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Web.Models;

public enum ContractStatus { Draft, Active, Expired, OnHold }
public enum ServiceLevel { Basic, Standard, Premium, Enterprise }

public class Contract
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Status")]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [Display(Name = "Service Level")]
    public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Signed Agreement (PDF)")]
    public string? SignedAgreementPath { get; set; }

    [Display(Name = "Original File Name")]
    public string? SignedAgreementFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Client? Client { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}