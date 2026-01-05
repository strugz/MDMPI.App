using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MDMPI.App.Api.Controllers.Logistic;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;

namespace MDMPI.App.Tests.Logistic
{
    public class RequestPullOutReturnPickUpControllerTests
    {
        [Fact]
        public async Task GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestPullOutReturnPickUpDto> { new RequestPullOutReturnPickUpDto() });
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Insert_ReturnsBadRequest_WhenInsertFails()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            mockRepo.Setup(r => r.InsertAsync(It.IsAny<InsertRequestPullOutReturnPickUpDto>())).ReturnsAsync((RequestPullOutReturnPickUpDto?)null);
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.Insert(new InsertRequestPullOutReturnPickUpDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
