using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.DB;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDTO> GetAllCategoriesAsync(int pageNumber, int pageSize)
    {
        ResponseDTO dto = new();

        try
        {
            var db = _categoryRepository.GetDbContext();

            var query = db.Set<Category>().AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .OrderByDescending(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = data.Select(c => new ReadCategoryDTO
            {
                CategoryId = c.Id,
                Name = c.Name,
                Description = c.Description

            }).ToList();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Message = "Lấy danh sách danh mục thành công.";
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
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Lỗi khi lấy danh sách danh mục: " + ex.Message
            };
        }

        return dto;
    }

    public async Task<ResponseDTO> GetCategoryByIdAsync(Guid id)
    {
        ResponseDTO dto = new();

        try
        {
            var category = await _categoryRepository.GetById(id);

            if (category == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Không tìm thấy category.";
                return dto;
            }

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Message = "Lấy category thành công.";
            dto.Data = new ReadCategoryDTO
            {
                CategoryId = category.Id,
                Name = category.Name
            };
        }
        catch (Exception ex)
        {
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Lỗi khi lấy category: " + ex.Message
            };
        }

        return dto;
    }

    public async Task<ResponseDTO> CreateCategoryAsync(CreateCategoryDTO request)
    {
        ResponseDTO dto = new();

        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                dto.Message = "Tên danh mục không được để trống.";
                return dto;
            }

            var existed = await _categoryRepository.GetFirstByExpression(
                x => x.Name.ToLower() == request.Name.ToLower()
            );

            if (existed != null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DUPLICATE_DATA;
                dto.Message = "Danh mục đã tồn tại.";
                return dto;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim()
            };

            await _categoryRepository.Insert(category);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
            dto.Message = "Tạo danh mục thành công.";
        }
        catch (Exception ex)
        {
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Không thể tạo danh mục: " + ex.Message
            };
        }

        return dto;
    }

    public async Task<ResponseDTO> UpdateCategoryAsync(Guid id, UpdateCategoryDTO request)
    {
        ResponseDTO dto = new();

        try
        {
            var category = await _categoryRepository.GetById(id);

            if (category == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Danh mục không tồn tại.";
                return dto;
            }

            if (request.Name != null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    dto.Message = "Tên danh mục không hợp lệ.";
                    return dto;
                }

                category.Name = request.Name.Trim();
            }

            await _categoryRepository.Update(category);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            dto.Message = "Cập nhật danh mục thành công.";
        }
        catch (Exception ex)
        {
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Lỗi khi cập nhật danh mục: " + ex.Message
            };
        }

        return dto;
    }

    public async Task<ResponseDTO> DeleteCategoryAsync(Guid id)
    {
        ResponseDTO dto = new();

        try
        {
            var category = await _categoryRepository.GetById(id);

            if (category == null)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = "Danh mục không tồn tại.";
                return dto;
            }

            await _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangeAsync();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
            dto.Message = "Xóa danh mục thành công.";
        }
        catch (Exception ex)
        {
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Không thể xóa danh mục: " + ex.Message
            };
        }

        return dto;
    }


    public async Task<ResponseDTO> SearchCategoriesAsync(string keyword, int pageNumber, int pageSize)
    {
        ResponseDTO dto = new();

        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.VALIDATION_FAILED;
                dto.Message = "Từ khóa không được để trống.";
                return dto;
            }

            var db = _categoryRepository.GetDbContext();

            var query = db.Set<Category>()
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()));

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                dto.Message = $"Không tìm thấy danh mục \"{keyword}\".";
                return dto;
            }

            var mapped = data.Select(c => new ReadCategoryDTO
            {
                CategoryId = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();

            dto.IsSucess = true;
            dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            dto.Message = "Tìm kiếm danh mục thành công.";
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
            dto = new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Lỗi khi tìm kiếm danh mục : " + ex.Message
            };
        }

        return dto;
    }
}