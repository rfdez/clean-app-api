﻿using AutoMapper;
using CleanApp.Core.CustomEntities;
using CleanApp.Core.DTOs;
using CleanApp.Core.Entities;
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
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriSerice;

        public TenantController(ITenantService tenantService, IMapper mapper, IUriService uriService)
        {
            _tenantService = tenantService;
            _mapper = mapper;
            _uriSerice = uriService;
        }

        /// <summary>
        /// Obtiene todos los inquilinos
        /// </summary>
        /// <param name="filters">Nombre del inquilino</param>
        /// <returns>Inquilino</returns>
        [HttpGet(Name = nameof(GetTenants))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TenantDto>>), StatusCodes.Status200OK)]
        public IActionResult GetTenants([FromQuery] TenantQueryFilter filters)
        {
            var tenants = _tenantService.GetTenants(filters);
            var tenantsDto = _mapper.Map<IEnumerable<TenantDto>>(tenants);

            var metadata = new Metadata
            {
                TotalCount = tenants.TotalCount,
                PageSize = tenants.PageSize,
                CurrentPage = tenants.CurrentPage,
                TotalPages = tenants.TotalPages,
                HasNextPage = tenants.HasNextPage,
                HasPreviousPage = tenants.HasPreviousPage,
                NextPageUrl = tenants.HasNextPage ? _uriSerice.GetPaginationUri((int)tenants.NextPageNumber, tenants.PageSize, Url.RouteUrl(nameof(GetTenants))).ToString() : null,
                PreviousPageUrl = tenants.HasPreviousPage ? _uriSerice.GetPaginationUri((int)tenants.NextPageNumber, tenants.PageSize, Url.RouteUrl(nameof(GetTenants))).ToString() : null

            };

            var response = new ApiResponse<IEnumerable<TenantDto>>(tenantsDto)
            {
                Meta = metadata
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un inquilino
        /// </summary>
        /// <param name="id">Identificador del inquilino</param>
        /// <returns>Inquilino</returns>
        [HttpGet("{id}", Name = nameof(GetTenant))]
        [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenant(int id)
        {
            var tenant = await _tenantService.GetTenant(id);
            var tenantDto = _mapper.Map<TenantDto>(tenant);

            var response = new ApiResponse<TenantDto>(tenantDto);

            return Ok(response);
        }

        /// <summary>
        /// Inserta un inquilino
        /// </summary>
        /// <param name="tenantDto">Inquilino a insertar</param>
        /// <returns>Inquilino insertado</returns>
        [HttpPost(Name = nameof(InsertTenant))]
        [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> InsertTenant(TenantDto tenantDto)
        {
            var tenant = _mapper.Map<Tenant>(tenantDto);
            await _tenantService.InsertTenant(tenant);
            tenantDto = _mapper.Map<TenantDto>(tenant);

            return CreatedAtAction(nameof(GetTenant), new { id = tenantDto.Id }, new ApiResponse<TenantDto>(tenantDto));
        }

        /// <summary>
        /// Actualiza un inquilino
        /// </summary>
        /// <param name="id">Identificador del inquilino</param>
        /// <param name="tenantDto">Nuevos valores del inquilino</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = nameof(UpdateTenantAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTenantAsync(int id, TenantDto tenantDto)
        {
            var tenant = _mapper.Map<Tenant>(tenantDto);
            tenant.Id = id;

            await _tenantService.UpdateTenantAsync(tenant);

            return NoContent();
        }

        /// <summary>
        /// Elimina un inquilino
        /// </summary>
        /// <param name="id">Identificador del inquilino</param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = nameof(DeleteTenant))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            await _tenantService.DeleteTenant(id);

            return NoContent();
        }
    }
}
