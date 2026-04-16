using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MDMPI.App.Api.Controllers.Logistic;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Tests.Logistic
{
    public class RequestPullOutReturnPickUpControllerTests
    {
        [Fact]
        public async Task GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockService = new Mock<IRequestPullOutReturnPickUpService>();
            mockService.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestPullOutReturnPickUpDto> { new RequestPullOutReturnPickUpDto() });
            var mockRemarks = new Mock<IRemarksService>();
            var mockImage = new Mock<IImageService>();

            var controller = new RequestPullOutReturnPickUpController(mockService.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Insert_ReturnsBadRequest_WhenInsertFails()
        {
            var mockService = new Mock<IRequestPullOutReturnPickUpService>();
            mockService.Setup(r => r.InsertAsync(It.IsAny<InsertRequestPullOutReturnPickUpDto>())).ReturnsAsync((RequestPullOutReturnPickUpDto?)null);
            var mockRemarks = new Mock<IRemarksService>();
            var mockImage = new Mock<IImageService>();

            var controller = new RequestPullOutReturnPickUpController(mockService.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.Insert(new InsertRequestPullOutReturnPickUpDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
