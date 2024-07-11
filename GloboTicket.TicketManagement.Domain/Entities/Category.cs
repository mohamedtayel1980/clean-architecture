
namespace GloboTicket.TicketManagement.Domain.Entities
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Event>? Events { get; set; }
    }
}
