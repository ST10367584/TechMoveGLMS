using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Web.Models;

public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    [Display(Name = "Cost (USD)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostUSD { get; set; }

    [Display(Name = "Cost (ZAR)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostZAR { get; set; }

    [Display(Name = "Exchange Rate (USD→ZAR)")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    [Display(Name = "Status")]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Contract? Contract { get; set; }
}