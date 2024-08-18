using GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GloboTicket.TicketManagement.Application.Responses;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandResponse : BaseResponse
    {
        public UpdateEventCommandResponse() : base()
        {

        }

       
        public UpdateEventDto EventDto { get; set; }
    }
}
