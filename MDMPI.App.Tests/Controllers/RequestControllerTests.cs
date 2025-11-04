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
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;

namespace MDMPI.App.Tests.Controllers
{
    public class RequestControllerTests
    {
        [Fact]
        public async void GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockRepo = new Mock<IRequestRepository>();
            mockRepo.Setup(r => r.GetAllRequestsAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestStandardDto> { new RequestStandardDto() });
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetRequestAll_ReturnsNotFound_WhenRepositoryReturnsNull()
        {
            var mockRepo = new Mock<IRequestRepository>();
            mockRepo.Setup(r => r.GetAllRequestsAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync((List<RequestStandardDto>?)null);
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestAll();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void GetCancelledRemarks_ReturnsOk_WhenRemarksExist()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.GetAllRemarks(It.IsAny<long>())).ReturnsAsync(new RemarksDto { Remarks = "x" });
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetCancelledRemarks(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void CancelRequest_ReturnsBadRequest_WhenIdInvalid()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.CancelRequest(0, "x");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void CancelRequest_ReturnsOk_WhenInsertRemarkSucceeds()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.InsertRemarkAndCancelRequestForStandardDeliveryAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(true);
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.CancelRequest(1, "x");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async void GetMobile_ReturnsOk_WhenMobileRepoReturnsData()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            mockMobile.Setup(m => m.GetAllMobilesAsync()).ReturnsAsync(new List<MobileDto> { new MobileDto { MobileID = 1, MobileName = "m1" } });
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetMobile();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetRequestImage_ReturnsFile_WhenImageExists()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();
            var bytes = new byte[] { 1 };
            mockImage.Setup(m => m.GetRequestImage(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(bytes);

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestImage("1", "t");

            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async void UploadImage_ReturnsOk_WhenUploadSucceeds()
        {
            var mockRepo = new Mock<IRequestRepository>();
            var mockMobile = new Mock<IMobileRepository>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var content = Encoding.UTF8.GetBytes("abc");
            var stream = new MemoryStream(content);
            IFormFile file = new FormFile(stream, 0, content.Length, "Image", "test.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

            mockImage.Setup(m => m.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("/path");

            var controller = new RequestController(mockRepo.Object, mockMobile.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UploadImageRequestDto { Image = file, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            Assert.IsType<OkResult>(result);
        }
    }
}
