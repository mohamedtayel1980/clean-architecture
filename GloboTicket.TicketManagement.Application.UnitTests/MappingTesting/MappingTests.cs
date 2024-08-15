using AutoMapper;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GloboTicket.TicketManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
