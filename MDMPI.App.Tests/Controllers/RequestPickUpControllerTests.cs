using System.IO;
using System.Text;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using MDMPI.App.Api.Controllers.Logistic;
using MDMPI.App.Api.Models;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;

namespace MDMPI.App.Tests.Controllers
{
    public class RequestPickUpControllerTests
    {
        [Fact]
        public async void GetAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockRepo = new Mock<IRequestPickUpRepository>();
            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestPickUpDto> { new RequestPickUpDto() });
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();

            var controller = new RequestPickUpController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void Insert_ReturnsBadRequest_WhenInsertFails()
        {
            var mockRepo = new Mock<IRequestPickUpRepository>();
            mockRepo.Setup(r => r.InsertAsync(It.IsAny<InsertRequestPickUpDto>())).ReturnsAsync(false);
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();

            var controller = new RequestPickUpController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.Insert(new InsertRequestPickUpDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void GetMobile_ReturnsOk_WhenMobileRepoReturnsData()
        {
            var mockRepo = new Mock<IRequestPickUpRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            mockMobile.Setup(m => m.GetAllMobilesAsync()).ReturnsAsync(new List<MobileDto> { new MobileDto { MobileID = 1, MobileName = "m1" } });

            var controller = new RequestPickUpController(mockRepo.Object, mockImage.Object, mockRemarks.Object, mockMobile.Object);

            var result = await controller.GetMobile();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
