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
    public class BrandBusinessLogic : IBrandBusinessLogic
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;

        public BrandBusinessLogic(IBrandRepository brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<BrandDto?> GetBrandByIdAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            return brand != null ? _mapper.Map<BrandDto>(brand) : null;
        }

        public async Task<BrandDto?> GetBrandByNameAsync(string name)
        {
            var brand = await _brandRepository.GetByNameAsync(name);
            return brand != null ? _mapper.Map<BrandDto>(brand) : null;
        }

        public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync()
        {
            var brands = await _brandRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandDto>>(brands);
        }

        public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
        {
            var brands = await _brandRepository.GetActiveBrandsAsync();
            return _mapper.Map<IEnumerable<BrandDto>>(brands);
        }

        public async Task<BrandDto> CreateBrandAsync(CreateBrandDto createDto, string createdBy)
        {
            if (!await IsBrandNameUniqueAsync(createDto.Name))
            {
                throw new InvalidOperationException($"Brand name '{createDto.Name}' already exists.");
            }

            var brand = new Brand
            {
                Name = createDto.Name,
                Description = createDto.Description,
                LogoUrl = createDto.LogoUrl,
                Website = createDto.Website,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var created = await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();

            return _mapper.Map<BrandDto>(created);
        }

        public async Task<BrandDto> UpdateBrandAsync(UpdateBrandDto updateDto, string updatedBy)
        {
            var brand = await _brandRepository.GetByIdAsync(updateDto.Id);
            if (brand == null)
            {
                throw new KeyNotFoundException($"Brand with ID {updateDto.Id} not found.");
            }

            if (brand.Name != updateDto.Name && !await IsBrandNameUniqueAsync(updateDto.Name, updateDto.Id))
            {
                throw new InvalidOperationException($"Brand name '{updateDto.Name}' already exists.");
            }

            brand.Name = updateDto.Name;
            brand.Description = updateDto.Description;
            brand.LogoUrl = updateDto.LogoUrl;
            brand.Website = updateDto.Website;
            brand.IsActive = updateDto.IsActive;
            brand.UpdatedAt = DateTime.UtcNow;
            brand.UpdatedBy = updatedBy;

            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();

            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<bool> DeleteBrandAsync(int id, string deletedBy)
        {
            var productCount = await GetBrandProductCountAsync(id);
            if (productCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete brand with {productCount} associated products.");
            }
            return await _brandRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<IEnumerable<BrandDto>> SearchBrandsAsync(string searchTerm)
        {
            var brands = await _brandRepository.SearchBrandsAsync(searchTerm);
            return _mapper.Map<IEnumerable<BrandDto>>(brands);
        }

        public async Task<bool> IsBrandNameUniqueAsync(string name, int? excludeId = null)
        {
            return await _brandRepository.IsNameUniqueAsync(name, excludeId);
        }

        public async Task<int> GetBrandProductCountAsync(int brandId)
        {
            return await _brandRepository.GetProductCountAsync(brandId);
        }

        public async Task UpdateBrandStatusAsync(int brandId, bool isActive)
        {
            await _brandRepository.UpdateBrandStatusAsync(brandId, isActive);
            await _brandRepository.SaveChangesAsync();
        }
    }
}