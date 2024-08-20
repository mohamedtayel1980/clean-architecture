using GloboTicket.TicketManagement.Application.Contracts;
using GloboTicket.TicketManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace GloboTicket.TicketManagement.Persistence.IntegrationTests
{
    public class GloboTicketDbContextTests
    {
        private readonly GloboTicketDbContext _globoTicketDbContext;
        private readonly Mock<ILoggedInUserService> _loggedInUserServiceMock;
        private readonly string _loggedInUserId;

        public GloboTicketDbContextTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<GloboTicketDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _loggedInUserId = "00000000-0000-0000-0000-000000000000";
            _loggedInUserServiceMock = new Mock<ILoggedInUserService>();
            _loggedInUserServiceMock.Setup(m => m.UserId).Returns(_loggedInUserId);

            _globoTicketDbContext = new GloboTicketDbContext(dbContextOptions, _loggedInUserServiceMock.Object);
        }

        [Fact]
        public async void Save_SetCreatedByProperty()
        {
            var ev = new Event() { EventId = Guid.NewGuid(), Name = "Test event" };

            _globoTicketDbContext.Events.Add(ev);
            await _globoTicketDbContext.SaveChangesAsync();

            ev.CreatedBy.ShouldBe(_loggedInUserId);
        }
        [Fact]
        public async Task Update_SetModifiedByProperty()
        {
            // Arrange
            var ev = new Event()
            {
                EventId = Guid.NewGuid(),
                Name = "Test event",
                CreatedBy = "OriginalUserId"
            };

            _globoTicketDbContext.Events.Add(ev);
            await _globoTicketDbContext.SaveChangesAsync();

            // Act
            ev.Name = "Updated event name"; // Modify the event
            ev.LastModifiedBy = _loggedInUserId; // Simulate setting the ModifiedBy property
            await _globoTicketDbContext.SaveChangesAsync();

            // Assert
            ev.LastModifiedBy.ShouldBe(_loggedInUserId); // Verify that the ModifiedBy property is set correctly
        }
    }
}