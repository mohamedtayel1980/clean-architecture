﻿using GloboTicket.TicketManagement.Application.Features.Events.Queries.GetEventsList;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.Features.Events.Queries
{
    public class GetEventsListQuery:IRequest<List<EventListVm>>
    {
    }
}
