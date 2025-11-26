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
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;

namespace MDMPI.App.Tests.Controllers
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
            mockRepo.Setup(r => r.InsertAsync(It.IsAny<InsertRequestPullOutReturnPickUpDto>())).ReturnsAsync(false);
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.Insert(new InsertRequestPullOutReturnPickUpDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenRequestIdInvalid()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UpdateRequestPullOutReturnPickUpDto { RequestID = 0 };

            var result = await controller.Update(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUpdateFails()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<UpdateRequestPullOutReturnPickUpDto>())).ReturnsAsync(false);
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UpdateRequestPullOutReturnPickUpDto { RequestID = 123 };

            var result = await controller.Update(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCancelledRemarks_ReturnsOk_WhenRemarksExist()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.GetAllRemarks(It.IsAny<long>())).ReturnsAsync(new RemarksDto { Remarks = "a" });
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetCancelledRemarks(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCancelledRemarks_ReturnsNotFound_WhenRemarksNull()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.GetAllRemarks(It.IsAny<long>())).ReturnsAsync((RemarksDto?)null);
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetCancelledRemarks(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsBadRequest_WhenIdInvalid()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.CancelRequest(0, "JCA", "x");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsNotFound_WhenInsertRemarkReturnsFalse()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.InsertRemarkAndCancelRequestForPullOutReturnPickUp(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(false);
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.CancelRequest(1, "JCA", "x");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsOk_WhenInsertRemarkSucceeds()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            mockRemarks.Setup(r => r.InsertRemarkAndCancelRequestForPullOutReturnPickUp(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(true);
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.CancelRequest(1, "JCA", "x");

            var ok = Assert.IsType<OkObjectResult>(result);
            // anonymous object with message
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsBadRequest_WhenArgsMissing()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestImage(null!, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsNotFound_WhenImageNull()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();
            mockImage.Setup(m => m.GetRequestImage(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((byte[]?)null);

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestImage("1", "t");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsFile_WhenImageExists()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();
            var bytes = new byte[] { 1, 2, 3 };
            mockImage.Setup(m => m.GetRequestImage(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(bytes);

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var result = await controller.GetRequestImage("1", "t");

            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsValidationProblem_WhenModelStateInvalid()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);
            controller.ModelState.AddModelError("Image", "required");

            var dto = new UploadImageRequestDto();

            var result = await controller.UploadImage(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.IsType<ValidationProblemDetails>(obj.Value);
        }

        [Fact]
        public async Task UploadImage_ReturnsBadRequest_WhenNoImage()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UploadImageRequestDto { Image = null, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsOk_WhenUploadSucceeds()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var content = Encoding.UTF8.GetBytes("abc");
            var stream = new MemoryStream(content);
            IFormFile file = new FormFile(stream, 0, content.Length, "Image", "test.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

            mockImage.Setup(m => m.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("/path");

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UploadImageRequestDto { Image = file, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsServerError_WhenUploadFails()
        {
            var mockRepo = new Mock<IRequestPullOutReturnPickUpRepository>();
            var mockLogger = new Mock<ILogger<RequestPullOutReturnPickUpController>>();
            var mockRemarks = new Mock<IRequestRemarksRepository>();
            var mockImage = new Mock<IImagePathTypeRepository>();

            var content = Encoding.UTF8.GetBytes("abc");
            var stream = new MemoryStream(content);
            IFormFile file = new FormFile(stream, 0, content.Length, "Image", "test.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

            mockImage.Setup(m => m.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string?)null);

            var controller = new RequestPullOutReturnPickUpController(mockRepo.Object, mockLogger.Object, mockRemarks.Object, mockImage.Object);

            var dto = new UploadImageRequestDto { Image = file, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
