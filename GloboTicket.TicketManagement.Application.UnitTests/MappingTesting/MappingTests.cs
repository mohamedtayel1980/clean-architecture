using AutoMapper;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent;
using GloboTicket.TicketManagement.Application.Profiles;
using GloboTicket.TicketManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.UnitTests.MappingTesting
{
    public class MappingTests
    {
        private readonly IMapper _mapper;

        public MappingTests()
        {
            var services = new ServiceCollection();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var serviceProvider = services.BuildServiceProvider();
            _mapper = serviceProvider.GetRequiredService<IMapper>();
        }

        [Fact]
        public void Should_Map_CreateEventCommand_To_Event()
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

            // Act
            var theEvent = _mapper.Map<Event>(request);

            // Assert
            Assert.NotNull(theEvent);
            Assert.Equal(request.Name, theEvent.Name);

            // Use breakpoint here to inspect theEvent
        }

        [Fact]
        public void Mapping_Should_Map_UpdateEventCommand_To_Event()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = new Mapper(configuration);

            var command = new UpdateEventCommand
            {
                EventId = Guid.NewGuid(),
                Name = "Updated Event",
                Price = 100,
                Artist = "Updated Artist",
                Date = DateTime.Now.AddMonths(1),
                Description = "Updated Description",
                CategoryId = Guid.NewGuid()
            };

            // Act
            var eventEntity = mapper.Map<Event>(command);

            // Assert
            eventEntity.ShouldNotBeNull();
            eventEntity.EventId.ShouldBe(command.EventId);
            eventEntity.Name.ShouldBe(command.Name);
            eventEntity.Price.ShouldBe(command.Price);
            eventEntity.Artist.ShouldBe(command.Artist);
            eventEntity.Date.ShouldBe(command.Date);
            eventEntity.Description.ShouldBe(command.Description);
            eventEntity.CategoryId.ShouldBe(command.CategoryId);
        }

        [Fact]
        public void Should_Map_UpdateEventCommand_To_Event_Using_Existing_Entity()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = configuration.CreateMapper();

            var command = new UpdateEventCommand
            {
                EventId = Guid.NewGuid(),
                Name = "Updated Event",
                Price = 100,
                Artist = "Updated Artist",
                Date = DateTime.Now.AddMonths(1),
                Description = "Updated Description",
                CategoryId = Guid.NewGuid()
            };

            var existingEvent = new Event
            {
                EventId = command.EventId,
                Name = "Old Event",
                Price = 50,
                Artist = "Old Artist",
                Date = DateTime.Now.AddMonths(-1),
                Description = "Old Description",
                CategoryId = Guid.NewGuid()
            };

            // Act
            mapper.Map(command, existingEvent);

            // Assert
            existingEvent.ShouldNotBeNull();
            existingEvent.EventId.ShouldBe(command.EventId); // This should remain unchanged
            existingEvent.Name.ShouldBe(command.Name);
            existingEvent.Price.ShouldBe(command.Price);
            existingEvent.Artist.ShouldBe(command.Artist);
            existingEvent.Date.ShouldBe(command.Date);
            existingEvent.Description.ShouldBe(command.Description);
            existingEvent.CategoryId.ShouldBe(command.CategoryId);
        }
    }
}
