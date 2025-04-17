using Moq;
using AutoMapper;
using FluentAssertions;
using CleaningMyName.Domain.Entities;
using CleaningMyName.UnitTests.Common;
using CleaningMyName.Domain.ValueObjects;
using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Application.Common.Mappings;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Application.Users.Queries.GetUserById;

namespace CleaningMyName.UnitTests.Application.Users.Queries;

public class GetUserByIdQueryTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

        // Configure AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithExistingUserId_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(
            "John", 
            "Doe", 
            Email.Create("john.doe@example.com"), 
            "hashedPassword");
        
        // Use reflection to set the Id property, which is privately set
        typeof(User).GetProperty("Id").SetValue(user, userId);

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);

        var query = new GetUserByIdQuery(userId);
        var handler = new GetUserByIdQueryHandler(_mockUnitOfWork.Object, _mapper);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email.Should().Be(user.Email.Value);
        result.FullName.Should().Be(user.FullName);
    }

    [Fact]
    public async Task Handle_WithNonExistingUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync((User)null);

        var query = new GetUserByIdQuery(userId);
        var handler = new GetUserByIdQueryHandler(_mockUnitOfWork.Object, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(query, default));
    }
}
