using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Events.Queries.GetEventDetail;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.UnitTests.Events.Query
{
    public class GetEventDetailQueryHandlerTests
    {
        private readonly Mock<IAsyncRepository<Event>> _eventRepositoryMock;
        private readonly Mock<IAsyncRepository<Category>> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetEventDetailQueryHandler _handler;

        public GetEventDetailQueryHandlerTests()
        {
            _eventRepositoryMock = new Mock<IAsyncRepository<Event>>();
            _categoryRepositoryMock = new Mock<IAsyncRepository<Category>>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetEventDetailQueryHandler(
                _mapperMock.Object,
                _eventRepositoryMock.Object,
                _categoryRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_GivenValidEventId_ShouldReturnEventDetailVm()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var @event = new Event
            {
                EventId = eventId,
                Name = "Sample Event",
                CategoryId = categoryId
            };

            var category = new Category
            {
                CategoryId = categoryId,
                Name = "Sample Category"
            };

            var eventDetailVm = new EventDetailVm
            {
                EventId = eventId,
                Name = "Sample Event",
                CategoryId = categoryId,
                Category = new CategoryDto { CategoryId = categoryId, Name = "Sample Category" }
            };

            _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(@event);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mapperMock.Setup(m => m.Map<EventDetailVm>(@event))
                .Returns(eventDetailVm);

            _mapperMock.Setup(m => m.Map<CategoryDto>(category))
                .Returns(eventDetailVm.Category);

            var query = new GetEventDetailQuery { Id = eventId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.EventId.ShouldBe(eventId);
            result.Name.ShouldBe("Sample Event");
            result.CategoryId.ShouldBe(categoryId);
            result.Category.ShouldNotBeNull();
            result.Category.Name.ShouldBe("Sample Category");

            _eventRepositoryMock.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _categoryRepositoryMock.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mapperMock.Verify(m => m.Map<EventDetailVm>(@event), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryDto>(category), Times.Once);
        }
    }
}
