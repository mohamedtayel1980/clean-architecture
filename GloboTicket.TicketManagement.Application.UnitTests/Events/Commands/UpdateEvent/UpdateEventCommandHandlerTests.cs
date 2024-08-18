using AutoMapper;
using FluentValidation.Results;
using FluentValidation;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;
using GloboTicket.TicketManagement.Application.UnitTests.Mocks;
using GloboTicket.TicketManagement.Application.Profiles;
using FluentAssertions;

namespace GloboTicket.TicketManagement.Application.UnitTests.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateEventCommandHandler _handler;

        public UpdateEventCommandHandlerTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateEventCommandHandler(_mapperMock.Object, _eventRepositoryMock.Object);
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
        public async Task Handle_Should_Return_ValidationErrors_When_Validation_Fails()
        {
            // Arrange
            var existingEventId = Guid.Parse("{EE272F8B-6096-4CB6-8625-BB4BB2D89E8B}");
            var eventToUpdate = new Event
            {
                EventId = existingEventId,
                Name = "Old Event Name",
                Price = 50,
                Artist = "Artist Name",
                Date = DateTime.Now.AddMonths(1),
                Description = "Old Event Description",
                ImageUrl = "https://example.com/image.jpg",
                CategoryId = Guid.NewGuid()
            };

            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(existingEventId))
                                .ReturnsAsync(eventToUpdate);

            var request = new UpdateEventCommand { EventId = existingEventId, Name = "" };
            var validationResult = new ValidationResult(new List<ValidationFailure>
    {
        new ValidationFailure("Name", "Event name is required.")
    });

            var validatorMock = new Mock<IValidator<UpdateEventCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            var handler = new UpdateEventCommandHandler(_mapperMock.Object, _eventRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.ValidationErrors.ShouldContain("Name is required.");
        }


        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Event_Does_Not_Exist()
        {
            // Arrange
            var request = new UpdateEventCommand { EventId = Guid.NewGuid() };

            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync((Event)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.ShouldBeFalse();
            result.ValidationErrors.ShouldContain("Event not found.");
        }

        
        [Fact]
        public async Task Handle_Should_Update_Event_When_Valid()
        {
            var existingEventId = Guid.Parse("{EE272F8B-6096-4CB6-8625-BB4BB2D89E8B}");
            var existingCategoryId = Guid.NewGuid();
           // var request = new UpdateEventCommand { EventId = existingEventId, Name = "Updated Event" };
            var eventToUpdate = new Event
            {
                EventId = existingEventId,
                Name = "Old Event",
                Price = 50,
                Artist = "Old Artist",
                Date = DateTime.Now.AddMonths(-1),
                Description = "Old Description",
                ImageUrl = "https://example.com/old-image.jpg",
                CategoryId = existingCategoryId
            };

            var request = new UpdateEventCommand
            {
                EventId = existingEventId,
                Name = "Updated Event",
                Price = 100,
                Artist = "Updated Artist",
                Date = DateTime.Now.AddMonths(2),
                Description = "Updated Description",
                ImageUrl = "https://example.com/updated-image.jpg",
                CategoryId = existingCategoryId
            };
            var updatedEventDto = new UpdateEventDto
            {
                EventId = existingEventId,
                Name = "Updated Event",
                Price = 100,
                Artist = "Updated Artist",
                Date = DateTime.Now.AddMonths(2),
                Description = "Updated Description",
                ImageUrl = "https://example.com/updated-image.jpg",
                CategoryId = existingCategoryId
            };

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = configuration.CreateMapper();

            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(existingEventId))
                                .ReturnsAsync(eventToUpdate);

            var handler = new UpdateEventCommandHandler(mapper, _eventRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.ShouldBeTrue();
            //compare DateTime properties but allow for some tolerance
            result.EventDto.Should().BeEquivalentTo(updatedEventDto, options =>
    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
           .When(info => info.Path.EndsWith("Date")));
            // Check if the DTO matches the expected updated DTO
            _eventRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Event>()), Times.Once);
        }


    }
}
