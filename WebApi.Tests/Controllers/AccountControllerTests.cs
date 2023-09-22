using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DataObjects;
using Application.Interfaces;
using Application.Models.Users;
using WebApi.Controllers;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data;
using AutoMapper;
using Moq;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IServiceWrapper> _serviceWrapperMock;
        private readonly AccountController _accountController;

        public AccountControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();
            _serviceWrapperMock = new Mock<IServiceWrapper>();
            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);
            _accountController = new AccountController(_serviceWrapperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var baseResponse = new BaseResponse<IEnumerable<UserDto>>(
                Result: new List<UserDto>(),
                Success: true,
                Messages: new List<string> { "Пользователи успешно получены" });

            _userServiceMock.Setup(s => s.GetAll()).Returns(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Get() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<IEnumerable<UserDto>>>(result.Value);
            Assert.True(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Empty(responseObject.Result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenServiceReturnsFailure()
        {
            // Arrange

            var baseResponse = new BaseResponse<IEnumerable<UserDto>>(
                Result: null,
                Success: false,
                Messages: new List<string> { "Данные не были получены" });

            _userServiceMock.Setup(s => s.GetAll()).Returns(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Get() as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<IEnumerable<UserDto>>>(result.Value);
            Assert.False(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Null(responseObject.Result);
        }

        [Fact]
        public async Task GetByOid_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var oid = "some_oid";

            var baseResponse = new BaseResponse<UserDto>(
                Result: new UserDto
                {
                    PhoneNumber = "7072022022"
                },
                Success: true,
                Messages: new List<string> { "Пользователь успешно найден" });

            _userServiceMock.Setup(s => s.GetByOid(oid)).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Get(oid) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<UserDto>>(result.Value);
            Assert.True(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.NotNull(responseObject.Result);
        }

        [Fact]
        public async Task GetByOid_ReturnsNotFound_WhenServiceReturnsFailure()
        {
            // Arrange
            var oid = "some_oid";

            var baseResponse = new BaseResponse<UserDto>(
                Result: null,
                Success: false,
                Messages: new List<string> { "Пользователь не найден" });

            _userServiceMock.Setup(s => s.GetByOid(oid)).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Get(oid) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<UserDto>>(result.Value);
            Assert.False(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Null(responseObject.Result);
        }

        [Fact]
        public async Task Post_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new GenericIdentity("username");
            mockHttpContext.SetupGet(context => context.User.Identity).Returns(mockIdentity);

            var controllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            var createUserDto = new CreateUserDto
            {
                PhoneNumber = "7072022022"
            };
            var mappedUserDto = new UserDto
            {
                PhoneNumber = "7072022022"
            };

            var baseResponse = new BaseResponse<string>(
                Result: "user_id",
                Success: true,
                Messages: new List<string> { "Пользователь успешно создан" });

            _mapperMock
                .Setup(m => m.Map<UserDto>(createUserDto))
                .Returns(mappedUserDto);

            _userServiceMock.Setup(s => s.CreateAsync(mappedUserDto, It.IsAny<string>()))
                .ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            _accountController.ControllerContext = controllerContext;

            // Act
            var result = await _accountController.Post(createUserDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<string>>(result.Value);
            Assert.True(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Equal("user_id", responseObject.Result);
        }

        /*[Fact]
        public async Task Post_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new GenericIdentity("username");
            mockHttpContext.SetupGet(context => context.User.Identity).Returns(mockIdentity);

            var controllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            var createUserDto = new CreateUserDto
            {
                PhoneNumber = "7072022022"
            };
            var mappedUserDto = new UserDto
            {
                PhoneNumber = "7072022022"
            };

            var baseResponse = new BaseResponse<string>(
                Result: "",
                Success: false,
                Messages: new List<string> { "Ошибка при создании пользователя" });

            _mapperMock.Setup(m => m.Map<UserDto>(createUserDto)).Returns(mappedUserDto);

            _userServiceMock.Setup(s => s.CreateAsync(mappedUserDto, It.IsAny<string>()))
                .ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            _accountController.ControllerContext = controllerContext;

            // Act
            var result = await _accountController.Post(createUserDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<string>>(result.Value);
            Assert.False(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Equal("", responseObject.Result);
        }*/

        /*[Fact]
        public async Task Put_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var updateUserDto = new UpdateUserDto
            {
                PhoneNumber = "7072022022"
            };

            var baseResponse = new BaseResponse<UserDto>(
                Result: new UserDto
                {
                    PhoneNumber = "7072022022"
                },
                Success: true,
                Messages: new List<string> { "Пользователь успешно изменен" });

            _userServiceMock.Setup(s => s.UpdateUser(updateUserDto, It.IsAny<string>())).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Put(updateUserDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<UserDto>>(result.Value);
            Assert.True(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.NotNull(responseObject.Result);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var updateUserDto = new UpdateUserDto
            {
                PhoneNumber = "7072022022"
            };
            var baseResponse = new BaseResponse<UserDto>(
                Result: null,
                Success: false,
                Messages: new List<string> { "Ошибка при изменении пользователя" });

            _userServiceMock.Setup(s => s.UpdateUser(updateUserDto,"Admin")).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Put(updateUserDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<UserDto>>(result.Value);
            Assert.False(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.Null(responseObject.Result);
        }*/

        [Fact]
        public async Task Delete_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            var oid = "some_oid";

            var baseResponse = new BaseResponse<bool>(
                Result: true,
                Success: true,
                Messages: new List<string> { "Пользователь успешно удален" });

            _userServiceMock.Setup(s => s.Delete(oid)).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Delete(oid) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<bool>>(result.Value);
            Assert.True(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.True(responseObject.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenServiceReturnsFailure()
        {
            // Arrange
            var oid = "some_oid";

            var baseResponse = new BaseResponse<bool>(
                Result: false,
                Success: false,
                Messages: new List<string> { "Пользователя не существует" });

            _userServiceMock.Setup(s => s.Delete(oid)).ReturnsAsync(baseResponse);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Delete(oid) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);

            var responseObject = Assert.IsType<BaseResponse<bool>>(result.Value);
            Assert.False(responseObject.Success);
            Assert.NotEmpty(responseObject.Messages);
            Assert.False(responseObject.Result);
        }

        [Fact]
        public async Task GetLoadResult_ReturnsOkResult()
        {
            // Arrange
            var loadOptions = new DataSourceLoadOptionsBase();

            var loadResult = new LoadResult();
            _userServiceMock.Setup(s => s.GetLoadResult(loadOptions)).ReturnsAsync(loadResult);

            _serviceWrapperMock.Setup(s => s.UserService).Returns(_userServiceMock.Object);

            // Act
            var result = await _accountController.Get(loadOptions) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = Assert.IsType<LoadResult>(result.Value);
            Assert.Same(loadResult, responseObject);
        }
    }
}