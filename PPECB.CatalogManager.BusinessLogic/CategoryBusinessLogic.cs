using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class CategoryBusinessLogic : ICategoryBusinessLogic
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryBusinessLogic(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<CategoryDto?> GetCategoryByCodeAsync(string code)
        {
            var category = await _categoryRepository.GetByCodeAsync(code);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<CategoryDto?> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, string createdBy)
        {
            // Validate unique code
            if (!await IsCategoryCodeUniqueAsync(createDto.Code))
            {
                throw new InvalidOperationException($"Category code '{createDto.Code}' already exists.");
            }

            // Validate unique name
            if (!await IsCategoryNameUniqueAsync(createDto.Name))
            {
                throw new InvalidOperationException($"Category name '{createDto.Name}' already exists.");
            }

            var category = new Category
            {
                Code = createDto.Code.ToUpper(),
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // Generate slug
            category.Slug = GenerateSlug(createDto.Name);

            // Set level and path
            category.Level = 1;
            category.Path = category.Name;

            var created = await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(created);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateDto, string updatedBy)
        {
            var category = await _categoryRepository.GetByIdAsync(updateDto.Id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {updateDto.Id} not found.");
            }

            // Validate unique code (excluding current category)
            if (category.Code != updateDto.Code && !await IsCategoryCodeUniqueAsync(updateDto.Code, updateDto.Id))
            {
                throw new InvalidOperationException($"Category code '{updateDto.Code}' already exists.");
            }

            // Validate unique name (excluding current category)
            if (category.Name != updateDto.Name && !await IsCategoryNameUniqueAsync(updateDto.Name, updateDto.Id))
            {
                throw new InvalidOperationException($"Category name '{updateDto.Name}' already exists.");
            }

            category.Code = updateDto.Code.ToUpper();
            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.IsActive = updateDto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = updatedBy;
            category.Slug = GenerateSlug(updateDto.Name);

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id, string deletedBy)
        {
            if (!await CanDeleteCategoryAsync(id))
            {
                throw new InvalidOperationException("Cannot delete category that has products.");
            }

            return await _categoryRepository.SoftDeleteAsync(id, deletedBy);
        }

        public async Task<bool> HardDeleteCategoryAsync(int id)
        {
            return await _categoryRepository.HardDeleteAsync(id);
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
        {
            var categories = await _categoryRepository.GetRootCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentCategoryId)
        {
            var categories = await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync()
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var categories = _mapper.Map<IEnumerable<CategoryDto>>(allCategories);
            return BuildHierarchy(categories.ToList(), null);
        }

        public async Task<string> GetCategoryPathAsync(int categoryId)
        {
            return await _categoryRepository.GetCategoryPathAsync(categoryId);
        }

        public async Task<bool> IsCategoryCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _categoryRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
        {
            return await _categoryRepository.IsNameUniqueAsync(name, excludeId);
        }

        public async Task<bool> CanDeleteCategoryAsync(int id)
        {
            var productCount = await _categoryRepository.GetProductCountAsync(id);
            return productCount == 0;
        }

        public async Task UpdateCategoryStatusAsync(int id, bool isActive)
        {
            await _categoryRepository.UpdateActiveStatusAsync(id, isActive);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task UpdateCategoryDisplayOrderAsync(int id, int displayOrder)
        {
            await _categoryRepository.UpdateDisplayOrderAsync(id, displayOrder);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryDto>> SearchCategoriesAsync(string searchTerm)
        {
            var categories = await _categoryRepository.SearchCategoriesAsync(searchTerm);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<int> GetCategoryProductCountAsync(int categoryId)
        {
            return await _categoryRepository.GetProductCountAsync(categoryId);
        }

        public async Task<Dictionary<int, int>> GetAllCategoryProductCountsAsync()
        {
            var categories = await _categoryRepository.GetCategoriesWithProductCountAsync();
            return categories.ToDictionary(c => c.Id, c => c.Products?.Count ?? 0);
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-").Replace("/", "-").Replace("\\", "-");
        }

        private IEnumerable<CategoryDto> BuildHierarchy(List<CategoryDto> categories, int? parentId)
        {
            return categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId,
                    Level = c.Level,
                    Path = c.Path,
                    DisplayOrder = c.DisplayOrder,
                    SubCategories = BuildHierarchy(categories, c.Id).ToList()
                });
        }
    }
}