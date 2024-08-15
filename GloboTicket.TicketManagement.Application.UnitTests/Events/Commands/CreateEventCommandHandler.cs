using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Infrastructure;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GloboTicket.TicketManagement.Application.Models.Mail;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GloboTicket.TicketManagement.Application.Exceptions;
using GloboTicket.TicketManagement.Application.Profiles;

namespace GloboTicket.TicketManagement.Application.UnitTests.Events.Commands
{
    public class CreateEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly CreateEventCommandHandler _handler;

        public CreateEventCommandHandlerTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _handler = new CreateEventCommandHandler(_mapperMock.Object, _eventRepositoryMock.Object, _emailServiceMock.Object);
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
        public void Mapping_Should_Map_CreateEventCommand_To_Event()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = new Mapper(configuration);
            var command = new CreateEventCommand
            {
                Name = "Test Event",
                Price = 50,
                Artist = "Test Artist",
                Date = DateTime.Now,
                Description = "Test Description",
                CategoryId = Guid.NewGuid()
            };

            // Act
            var eventEntity = mapper.Map<Event>(command);

            // Assert
            eventEntity.Name.ShouldBe(command.Name);
        }
        [Fact]
        public void Should_Map_CreateEventCommand_To_Event()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = configuration.CreateMapper();

            var command = new CreateEventCommand
            {
                Name = "Test Event",
                Price = 50,
                Artist = "Test Artist",
                Date = DateTime.Now,
                Description = "Test Description",
                CategoryId = Guid.NewGuid()
            };

            // Act
            var eventEntity = mapper.Map<Event>(command);

            // Assert
            eventEntity.ShouldNotBeNull();
            eventEntity.Name.ShouldBe(command.Name);
            eventEntity.Price.ShouldBe(command.Price);
            eventEntity.Artist.ShouldBe(command.Artist);
            eventEntity.Date.ShouldBe(command.Date);
            eventEntity.Description.ShouldBe(command.Description);
            eventEntity.CategoryId.ShouldBe(command.CategoryId);
        }
        [Fact]
        public async Task Handle_Should_Throw_ValidationException_If_Validation_Fails()
        {
            var command = new CreateEventCommand
            {
                Name = "Test Event",
                Date = DateTime.Now.AddDays(-1) // Invalid date
            };

            await Should.ThrowAsync<ValidationException>(async () =>
     await _handler.Handle(command, CancellationToken.None));

        }

        [Fact]
        public async Task Handle_Should_Call_AddAsync_If_Validation_Succeeds()
        {
            string eventName = RundomEventName();
            var request = new CreateEventCommand
            {
                Name = eventName,
                
                Price = 50,
                Artist = "Test Artist",
                Date = DateTime.Now.AddDays(20),
                Description = "Join John for his farewell tour across 15 continents. John really needs no introduction since he has already mesmerized the world with his banjo.",
                ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/GloboTicket/banjo.jpg",
               
                CategoryId = Guid.NewGuid()
            };

            var eventEntity = new Event
            {
                EventId = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Artist = request.Artist,
                Date = request.Date,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Category = new Category() // Mock category if needed
            };

            _mapperMock.Setup(m => m.Map<Event>(It.IsAny<CreateEventCommand>())).Returns(eventEntity);

            _eventRepositoryMock
        .Setup(repo => repo.AddAsync(It.IsAny<Event>()))
        .ReturnsAsync((Event e) => e); // Ensure the mock returns the event

            _mapperMock.Setup(m => m.Map<CreateEventDto>(It.IsAny<Event>()))
            .Returns((Event e) => new CreateEventDto
            {
                EventId = e.EventId,
                Name = e.Name
            });

            var result = await _handler.Handle(request, CancellationToken.None);
         
            _eventRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Event>()), Times.Once);
            result.EventDto.EventId.ShouldBe(eventEntity.EventId);
        }
        
        [Fact]
        public async Task Handle_Should_Send_Email_Notification()
        {
            string eventName = RundomEventName();
            var request = new CreateEventCommand
            {

                Name = eventName,
                Price = 135,
                Artist = "Nick Sailor",
                Date = DateTime.Now.AddMonths(8),
                Description = "The critics are over the moon and so will you after you've watched this sing and dance extravaganza written by Nick Sailor, the man from 'My dad and sister'.",
                ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/GloboTicket/musical.jpg",
                CategoryId = Guid.Parse("{ADC42C09-08C1-4D2C-9F96-2D15BB1AF299}")
            };

            var eventEntity = new Event
            {
                EventId = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Artist = request.Artist,
                Date = request.Date,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Category = new Category() // Mock category if needed
            };
            _mapperMock.Setup(m => m.Map<Event>(It.IsAny<CreateEventCommand>())).Returns(eventEntity);


            await _handler.Handle(request, CancellationToken.None);

            _emailServiceMock.Verify(emailService => emailService.SendEmail(It.IsAny<Email>()), Times.Once);
        }

        private static string RundomEventName()
        {
            var random = new Random();
            string prefix = "Valid Event";
            int maxLength = 50 - prefix.Length; // Calculate the maximum length for the random part
            string randomPart = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", maxLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            string eventName = $"{prefix}{randomPart}";
            return eventName;
        }

        [Fact]
        public async Task Handle_Should_Not_Throw_Exception_If_Email_Fails()
        {
            var command = new CreateEventCommand
            {
                Name = "Valid Event",
                Date = DateTime.Now.AddDays(1),
                Price = 100
            };

            var eventEntity = new Event { EventId = Guid.NewGuid() };

            _mapperMock.Setup(m => m.Map<Event>(It.IsAny<CreateEventCommand>())).Returns(eventEntity);
            _eventRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).ReturnsAsync(eventEntity);
            _eventRepositoryMock.Setup(repo => repo.IsEventNameAndDateUnique(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(true);
            _emailServiceMock.Setup(emailService => emailService.SendEmail(It.IsAny<Email>())).ThrowsAsync(new Exception("Email sending failed"));

            var result = await _handler.Handle(command, CancellationToken.None);

            _emailServiceMock.Verify(emailService => emailService.SendEmail(It.IsAny<Email>()), Times.Once);
            result.EventDto.EventId.ShouldBe(eventEntity.EventId); // Ensure the event is still returned even if email fails
        }

        [Fact]
        public async Task Handle_Should_Map_CreateEventCommand_To_Event()
        {
            // Arrange
            var request = new CreateEventCommand
            {
                Name = "Test Event",
                Price = 50,
                Artist = "Test Artist",
                Date = DateTime.Now,
                Description = "Test Description",
                CategoryId = Guid.NewGuid()
            };

            var eventEntity = new Event
            {
                EventId = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Artist = request.Artist,
                Date = request.Date,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Category = new Category() // Mock category if needed
            };

            _mapperMock.Setup(m => m.Map<Event>(It.IsAny<CreateEventCommand>())).Returns(eventEntity);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _mapperMock.Verify(m => m.Map<Event>(It.IsAny<CreateEventCommand>()), Times.Once);
        }
    }

}
