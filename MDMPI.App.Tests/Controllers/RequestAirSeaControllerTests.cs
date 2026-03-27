using MDMPI.App.Api.Controllers.Logistic;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MDMPI.App.Tests.Controllers
{
    public class RequestAirSeaControllerTests
    {
        [Fact]
        public async void GetAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockRepo = new Mock<IRequestAirSeaRepository>();
            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestAirSeaDto> { new RequestAirSeaDto() });
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();

            var controller = new RequestAirSeaController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void Insert_ReturnsBadRequest_WhenInsertFails()
        {
            var mockRepo = new Mock<IRequestAirSeaRepository>();
            mockRepo.Setup(r => r.InsertAsync(It.IsAny<InsertRequestAirSeaDto>())).ReturnsAsync((RequestAirSeaDto?)null);
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();

            var controller = new RequestAirSeaController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.Insert(new InsertRequestAirSeaDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void GetMobile_ReturnsOk_WhenMobileRepoReturnsData()
        {
            var mockRepo = new Mock<IRequestAirSeaRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            mockMobile.Setup(m => m.GetAllMobilesAsync()).ReturnsAsync(new List<MobileDto> { new MobileDto { MobileID = 1, MobileName = "m1" } });

            var controller = new RequestAirSeaController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.GetMobile();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
