using GloboTicket.TicketManagement.Application.Features.Categories.Commands.CreateCateogry;
using GloboTicket.TicketManagement.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandResponse : BaseResponse
    {
        public CreateEventCommandResponse() : base()
        {

        }

        public CreateEventDto EventDto { get; set; } = default!;
    }
}
