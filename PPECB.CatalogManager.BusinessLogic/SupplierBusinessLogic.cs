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
    public class SupplierBusinessLogic : ISupplierBusinessLogic
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;

        public SupplierBusinessLogic(ISupplierRepository supplierRepository, IMapper mapper)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            return supplier != null ? _mapper.Map<SupplierDto>(supplier) : null;
        }

        public async Task<SupplierDto?> GetSupplierByCodeAsync(string code)
        {
            var supplier = await _supplierRepository.GetByCodeAsync(code);
            return supplier != null ? _mapper.Map<SupplierDto>(supplier) : null;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync()
        {
            var suppliers = await _supplierRepository.GetActiveSuppliersAsync();
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto, string createdBy)
        {
            if (!await IsSupplierCodeUniqueAsync(createDto.Code))
            {
                throw new InvalidOperationException($"Supplier code '{createDto.Code}' already exists.");
            }

            var supplier = new Supplier
            {
                Code = createDto.Code,
                Name = createDto.Name,
                ContactPerson = createDto.ContactPerson,
                Email = createDto.Email,
                Phone = createDto.Phone,
                Address = createDto.Address,
                TaxNumber = createDto.TaxNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var created = await _supplierRepository.AddAsync(supplier);
            await _supplierRepository.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(created);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierDto updateDto, string updatedBy)
        {
            var supplier = await _supplierRepository.GetByIdAsync(updateDto.Id);
            if (supplier == null)
            {
                throw new KeyNotFoundException($"Supplier with ID {updateDto.Id} not found.");
            }

            if (supplier.Code != updateDto.Code && !await IsSupplierCodeUniqueAsync(updateDto.Code, updateDto.Id))
            {
                throw new InvalidOperationException($"Supplier code '{updateDto.Code}' already exists.");
            }

            supplier.Code = updateDto.Code;
            supplier.Name = updateDto.Name;
            supplier.ContactPerson = updateDto.ContactPerson;
            supplier.Email = updateDto.Email;
            supplier.Phone = updateDto.Phone;
            supplier.Address = updateDto.Address;
            supplier.TaxNumber = updateDto.TaxNumber;
            supplier.IsActive = updateDto.IsActive;
            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedBy = updatedBy;

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int id, string deletedBy)
        {
            var productCount = await GetSupplierProductCountAsync(id);
            if (productCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete supplier with {productCount} associated products.");
            }
            return await _supplierRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm)
        {
            var suppliers = await _supplierRepository.SearchSuppliersAsync(searchTerm);
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<bool> IsSupplierCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _supplierRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public async Task<int> GetSupplierProductCountAsync(int supplierId)
        {
            return await _supplierRepository.GetProductCountAsync(supplierId);
        }

        public async Task UpdateSupplierStatusAsync(int supplierId, bool isActive)
        {
            await _supplierRepository.UpdateSupplierStatusAsync(supplierId, isActive);
            await _supplierRepository.SaveChangesAsync();
        }
    }
}