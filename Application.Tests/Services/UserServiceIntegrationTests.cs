using System.Linq.Expressions;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Application.Models.Users;
using Application.Services;
using Application.Validation;
using AutoMapper;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services;

public class UserServiceIntegrationTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IUserService _userService;

    public UserServiceIntegrationTests()
    {
        UserValidator userValidatorMock = new();
        UpdateUserValidator updateUserValidator = new();
        _mapperMock = new Mock<IMapper>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _userService = new UserService(_repositoryWrapperMock.Object, _mapperMock.Object, userValidatorMock, updateUserValidator);
    }

    [Fact]
    public void GetAll_ReturnsUsers_Successfully()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User1",
                Surname = "Surname1",
                Login = "user1@example.com",
                PasswordHash = "QnWCCW4LnwMO2ZTuUFJx+ZDN6vuNLTrbEq7+4eswbco=",
                Salt = Convert.FromBase64String("tvNjITz/GQLm7OyVWZNYQg=="),
                RefreshTokens = new List<RefreshToken>(),
                ExecutiveCompanyId = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = "Admin",
                LastModified = DateTime.Now,
                LastModifiedBy = "Admin",
                IsDelete = false,
                PhoneNumber = "7072020202"
            },
        };

        var userDtoList = users.Select(u => new UserDto { Id = u.Id, Name = u.Name, Surname = u.Surname, PhoneNumber = u.Name}).ToList();

        _repositoryWrapperMock.Setup(r => r.UserRepository.GetAll()).Returns(users.AsQueryable());
        _mapperMock.Setup(m => m.Map<List<UserDto>>(It.IsAny<IQueryable<User>>())).Returns(userDtoList);

        // Act
        var response = _userService.GetAll();

        // Assert
        Assert.True(response.Success);
        Assert.NotEmpty(response.Result);
        Assert.Equal(userDtoList, response.Result);
        Assert.NotEmpty(response.Messages);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var emptyUsersList = new List<User>();
        var userDtoList = new List<UserDto>();

        _repositoryWrapperMock.Setup(r => r.UserRepository.GetAll()).Returns(emptyUsersList.AsQueryable());
        _mapperMock.Setup(m => m.Map<List<UserDto>>(It.IsAny<IQueryable<User>>())).Returns(userDtoList);

        // Act
        var response = _userService.GetAll();

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.Result);
        Assert.Equal(userDtoList, response.Result);
        Assert.NotEmpty(response.Messages);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var newUserDto = new UserDto
        {
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            Password = "Qwerty@123",
            Roles = new List<string>()
            {
                "Analyst"
            },
            ExecutiveCompanyId = new Guid(),
            PhoneNumber = "7072020202"
        };

        var newUser = new User
        {
            Id = new Guid(),
            Login = newUserDto.Login,
            Name = newUserDto.Name,
            Surname = newUserDto.Surname,
            PasswordHash = "QnWCCW4LnwMO2ZTuUFJx+ZDN6vuNLTrbEq7+4eswbco=",
            Salt = Convert.FromBase64String("tvNjITz/GQLm7OyVWZNYQg=="),
            ExecutiveCompanyId = newUserDto.ExecutiveCompanyId,
            RefreshTokens = new List<RefreshToken>(),
            CreatedBy = "Admin",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            LastModifiedBy = "Admin",
            IsDelete = false,
            PhoneNumber = newUserDto.PhoneNumber
        };

        var role1 = new Role() { Id = new Guid(), RoleName = "Analyst" };
        var role2 = new Role() { Id = new Guid(), RoleName = "Manager" };
        var roles = new List<Role> { role1, role2 };
        var userRoles = new List<UserRole>
        {
            new() { UserId = newUser.Id, Role = role1, RoleId = role1.Id, User = newUser },
            new() { UserId = newUser.Id, Role = role2, RoleId = role2.Id, User = newUser },
        };

        _repositoryWrapperMock.Setup(r => r.RoleRepository.GetAll()).Returns(roles.AsQueryable);
        _repositoryWrapperMock.Setup(r => r.UserRoleRepository.GetAll()).Returns(userRoles.AsQueryable);

        _mapperMock.Setup(m => m.Map<User>(newUserDto))
            .Returns(newUser);

        _repositoryWrapperMock.Setup(r => r.UserRepository
                .CreateAsync(newUser))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _userService.CreateAsync(newUserDto, "Admin");

        // Assert
        _repositoryWrapperMock.Verify(r => r.UserRepository.CreateAsync(It.IsAny<User>()), Times.Once);
        _repositoryWrapperMock.Verify(r => r.Save(), Times.Once);

        Assert.True(response.Success);
        Assert.NotEmpty(response.Result);
        Assert.NotEmpty(response.Messages);
    }

    [Fact]
    public async Task GetByOid_UserFound_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            PasswordHash = "QnWCCW4LnwMO2ZTuUFJx+ZDN6vuNLTrbEq7+4eswbco=",
            Salt = Convert.FromBase64String("tvNjITz/GQLm7OyVWZNYQg=="),
            ExecutiveCompanyId = new Guid(),
            RefreshTokens = new List<RefreshToken>(),
            CreatedBy = "Admin",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            LastModifiedBy = "Admin",
            IsDelete = false,
            PhoneNumber = "7072020202"
        };
        var role = new Role() { Id = new Guid(), RoleName = "Analyst" };
        var userRoles = new List<UserRole>
        {
            new() { UserId = user.Id, Role = role, RoleId = role.Id, User = user }
        };

        var userDto = new UserDto
        {
            Id = userId,
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            Password = "Qwerty@123",
            Roles = userRoles.Where(userRole => userRole.UserId == user.Id)
                .Select(userRole => userRole.Role.RoleName)
                .ToList(),
            ExecutiveCompanyId = new Guid(),
            PhoneNumber = "7072020202"
        };

        _mapperMock.Setup(r => r.Map<UserDto>(user)).Returns(userDto);

        _repositoryWrapperMock.Setup(r => r.UserRoleRepository.GetAll()).Returns(userRoles.AsQueryable);

        _repositoryWrapperMock.Setup(r => r.UserRepository
                .GetByCondition(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        _repositoryWrapperMock.Setup(r => r.UserRoleRepository.GetAll())
            .Returns(userRoles.AsQueryable());

        // Act
        var response = await _userService.GetByOid(userId.ToString());

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
        Assert.Equal(userId, response.Result.Id);
        Assert.NotEmpty(response.Result.Roles);
        Assert.Contains("Analyst", response.Result.Roles);
        Assert.NotEmpty(response.Messages);
    }

    /*[Fact]
    public async Task UpdateUser_ExistingUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var executiveCompanyId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Id = userId.ToString(),
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            Roles = new List<string>()
            {
                "Analyst"
            },
            ExecutiveCompanyId = executiveCompanyId,
            PhoneNumber = "7072020202"
        };

        var updatedUser = new User
        {
            Id = new Guid(updateUserDto.Id),
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            PasswordHash = "QnWCCW4LnwMO2ZTuUFJx+ZDN6vuNLTrbEq7+4eswbco=",
            Salt = Convert.FromBase64String("tvNjITz/GQLm7OyVWZNYQg=="),
            CreatedBy = "Admin",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            LastModifiedBy = "Admin",
            IsDelete = false,
            ExecutiveCompanyId = updateUserDto.ExecutiveCompanyId,
            RefreshTokens = new List<RefreshToken>(),
            PhoneNumber = "7072020202"
        };
        
        var role1 = new Role() { Id = new Guid(), RoleName = "Analyst" };
        var role2 = new Role() { Id = new Guid(), RoleName = "Manager" };
        var roles = new List<Role> { role1, role2 };
        var userRoles = new List<UserRole>
        {
            new() { UserId = updatedUser.Id, Role = role1, RoleId = role1.Id, User = updatedUser },
            new() { UserId = updatedUser.Id, Role = role2, RoleId = role2.Id, User = updatedUser },
        };

        _repositoryWrapperMock.Setup(r => r.RoleRepository.GetAll()).Returns(roles.AsQueryable);
        _repositoryWrapperMock.Setup(r => r.UserRoleRepository.GetAll()).Returns(userRoles.AsQueryable);
        
        var existingUserDto = new UserDto
        {
            Id = updatedUser.Id,
            Login = "test@mail.kz",
            Name = "Testname",
            Surname = "Testsurname",
            Roles = userRoles.Where(userRole => userRole.UserId == updatedUser.Id)
                .Select(userRole => userRole.Role.RoleName)
                .ToList(),
            CreatedBy = "Admin",
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            LastModifiedBy = "Admin",
            Password = "Qwerty@123",
            ExecutiveCompanyId = updatedUser.ExecutiveCompanyId,
            IsDelete = false,
            PhoneNumber = "7072020202"
        };

        _repositoryWrapperMock.Setup(r => r.UserRoleRepository.GetAll()).Returns(userRoles.AsQueryable);

        _mapperMock.Setup(r => r.Map<UserDto>(updatedUser)).Returns(existingUserDto);


        _repositoryWrapperMock.Setup(r => r.UserRepository
                .GetByCondition(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(updatedUser);

        _repositoryWrapperMock.Setup(r => r.UserRepository
            .Update(updatedUser));


        // Act
        var response = await _userService.UpdateUser(updateUserDto, "Admin");

        // Assert
        _repositoryWrapperMock.Verify(r => r.UserRepository.Update(updatedUser), Times.Once);
        _repositoryWrapperMock.Verify(r => r.Save(), Times.Once);

        Assert.True(response.Success);
        Assert.NotNull(response.Result);
        Assert.NotEmpty(response.Messages);
    }*/
}