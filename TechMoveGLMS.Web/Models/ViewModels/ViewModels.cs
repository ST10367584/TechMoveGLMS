using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Models.ViewModels
{
    public class ContractSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public ContractStatus? StatusFilter { get; set; }
        public int? ClientFilter { get; set; }
        public List<Contract> Results { get; set; } = new();
        public List<Client> AllClients { get; set; } = new();
    }

    public class ServiceRequestCreateViewModel
    {
        public int ContractId { get; set; }
        public string? ContractInfo { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRate { get; set; }
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    }

    public class ContractCreateViewModel
    {
        public Contract Contract { get; set; } = new();
        public IFormFile? SignedAgreement { get; set; }
        public List<Client> Clients { get; set; } = new();
    }
}
