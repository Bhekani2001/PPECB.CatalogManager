using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class WarehouseBusinessLogic : IWarehouseBusinessLogic
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;

        public WarehouseBusinessLogic(IWarehouseRepository warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
        }

        public async Task<WarehouseDto?> GetWarehouseByCodeAsync(string code)
        {
            var warehouse = await _warehouseRepository.GetByCodeAsync(code);
            return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
        }

        public async Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
            return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
        }

        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createDto, string createdBy)
        {
            if (!await IsWarehouseCodeUniqueAsync(createDto.Code))
            {
                throw new InvalidOperationException($"Warehouse code '{createDto.Code}' already exists.");
            }

            var warehouse = new Warehouse
            {
                Code = createDto.Code,
                Name = createDto.Name,
                Location = createDto.Location,
                Address = createDto.Address,
                ManagerName = createDto.ManagerName,
                Phone = createDto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var created = await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(created);
        }

        public async Task<WarehouseDto> UpdateWarehouseAsync(UpdateWarehouseDto updateDto, string updatedBy)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(updateDto.Id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Warehouse with ID {updateDto.Id} not found.");
            }

            if (warehouse.Code != updateDto.Code && !await IsWarehouseCodeUniqueAsync(updateDto.Code, updateDto.Id))
            {
                throw new InvalidOperationException($"Warehouse code '{updateDto.Code}' already exists.");
            }

            warehouse.Code = updateDto.Code;
            warehouse.Name = updateDto.Name;
            warehouse.Location = updateDto.Location;
            warehouse.Address = updateDto.Address;
            warehouse.ManagerName = updateDto.ManagerName;
            warehouse.Phone = updateDto.Phone;
            warehouse.IsActive = updateDto.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;
            warehouse.UpdatedBy = updatedBy;

            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        public async Task<bool> DeleteWarehouseAsync(int id, string deletedBy)
        {
            return await _warehouseRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<IEnumerable<WarehouseDto>> SearchWarehousesAsync(string searchTerm)
        {
            var warehouses = await _warehouseRepository.SearchWarehousesAsync(searchTerm);
            return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
        }

        public async Task<bool> IsWarehouseCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _warehouseRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public async Task<int> GetWarehouseProductCountAsync(int warehouseId)
        {
            return await _warehouseRepository.GetProductCountAsync(warehouseId);
        }

        public async Task UpdateWarehouseStatusAsync(int warehouseId, bool isActive)
        {
            await _warehouseRepository.UpdateWarehouseStatusAsync(warehouseId, isActive);
            await _warehouseRepository.SaveChangesAsync();
        }
    }
}