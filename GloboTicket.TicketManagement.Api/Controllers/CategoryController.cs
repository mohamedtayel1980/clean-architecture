﻿using GloboTicket.TicketManagement.Application.Features.Categories.Commands.CreateCateogry;
using GloboTicket.TicketManagement.Application.Features.Categories.Queries.GetCategoriesList;
using GloboTicket.TicketManagement.Application.Features.Categories.Queries.GetCategoriesListWithEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.TicketManagement.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("all",Name ="GetAllCategroies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryListVm>>> GetAllCategories()
        {
            var dtos = await _mediator.Send(new GetCategoriesListQuery());
            return Ok(dtos);
        }

        [HttpGet("allwithevents", Name = "GetCategoriesWithEvents")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryEventListVm>>> GetCategoriesWithEvents(bool includeHistory)
        {
            GetCategoriesListWithEventsQuery getCategoriesListWithEventsQuery = 
                new GetCategoriesListWithEventsQuery() { IncludeHistory = includeHistory };

            var dtos = await _mediator.Send(getCategoriesListWithEventsQuery);
            return Ok(dtos);
        }

        [HttpPost(Name = "AddCategory")]
        public async Task<ActionResult<CreateCategoryCommandResponse>> Create([FromBody] CreateCategoryCommand createCategoryCommand)
        {
            var response = await _mediator.Send(createCategoryCommand);
            return Ok(response);
        }
    }
}
