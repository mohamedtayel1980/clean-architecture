namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
