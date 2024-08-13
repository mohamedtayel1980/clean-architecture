using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GloboTicket.TicketManagement.Application.UnitTests.Mocks;
using GloboTicket.TicketManagement.Domain.Entities;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GloboTicket.TicketManagement.Application.UnitTests.Events.Commands
{
    public class CreateEventCommandValidatorTests
    {
        private readonly CreateEventCommandValidator _validator;
        private readonly Mock<IEventRepository> _eventRepositoryMock;

        public CreateEventCommandValidatorTests()
        {
            // Use the mock repository from the RepositoryMocks class
            _eventRepositoryMock = RepositoryMocks.GetEventRepository();
            _validator = new CreateEventCommandValidator(_eventRepositoryMock.Object);
        }


        [Fact]
        public async Task Should_Have_Error_When_Name_Is_Empty()
        {
            var command = new CreateEventCommand { Name = "" };

            var result = await _validator.ValidateAsync(command);

            result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required.");

        }

        [Fact]
        public async Task Should_Have_Error_When_Date_Is_In_Past()
        {
            var command = new CreateEventCommand { Date = DateTime.Now.AddDays(-1) };

            var result = await _validator.ValidateAsync(command);

            result.Errors.ShouldContain(e => e.PropertyName == "Date");
        }

        [Fact]
        public async Task Should_Have_Error_When_Price_Is_Zero_Or_Negative()
        {
            var command = new CreateEventCommand { Price = 0 };

            var result = await _validator.ValidateAsync(command);

            result.Errors.ShouldContain(e => e.PropertyName == "Price");
        }

        [Fact]
        public async Task Should_Have_Error_When_Name_And_Date_Are_Not_Unique()
        {
            // Arrange
            var command = new CreateEventCommand
            {
                Name = "John Egbert Live",
                Date = DateTime.Now.AddMonths(6)
            };
            
            var result = await _validator.ValidateAsync(command);

            
            result.Errors.ShouldContain(e => e.ErrorMessage == "An event with the same name and date already exists.");
            
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Validation_Is_Successful()
        {
            _eventRepositoryMock.Setup(repo => repo.IsEventNameAndDateUnique(It.IsAny<string>(), It.IsAny<DateTime>()))
                                .ReturnsAsync(false);

            var command = new CreateEventCommand
            {
                Name = "Valid Eventeeeeee",
                Date = DateTime.Now.AddDays(91),
                Price = 10098
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.ShouldBeTrue();
        }
    }
}
