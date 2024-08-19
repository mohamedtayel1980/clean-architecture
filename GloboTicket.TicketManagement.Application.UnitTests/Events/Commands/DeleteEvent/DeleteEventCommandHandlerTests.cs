using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Exceptions;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.DeleteEvent;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.UnitTests.Events.Commands.DeleteEvent
{
    public class DeleteEventCommandHandlerTests
    {
        private readonly Mock<IAsyncRepository<Event>> _eventRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeleteEventCommandHandler _handler;

        public DeleteEventCommandHandlerTests()
        {
            _eventRepositoryMock = new Mock<IAsyncRepository<Event>>();
            _mapperMock = new Mock<IMapper>();
            _handler = new DeleteEventCommandHandler(_mapperMock.Object, _eventRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_EventFound_DeletesEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event { EventId = eventId };
            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(@event);
            _eventRepositoryMock.Setup(repo => repo.DeleteAsync(@event)).Returns(Task.CompletedTask);

            var request = new DeleteEventCommand { EventId = eventId };

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _eventRepositoryMock.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _eventRepositoryMock.Verify(repo => repo.DeleteAsync(@event), Times.Once);
            _eventRepositoryMock.Invocations.Count.ShouldBe(2); // Ensures exactly 2 methods were invoked
        }

        [Fact]
        public async Task Handle_EventNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

            var request = new DeleteEventCommand { EventId = eventId };

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(request, CancellationToken.None));

            exception.ShouldNotBeNull();
            exception.Message.ShouldContain(nameof(Event));
            exception.Message.ShouldContain(eventId.ToString());

            _eventRepositoryMock.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _eventRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Event>()), Times.Never);
        }
    }
}
