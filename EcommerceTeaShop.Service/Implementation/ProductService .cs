using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IGenericRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDTO> GetProductsAsync(int pageNumber, int pageSize)
    {
        ResponseDTO dto = new();

        try
        {
            var db = _productRepository.GetDbContext();

            var query = db.Set<Product>()
                .Include(x => x.Category)
                .Where(x => x.IsActive);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = data.Select(p => new ReadProductDTO
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name
            });

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Message = "Lấy danh sách sản phẩm thành công.";
            dto.Data = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = mapped
            };
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

    public async Task<ResponseDTO> GetProductByIdAsync(Guid id)
    {
        ResponseDTO dto = new();

        try
        {
            var product = await _productRepository.GetDbContext()
                .Set<Product>()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Không tìm thấy sản phẩm.";
                return dto;
            }

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Data = new ReadProductDTO
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryName = product.Category.Name
            };
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

    public async Task<ResponseDTO> SearchProductsAsync(string keyword, int pageNumber, int pageSize)
    {
        ResponseDTO dto = new();

        try
        {
            var db = _productRepository.GetDbContext();

            var query = db.Set<Product>()
                .Include(x => x.Category)
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()));

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = $"Không tìm thấy sản phẩm {keyword}.";
                return dto;
            }

            var mapped = data.Select(p => new ReadProductDTO
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name
            });

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Data = mapped;
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

    public async Task<ResponseDTO> GetProductsByCategoryAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        ResponseDTO dto = new();

        try
        {
            var db = _productRepository.GetDbContext();

            var query = db.Set<Product>()
                .Include(x => x.Category)
                .Where(x => x.CategoryId == categoryId);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = data.Select(p => new ReadProductDTO
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name
            });

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Data = mapped;
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

    public async Task<ResponseDTO> CreateProductAsync(CreateProductDTO dtoRequest)
    {
        ResponseDTO dto = new();

        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dtoRequest.Name,
                Description = dtoRequest.Description,
                Price = dtoRequest.Price,
                StockQuantity = dtoRequest.StockQuantity,
                CategoryId = dtoRequest.CategoryId,
                IsActive = true
            };

            await _productRepository.Insert(product);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
            dto.Message = "Tạo sản phẩm thành công.";
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

    public async Task<ResponseDTO> DeleteProductAsync(Guid id)
    {
        ResponseDTO dto = new();

        try
        {
            var product = await _productRepository.GetById(id);

            if (product == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Không tìm thấy sản phẩm.";
                return dto;
            }

            await _productRepository.Delete(product);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
            dto.Message = "Xóa sản phẩm thành công.";
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }


    public async Task<ResponseDTO> UpdateProductAsync(Guid id, UpdateProductDTO dtoRequest)
    {
        ResponseDTO dto = new();

        try
        {
            var product = await _productRepository.GetById(id);

            if (product == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Không tìm thấy sản phẩm.";
                return dto;
            }

            if (dtoRequest.Name != null)
            {
                if (string.IsNullOrWhiteSpace(dtoRequest.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên sản phẩm không hợp lệ.";
                    return dto;
                }

                product.Name = dtoRequest.Name;
            }

            if (dtoRequest.Description != null)
                product.Description = dtoRequest.Description;

            if (dtoRequest.Price.HasValue)
            {
                if (dtoRequest.Price <= 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Giá sản phẩm phải lớn hơn 0.";
                    return dto;
                }

                product.Price = dtoRequest.Price.Value;
            }

            if (dtoRequest.StockQuantity.HasValue)
            {
                if (dtoRequest.StockQuantity < 0)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Số lượng tồn kho không hợp lệ.";
                    return dto;
                }

                product.StockQuantity = dtoRequest.StockQuantity.Value;
            }

            if (dtoRequest.IsActive.HasValue)
                product.IsActive = dtoRequest.IsActive.Value;

            await _productRepository.Update(product);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            dto.Message = "Cập nhật sản phẩm thành công.";
        }
        catch (Exception ex)
        {
            dto.IsSucess = false;
            dto.BusinessCode = BusinessCode.EXCEPTION;
            dto.Message = ex.Message;
        }

        return dto;
    }

}