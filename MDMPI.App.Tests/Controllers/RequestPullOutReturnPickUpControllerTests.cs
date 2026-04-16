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
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Tests.Controllers
{
    public class RequestPullOutReturnPickUpControllerTests
    {
        private RequestPullOutReturnPickUpController CreateController(
            Mock<IRequestPullOutReturnPickUpService>? mockService = null,
            Mock<IRemarksService>? mockRemarks = null,
            Mock<IImageService>? mockImage = null)
        {
            return new RequestPullOutReturnPickUpController(
                (mockService ?? new Mock<IRequestPullOutReturnPickUpService>()).Object,
                (mockRemarks ?? new Mock<IRemarksService>()).Object,
                (mockImage ?? new Mock<IImageService>()).Object);
        }

        [Fact]
        public async Task GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()
        {
            var mockService = new Mock<IRequestPullOutReturnPickUpService>();
            mockService.Setup(r => r.GetAllAsync(It.IsAny<RequestQueryDto>())).ReturnsAsync(new List<RequestPullOutReturnPickUpDto> { new RequestPullOutReturnPickUpDto() });

            var controller = CreateController(mockService: mockService);

            var result = await controller.GetRequestAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Insert_ReturnsBadRequest_WhenInsertFails()
        {
            var mockService = new Mock<IRequestPullOutReturnPickUpService>();
            mockService.Setup(r => r.InsertAsync(It.IsAny<InsertRequestPullOutReturnPickUpDto>())).ReturnsAsync((RequestPullOutReturnPickUpDto?)null);

            var controller = CreateController(mockService: mockService);

            var result = await controller.Insert(new InsertRequestPullOutReturnPickUpDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenRequestIdInvalid()
        {
            var controller = CreateController();

            var dto = new UpdateRequestPullOutReturnPickUpDto { RequestID = 0 };

            var result = await controller.Update(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUpdateFails()
        {
            var mockService = new Mock<IRequestPullOutReturnPickUpService>();
            mockService.Setup(r => r.UpdateAsync(It.IsAny<UpdateRequestPullOutReturnPickUpDto>())).ReturnsAsync(false);

            var controller = CreateController(mockService: mockService);

            var dto = new UpdateRequestPullOutReturnPickUpDto { RequestID = 123 };

            var result = await controller.Update(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCancelledRemarks_ReturnsOk_WhenRemarksExist()
        {
            var mockRemarks = new Mock<IRemarksService>();
            mockRemarks.Setup(r => r.GetAllRemarks(It.IsAny<long>())).ReturnsAsync(new RemarksDto { Remarks = "a" });

            var controller = CreateController(mockRemarks: mockRemarks);

            var result = await controller.GetCancelledRemarks(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCancelledRemarks_ReturnsNotFound_WhenRemarksNull()
        {
            var mockRemarks = new Mock<IRemarksService>();
            mockRemarks.Setup(r => r.GetAllRemarks(It.IsAny<long>())).ReturnsAsync((RemarksDto?)null);

            var controller = CreateController(mockRemarks: mockRemarks);

            var result = await controller.GetCancelledRemarks(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsBadRequest_WhenIdInvalid()
        {
            var controller = CreateController();

            var result = await controller.CancelRequest(0, "JCA", "x");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsNotFound_WhenInsertRemarkReturnsFalse()
        {
            var mockRemarks = new Mock<IRemarksService>();
            mockRemarks.Setup(r => r.CancelPullOutReturnPickUpAsync(It.IsAny<long>(), "JCA", It.IsAny<string>())).ReturnsAsync(false);

            var controller = CreateController(mockRemarks: mockRemarks);

            var result = await controller.CancelRequest(1, "JCA", "x");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CancelRequest_ReturnsOk_WhenInsertRemarkSucceeds()
        {
            var mockRemarks = new Mock<IRemarksService>();
            mockRemarks.Setup(r => r.CancelPullOutReturnPickUpAsync(It.IsAny<long>(), "JCA", It.IsAny<string>())).ReturnsAsync(true);

            var controller = CreateController(mockRemarks: mockRemarks);

            var result = await controller.CancelRequest(1, "JCA", "x");

            var ok = Assert.IsType<OkObjectResult>(result);
            // anonymous object with message
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsBadRequest_WhenArgsMissing()
        {
            var controller = CreateController();

            var result = await controller.GetRequestImage(null!, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsNotFound_WhenImageNull()
        {
            var mockImage = new Mock<IImageService>();
            mockImage.Setup(m => m.GetRequestImageAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((byte[]?)null);

            var controller = CreateController(mockImage: mockImage);

            var result = await controller.GetRequestImage("1", "t");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetRequestImage_ReturnsFile_WhenImageExists()
        {
            var mockImage = new Mock<IImageService>();
            var bytes = new byte[] { 1, 2, 3 };
            mockImage.Setup(m => m.GetRequestImageAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(bytes);

            var controller = CreateController(mockImage: mockImage);

            var result = await controller.GetRequestImage("1", "t");

            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsValidationProblem_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Image", "required");

            var dto = new UploadImageRequestDto();

            var result = await controller.UploadImage(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.IsType<ValidationProblemDetails>(obj.Value);
        }

        [Fact]
        public async Task UploadImage_ReturnsBadRequest_WhenNoImage()
        {
            var controller = CreateController();

            var dto = new UploadImageRequestDto { Image = null, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsOk_WhenUploadSucceeds()
        {
            var mockImage = new Mock<IImageService>();

            var content = Encoding.UTF8.GetBytes("abc");
            var stream = new MemoryStream(content);
            IFormFile file = new FormFile(stream, 0, content.Length, "Image", "test.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

            mockImage.Setup(m => m.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("/path");

            var controller = CreateController(mockImage: mockImage);

            var dto = new UploadImageRequestDto { Image = file, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UploadImage_ReturnsServerError_WhenUploadFails()
        {
            var mockImage = new Mock<IImageService>();

            var content = Encoding.UTF8.GetBytes("abc");
            var stream = new MemoryStream(content);
            IFormFile file = new FormFile(stream, 0, content.Length, "Image", "test.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

            mockImage.Setup(m => m.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string?)null);

            var controller = CreateController(mockImage: mockImage);

            var dto = new UploadImageRequestDto { Image = file, RequestID = "1", Type = "Proof" };

            var result = await controller.UploadImage(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
