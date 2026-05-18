using TechMoveGLMS.Web.Models;
using Xunit;

namespace TechMoveGLMS.Tests;

public class WorkflowLogicTests
{
    private bool CanCreate(Contract c) => c.Status != ContractStatus.Expired && c.Status != ContractStatus.OnHold;

    [Fact] public void Active_Allowed() => Assert.True(CanCreate(new Contract { Status = ContractStatus.Active }));
    [Fact] public void Draft_Allowed() => Assert.True(CanCreate(new Contract { Status = ContractStatus.Draft }));
    [Fact] public void Expired_Blocked() => Assert.False(CanCreate(new Contract { Status = ContractStatus.Expired }));
    [Fact] public void OnHold_Blocked() => Assert.False(CanCreate(new Contract { Status = ContractStatus.OnHold }));
}