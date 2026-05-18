namespace TechMoveGLMS.Web.Models.ViewModels;

public class ContractCreateViewModel
{
    public Contract Contract { get; set; } = new();
    public List<Client> Clients { get; set; } = new();
    public IFormFile? SignedAgreement { get; set; }
}