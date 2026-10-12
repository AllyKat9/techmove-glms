using Xunit;
using Moq;
using TechMoveGLMS.API.Services;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.Tests.Tests.Unit
{
    public class ServiceRequestFactoryTests
    {
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly ServiceRequestFactory _factory;
        private readonly DateTime _now;

        public ServiceRequestFactoryTests()
        {
            _mockContractRepo = new Mock<IContractRepository>();
            _factory = new ServiceRequestFactory(_mockContractRepo.Object);
            _now = DateTime.Now;
        }

        // Tests that a servive request is successful for a valid active contract
        [Fact]
        public async Task CreateRequestAsync_WithValidActiveContract_ReturnsSuccess()
        {
            // Arrange
            var contract = new Contract
            {
                Id = 1,
                Status = ContractStatus.Active,
                StartDate = _now.AddDays(-30),
                EndDate = _now.AddDays(30)
            };

            _mockContractRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(contract);
            _mockContractRepo.Setup(r => r.CanCreateServiceRequest(contract))
                .Returns(true);

            // Act
            var result = await _factory.CreateRequestAsync(1, "Test Service", 100m, 19.50m);

            // Assert
            Assert.True(result.success);
            Assert.NotNull(result.request);
            Assert.Equal(100m, result.request.CostUSD);
            Assert.Equal(1950m, result.request.CostZAR);
            Assert.Equal(RequestStatus.Pending, result.request.Status);
        }

        // Tests failure when contract is expired
        [Fact]
        public async Task CreateRequestAsync_WithExpiredContract_ReturnsError()
        {
            // Arrange
            var contract = new Contract { Id = 1, Status = ContractStatus.Expired };

            _mockContractRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(contract);
            _mockContractRepo.Setup(r => r.CanCreateServiceRequest(contract))
                .Returns(false);

            // Act
            var result = await _factory.CreateRequestAsync(1, "Test Service", 100m, 19.50m);

            // Assert
            Assert.False(result.success);
            Assert.Null(result.request);
            Assert.Contains("Cannot create service request", result.errorMessage);
        }

        // Tests failure when contract is on hold
        [Fact]
        public async Task CreateRequestAsync_WithOnHoldContract_ReturnsError()
        {
            // Arrange
            var contract = new Contract { Id = 1, Status = ContractStatus.OnHold };

            _mockContractRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(contract);
            _mockContractRepo.Setup(r => r.CanCreateServiceRequest(contract))
                .Returns(false);

            // Act
            var result = await _factory.CreateRequestAsync(1, "Test Service", 100m, 19.50m);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Contract status is OnHold", result.errorMessage);
        }

        // Verifies that new requess can't be made on non-existent contracts
        [Fact]
        public async Task CreateRequestAsync_WithNonExistentContract_ReturnsError()
        {
            // Arrange
            _mockContractRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Contract?)null);

            // Act
            var result = await _factory.CreateRequestAsync(999, "Test Service", 100m, 19.50m);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Contract not found", result.errorMessage);
        }

        // Verifies that amounts are converted correctly to Rands
        [Fact]
        public async Task CreateRequestAsync_CalculatesZarCorrectly()
        {
            // Arrange
            var contract = new Contract { Id = 1, Status = ContractStatus.Active };
            _mockContractRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(contract);
            _mockContractRepo.Setup(r => r.CanCreateServiceRequest(contract))
                .Returns(true);

            decimal[] testCases = { 100m, 50.50m, 0m, 1000m };
            decimal[] rates = { 19.50m, 18.75m, 20.00m };

            foreach (var usdAmount in testCases)
            {
                foreach (var rate in rates)
                {
                    // Act
                    var result = await _factory.CreateRequestAsync(1, "Test", usdAmount, rate);

                    // Assert
                    Assert.Equal(usdAmount * rate, result.request.CostZAR);
                    Assert.Equal(rate, result.request.ExchangeRateUsed);
                }
            }
        }
    }
}
