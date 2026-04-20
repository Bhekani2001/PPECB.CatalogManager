using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class InventoryTransactionBusinessLogic : IInventoryTransactionBusinessLogic
    {
        private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;

        public InventoryTransactionBusinessLogic(
            IInventoryTransactionRepository inventoryTransactionRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IMapper mapper)
        {
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id)
        {
            var transaction = await _inventoryTransactionRepository.GetByIdWithIncludesAsync(id, t => t.Product, t => t.Warehouse, t => t.PerformedByUser);
            return transaction != null ? _mapper.Map<InventoryTransactionDto>(transaction) : null;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductAsync(int productId)
        {
            var transactions = await _inventoryTransactionRepository.GetTransactionsByProductAsync(productId);
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseAsync(int warehouseId)
        {
            var transactions = await _inventoryTransactionRepository.GetTransactionsByWarehouseAsync(warehouseId);
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        }

        public async Task<PagedResultDto<InventoryTransactionDto>> GetPagedTransactionsAsync(int pageNumber, int pageSize = 20)
        {
            var pagedResult = await _inventoryTransactionRepository.GetPagedTransactionsAsync(pageNumber, pageSize);
            return new PagedResultDto<InventoryTransactionDto>
            {
                Items = _mapper.Map<List<InventoryTransactionDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<InventoryTransactionDto> AddStockAsync(int productId, int quantity, int? warehouseId, string referenceNumber, string notes, int performedByUserId)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Validate warehouse if provided
            if (warehouseId.HasValue)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId.Value);
                if (warehouse == null)
                {
                    throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found.");
                }
            }

            // Get current stock
            var currentStock = await GetCurrentStockAsync(productId, warehouseId);

            // Create transaction
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Type = TransactionType.Receipt,
                Quantity = quantity,
                PreviousStock = currentStock,
                NewStock = currentStock + quantity,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                PerformedByUserId = performedByUserId > 0 ? performedByUserId : null,  // Only set if valid
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByUserId.ToString()
            };

            await _inventoryTransactionRepository.AddAsync(transaction);

            // Update product stock
            await _productRepository.UpdateStockQuantityAsync(productId, currentStock + quantity);
            await _inventoryTransactionRepository.SaveChangesAsync();

            return _mapper.Map<InventoryTransactionDto>(transaction);
        }
        public async Task<InventoryTransactionDto> RemoveStockAsync(int productId, int quantity, int? warehouseId, string referenceNumber, string notes, int performedByUserId)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            // Get current stock
            var currentStock = await GetCurrentStockAsync(productId, warehouseId);

            // Check if sufficient stock
            if (currentStock < quantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {currentStock}, Requested: {quantity}");
            }

            // Create transaction
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Type = TransactionType.Issue,
                Quantity = quantity,
                PreviousStock = currentStock,
                NewStock = currentStock - quantity,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                PerformedByUserId = performedByUserId,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByUserId.ToString()
            };

            await _inventoryTransactionRepository.AddAsync(transaction);

            // Update product stock
            await _productRepository.UpdateStockQuantityAsync(productId, currentStock - quantity);
            await _inventoryTransactionRepository.SaveChangesAsync();

            return _mapper.Map<InventoryTransactionDto>(transaction);
        }

        public async Task<InventoryTransactionDto> TransferStockAsync(int productId, int quantity, int fromWarehouseId, int toWarehouseId, string referenceNumber, string notes, int performedByUserId)
        {
            // Validate warehouses
            var fromWarehouse = await _warehouseRepository.GetByIdAsync(fromWarehouseId);
            if (fromWarehouse == null)
            {
                throw new KeyNotFoundException($"Source warehouse with ID {fromWarehouseId} not found.");
            }

            var toWarehouse = await _warehouseRepository.GetByIdAsync(toWarehouseId);
            if (toWarehouse == null)
            {
                throw new KeyNotFoundException($"Destination warehouse with ID {toWarehouseId} not found.");
            }

            // Remove from source warehouse
            await RemoveStockAsync(productId, quantity, fromWarehouseId, referenceNumber, $"Transfer out to warehouse {toWarehouseId}", performedByUserId);

            // Add to destination warehouse
            var result = await AddStockAsync(productId, quantity, toWarehouseId, referenceNumber, $"Transfer in from warehouse {fromWarehouseId}", performedByUserId);

            return result;
        }

        public async Task<InventoryTransactionDto> AdjustStockAsync(int productId, int newQuantity, string reason, int performedByUserId)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            var currentStock = product.StockQuantity;
            var difference = newQuantity - currentStock;

            if (difference == 0)
            {
                throw new InvalidOperationException("New quantity is the same as current stock. No adjustment needed.");
            }

            var transactionType = difference > 0 ? TransactionType.Receipt : TransactionType.Issue;

            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Type = transactionType,
                Quantity = Math.Abs(difference),
                PreviousStock = currentStock,
                NewStock = newQuantity,
                ReferenceNumber = "ADJUSTMENT",
                Notes = $"Stock adjustment: {reason}",
                PerformedByUserId = performedByUserId,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = performedByUserId.ToString()
            };

            await _inventoryTransactionRepository.AddAsync(transaction);

            // Update product stock
            await _productRepository.UpdateStockQuantityAsync(productId, newQuantity);
            await _inventoryTransactionRepository.SaveChangesAsync();

            return _mapper.Map<InventoryTransactionDto>(transaction);
        }

        public async Task<int> GetCurrentStockAsync(int productId, int? warehouseId = null)
        {
            return await _inventoryTransactionRepository.GetCurrentStockForProductAsync(productId, warehouseId);
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            return await _inventoryTransactionRepository.GetTotalStockValueAsync();
        }

        public async Task<decimal> GetTotalStockValueByWarehouseAsync(int warehouseId)
        {
            return await _inventoryTransactionRepository.GetTotalStockValueByWarehouseAsync(warehouseId);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetProductMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _inventoryTransactionRepository.GetProductMovementHistoryAsync(productId, startDate, endDate);
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        }
    }
}