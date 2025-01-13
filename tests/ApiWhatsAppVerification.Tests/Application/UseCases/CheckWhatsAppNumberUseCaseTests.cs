using ApiWhatsAppVerification.Application.Interfaces.Repositories;
using ApiWhatsAppVerification.Application.Interfaces.Services;
using ApiWhatsAppVerification.Application.UseCases;
using ApiWhatsAppVerification.Domain.Entities;
using Moq;

namespace ApiWhatsAppVerification.Tests.Application.UseCases
{
    public class CheckWhatsAppNumberUseCaseTests
    {
        private Mock<IPhoneNumberVerificationRepository> _mockRepo;
        private Mock<IWhatsAppVerifier> _mockVerifier;
        private CheckWhatsAppNumberUseCase _useCase;

        //[SetUp]
        //public void Setup()
        //{
        //    _mockRepo = new Mock<IPhoneNumberVerificationRepository>();
        //    _mockVerifier = new Mock<IWhatsAppVerifier>();
        //    _useCase = new CheckWhatsAppNumberUseCase(_mockRepo.Object, _mockVerifier.Object);
        //}

        //[Test]
        //public async Task ShouldSaveVerificationWhenNumberNotExists()
        //{
        //    // Arrange
        //    var phoneNumber = "123456789";
        //    _mockRepo.Setup(r => r.GetByPhoneNumberAsync(phoneNumber))
        //             .ReturnsAsync((PhoneNumberVerification)null);
        //    _mockVerifier.Setup(v => v.VerifyAsync(phoneNumber))
        //                 .ReturnsAsync(true);

        //    // Act
        //    var result = await _useCase.ExecuteAsync(phoneNumber);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.HasWhatsApp);
        //    _mockRepo.Verify(r => r.SaveAsync(It.IsAny<PhoneNumberVerification>()), Times.Once);
        //}
    }
}
