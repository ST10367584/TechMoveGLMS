namespace TechMoveGLMS.Web.Models.ViewModels;

public class ContractSearchViewModel
{
    public string? SearchTerm { get; set; }
    public ContractStatus? StatusFilter { get; set; }
    public int? ClientFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public List<Contract> Results { get; set; } = new();
    public List<Client> AllClients { get; set; } = new();
}