﻿using AutoMapper;
using CleanApp.Core.CustomEntities;
using CleanApp.Core.DTOs;
using CleanApp.Core.Entities;
using CleanApp.Core.Enumerations;
using CleanApp.Core.QueryFilters;
using CleanApp.Core.Responses;
using CleanApp.Core.Services;
using CleanApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CleanApp.Api.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class WeekController : ControllerBase
    {
        private readonly IWeekService _weekService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriSerice;

        public WeekController(IWeekService weekService, IMapper mapper, IUriService uriService)
        {
            _weekService = weekService;
            _mapper = mapper;
            _uriSerice = uriService;
        }

        /// <summary>
        /// Obtiene todas las semanas
        /// </summary>
        /// <param name="filters">Filtrar semanas por mes</param>
        /// <returns>Lista de semanas</returns>
        [HttpGet(Name = nameof(GetWeeks))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeekDto>>), StatusCodes.Status200OK)]
        public IActionResult GetWeeks([FromQuery] WeekQueryFilter filters)
        {
            var weeks = _weekService.GetWeeks(filters);
            var weeksDto = _mapper.Map<IEnumerable<WeekDto>>(weeks);

            var metadata = new Metadata
            {
                TotalCount = weeks.TotalCount,
                PageSize = weeks.PageSize,
                CurrentPage = weeks.CurrentPage,
                TotalPages = weeks.TotalPages,
                HasNextPage = weeks.HasNextPage,
                HasPreviousPage = weeks.HasPreviousPage,
                NextPageUrl = weeks.HasNextPage ? _uriSerice.GetPaginationUri((int)weeks.NextPageNumber, weeks.PageSize, Url.RouteUrl(nameof(GetWeeks))).ToString() : null,
                PreviousPageUrl = weeks.HasPreviousPage ? _uriSerice.GetPaginationUri((int)weeks.NextPageNumber, weeks.PageSize, Url.RouteUrl(nameof(GetWeeks))).ToString() : null
            };

            var response = new ApiResponse<IEnumerable<WeekDto>>(weeksDto)
            {
                Meta = metadata
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene el mes solicitado
        /// </summary>
        /// <param name="id">Identidficador de la semana</param>
        /// <returns>Una semana</returns>
        [HttpGet("{id}", Name = nameof(GetWeek))]
        [ProducesResponseType(typeof(ApiResponse<WeekDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWeek(int id)
        {
            var week = await _weekService.GetWeek(id);
            var weekDto = _mapper.Map<WeekDto>(week);

            var response = new ApiResponse<WeekDto>(weekDto);

            return Ok(response);
        }

        /// <summary>
        /// Inserta la semana a un mes
        /// </summary>
        /// <param name="weekDto">Semana a insertar</param>
        /// <returns>Semana creada</returns>
        [HttpPost(Name = nameof(InsertWeek))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [ProducesResponseType(typeof(ApiResponse<WeekDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> InsertWeek(WeekDto weekDto)
        {
            var week = _mapper.Map<Week>(weekDto);
            await _weekService.InsertWeek(week);
            weekDto = _mapper.Map<WeekDto>(week);

            return CreatedAtAction(nameof(GetWeek), new { id = weekDto.Id }, new ApiResponse<WeekDto>(weekDto));
        }

        /// <summary>
        /// Actualiza una semana del mes
        /// </summary>
        /// <param name="id">Identificador de la semana</param>
        /// <param name="weekDto">Nuevo valor para la semana</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = nameof(UpdateWeekAsync))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateWeekAsync(int id, WeekDto weekDto)
        {
            var week = _mapper.Map<Week>(weekDto);
            week.Id = id;

            await _weekService.UpdateWeekAsync(week);

            return NoContent();
        }

        /// <summary>
        /// Elimina una semana del mes
        /// </summary>
        /// <param name="id">Identificador de la semana</param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = nameof(DeleteWeek))]
        [Authorize(Roles = nameof(RoleType.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteWeek(int id)
        {
            await _weekService.DeleteWeek(id);

            return NoContent();
        }
    }
}
