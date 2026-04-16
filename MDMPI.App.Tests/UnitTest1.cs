using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MDMPI.App.Api.Controllers.Logistic;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Tests
{
    public class ControllerTests
    {
        [Fact]
        public async Task RequestController_GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockService = new Mock<IRequestService>();
            mockService.Setup(r => r.GetAllRequestsAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestStandardDto> { new RequestStandardDto() });
            var mockMobile = new Mock<IMobileService>();
            var mockRemarks = new Mock<IRemarksService>();
            var mockImage = new Mock<IImageService>();
            var controller = new RequestController(mockService.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RequestPickUpController_GetAll_ReturnsNotFound_WhenRepoReturnsNull()
        {
            var mockService = new Mock<IRequestPickUpService>();
            mockService.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync((List<RequestPickUpDto>?)null);
            var mockImage = new Mock<IImageService>();
            var mockRemarks = new Mock<IRemarksService>();
            var mockMobile = new Mock<IMobileService>();

            var controller = new RequestPickUpController(mockService.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.GetAll();

            Assert.IsType<NotFoundResult>(result);
        }
    }
}