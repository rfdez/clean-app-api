﻿using CleanApp.Core.CustomEntities;
using CleanApp.Core.Entities;
using CleanApp.Core.Exceptions;
using CleanApp.Core.Interfaces;
using CleanApp.Core.QueryFilters;
using CleanApp.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace CleanApp.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaginationOptions _paginationOptions;

        public RoomService(IUnitOfWork unitOfWork, IOptions<PaginationOptions> options)
        {
            _unitOfWork = unitOfWork;
            _paginationOptions = options.Value;
        }

        public PagedList<Room> GetRooms(RoomQueryFilter filters)
        {
            filters.PageNumber = filters.PageNumber == 0 ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == 0 ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var rooms = _unitOfWork.RoomRepository.GetAll();

            if (filters.RoomName != null)
            {
                rooms = rooms.Where(r => r.RoomName.ToLower().Contains(filters.RoomName)).AsEnumerable();
            }

            var pagedRooms = PagedList<Room>.Create(rooms.Count() > 0 ? rooms : throw new BusinessException("No hay habitaciones disponibles."), filters.PageNumber, filters.PageSize);

            return pagedRooms;
        }

        public async Task<Room> GetRoom(int id)
        {
            return await _unitOfWork.RoomRepository.GetById(id) ?? throw new BusinessException("No existe la habitación solicitada.");
        }

        public async Task InsertRoom(Room room)
        {
            var existsHome = await _unitOfWork.HomeRepository.GetById(room.HomeId);

            if (existsHome == null)
            {
                throw new BusinessException("No existe la vivienda a asignar.");
            }

            await _unitOfWork.RoomRepository.Add(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            var exists = await _unitOfWork.RoomRepository.GetById(room.Id);

            if (exists == null)
            {
                throw new BusinessException("No existe la habitación a actualizar.");
            }

            var existsHome = await _unitOfWork.HomeRepository.GetById(room.HomeId);

            if (existsHome == null)
            {
                throw new BusinessException("No existe la vivienda a asignar.");
            }

            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteRoom(int id)
        {
            var exists = await _unitOfWork.RoomRepository.GetById(id);

            if (exists == null)
            {
                throw new BusinessException("No existe la habitación a eliminar.");
            }

            await _unitOfWork.RoomRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
