using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Categories.Commands.CreateCateogry;
using GloboTicket.TicketManagement.Application.Profiles;
using GloboTicket.TicketManagement.Application.UnitTests.Mocks;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;

namespace GloboTicket.TicketManagement.Application.UnitTests.Categories.Commands
{
    public class CreateCategoryTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IAsyncRepository<Category>> _mockCategoryRepository;

        public CreateCategoryTests()
        {
            _mockCategoryRepository = RepositoryMocks.GetCategoryRepository();
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configurationProvider.CreateMapper();
        }
        [Fact]
        public void AutoMapper_Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            config.AssertConfigurationIsValid();
        }
        [Fact]
        public async Task Handle_ValidCategory_AddedToCategoriesRepo()
        {
            var handler = new CreateCategoryCommandHandler(_mapper, _mockCategoryRepository.Object);

            await handler.Handle(new CreateCategoryCommand() { Name = "Test" }, CancellationToken.None);

            var allCategories = await _mockCategoryRepository.Object.ListAllAsync();
            allCategories.Count.ShouldBe(5);
        }

        [Fact]
        public async Task Handle_NullCategoryName_ShouldReturnValidationError()
        {
            // Arrange
            var handler = new CreateCategoryCommandHandler(_mapper, _mockCategoryRepository.Object);
            var command = new CreateCategoryCommand() { Name = null };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.ValidationErrors?.ShouldContain("Name is required."); // Assuming this is the error message from the validator
            var allCategories = await _mockCategoryRepository.Object.ListAllAsync();
            allCategories.Count.ShouldBe(4); // No new category should be added
        }

        [Fact]
        public async Task Handle_EmptyCategoryName_ShouldReturnValidationError()
        {
            // Arrange
            var handler = new CreateCategoryCommandHandler(_mapper, _mockCategoryRepository.Object);
            var command = new CreateCategoryCommand() { Name = string.Empty };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.ValidationErrors?.ShouldContain("Name is required."); // Assuming this is the error message from the validator
            var allCategories = await _mockCategoryRepository.Object.ListAllAsync();
            allCategories.Count.ShouldBe(4); // No new category should be added
        }

        [Fact]
        public async Task Handle_CategoryNameExceedsMaxLength_ShouldReturnValidationError()
        {
            // Arrange
            var handler = new CreateCategoryCommandHandler(_mapper, _mockCategoryRepository.Object);
            var command = new CreateCategoryCommand() { Name = new string('A', 51) }; // 51 characters long

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.ValidationErrors?.ShouldContain("Name must not exceed 10 characters.");
            var allCategories = await _mockCategoryRepository.Object.ListAllAsync();
            allCategories.Count.ShouldBe(4); // No new category should be added
        }

    }
}
