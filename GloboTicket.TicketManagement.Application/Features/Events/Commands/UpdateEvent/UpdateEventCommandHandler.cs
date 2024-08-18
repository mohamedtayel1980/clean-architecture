using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Domain.Entities;
using MediatR;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, UpdateEventCommandResponse>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public UpdateEventCommandHandler(IMapper mapper, IEventRepository eventRepository)
        {
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<UpdateEventCommandResponse> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var updateEventCommandResponse = new UpdateEventCommandResponse();
            // Retrieve the event to update
            var eventToUpdate = await _eventRepository.GetByIdAsync(request.EventId);
            if (eventToUpdate == null)
            {
                updateEventCommandResponse.Success = false;
                updateEventCommandResponse.ValidationErrors = new List<string> { "Event not found." };
                return updateEventCommandResponse;
            }
            // Validate the request
            var validator = new UpdateEventCommandValidator(_eventRepository);
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
            {
                updateEventCommandResponse.Success = false;
                updateEventCommandResponse.ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return updateEventCommandResponse;
            }

            

            // Map the request to the existing event
            eventToUpdate=_mapper.Map<Event>(request);
            // Update the event
            await _eventRepository.UpdateAsync(eventToUpdate);

            // Prepare the response DTO
            updateEventCommandResponse.EventDto = _mapper.Map<UpdateEventDto>(eventToUpdate);

            return updateEventCommandResponse;
        }
    }
}