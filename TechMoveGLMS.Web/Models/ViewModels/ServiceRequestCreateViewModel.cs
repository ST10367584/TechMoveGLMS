namespace TechMoveGLMS.Web.Models.ViewModels;

public class ServiceRequestCreateViewModel
{
    public int ContractId { get; set; }
    public string ContractInfo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostUSD { get; set; }
    public decimal CostZAR { get; set; }
    public decimal ExchangeRate { get; set; }
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
}