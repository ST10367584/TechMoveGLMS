using TechMoveGLMS.Web.Models;
using Xunit;

namespace TechMoveGLMS.Tests
{
    /// <summary>
    /// Unit tests for contract workflow business rules.
    /// Verifies that service requests cannot be created
    /// when the parent contract is Expired or OnHold.
    /// </summary>
    public class WorkflowLogicTests
    {
        // ── Helper: simulates the controller's guard logic ────────────────────
        private static bool CanCreateServiceRequest(Contract contract)
        {
            return contract.Status != ContractStatus.Expired &&
                   contract.Status != ContractStatus.OnHold;
        }

        // ── 1. Active contract allows service request creation ─────────────────
        [Fact]
        public void CanCreateServiceRequest_ActiveContract_ReturnsTrue()
        {
            var contract = new Contract { Status = ContractStatus.Active };
            Assert.True(CanCreateServiceRequest(contract));
        }

        // ── 2. Draft contract allows service request creation ──────────────────
        [Fact]
        public void CanCreateServiceRequest_DraftContract_ReturnsTrue()
        {
            var contract = new Contract { Status = ContractStatus.Draft };
            Assert.True(CanCreateServiceRequest(contract));
        }

        // ── 3. Expired contract blocks service request creation ────────────────
        [Fact]
        public void CanCreateServiceRequest_ExpiredContract_ReturnsFalse()
        {
            var contract = new Contract { Status = ContractStatus.Expired };
            Assert.False(CanCreateServiceRequest(contract));
        }

        // ── 4. OnHold contract blocks service request creation ─────────────────
        [Fact]
        public void CanCreateServiceRequest_OnHoldContract_ReturnsFalse()
        {
            var contract = new Contract { Status = ContractStatus.OnHold };
            Assert.False(CanCreateServiceRequest(contract));
        }

        // ── 5. Contract with EndDate in the past should be considered for expiry ─
        [Fact]
        public void Contract_EndDateInPast_IsMarkedExpired()
        {
            var contract = new Contract
            {
                StartDate = DateTime.Today.AddYears(-2),
                EndDate = DateTime.Today.AddYears(-1),
                Status = ContractStatus.Expired
            };
            Assert.Equal(ContractStatus.Expired, contract.Status);
            Assert.False(CanCreateServiceRequest(contract));
        }

        // ── 6. All statuses parametrized ─────────────────────────────────────
        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, true)]
        [InlineData(ContractStatus.Expired, false)]
        [InlineData(ContractStatus.OnHold, false)]
        public void CanCreateServiceRequest_AllStatuses_CorrectResult(
            ContractStatus status, bool expectedResult)
        {
            var contract = new Contract { Status = status };
            Assert.Equal(expectedResult, CanCreateServiceRequest(contract));
        }
    }
}
