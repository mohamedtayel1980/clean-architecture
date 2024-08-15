using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Infrastructure;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Categories.Commands.CreateCateogry;
using GloboTicket.TicketManagement.Application.Models.Mail;
using GloboTicket.TicketManagement.Domain.Entities;
using MediatR;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, CreateEventCommandResponse>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        public CreateEventCommandHandler(IMapper mapper, IEventRepository eventRepository, IEmailService emailService)
        {
            _mapper = mapper;
            _eventRepository = eventRepository;
            _emailService = emailService;
        }

        public async Task<CreateEventCommandResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {

            var theEvent = _mapper.Map<Event>(request);            
            var createEventCommandResponse = new CreateEventCommandResponse();
            var validator = new CreateEventCommandValidator(_eventRepository);
            var validationResult = await validator.ValidateAsync(request);
            if (validationResult.Errors.Count > 0)
            {
                createEventCommandResponse.Success = false;
                createEventCommandResponse.ValidationErrors = new List<string>();
                foreach (var error in validationResult.Errors)
                {
                    createEventCommandResponse.ValidationErrors.Add(error.ErrorMessage);
                }
            }
            if (createEventCommandResponse.Success)
            {
                theEvent = await _eventRepository.AddAsync(theEvent);
                createEventCommandResponse.EventDto = _mapper.Map<CreateEventDto>(theEvent);
                var email = new Email() { To = "moh.moh701@gmail.com", Body = $"A new event was created: {request}", Subject = "A new event was created" };

                //Sending email notification to admin address
                try
                {
                    await _emailService.SendEmail(email);
                }
                catch (Exception ex)
                {
                    //this shouldn't stop the API from doing else so this can be logged
                }
            }




            return createEventCommandResponse;

        }
    }
}
