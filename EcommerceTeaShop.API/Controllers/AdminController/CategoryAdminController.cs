using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/category")]
[Authorize(Roles = "ADMIN")]
public class CategoryAdminController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryAdminController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);
        return StatusFromResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return StatusFromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDTO dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return StatusFromResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryDTO dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return StatusFromResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return StatusFromResult(result);
    }

    private IActionResult StatusFromResult(ResponseDTO result)
    {
        if (result == null)
            return StatusCode(500, new { message = "Server không phản hồi." });

        return result.BusinessCode switch
        {
            // 400
            BusinessCode.VALIDATION_FAILED or
            BusinessCode.VALIDATION_ERROR or
            BusinessCode.INVALID_INPUT or
            BusinessCode.INVALID_DATA
                => BadRequest(result),

            // 404
            BusinessCode.DATA_NOT_FOUND
                => NotFound(result),

            // 500
            BusinessCode.EXCEPTION or
            BusinessCode.INTERNAL_ERROR
                => StatusCode(500, result),

            // 201
            BusinessCode.INSERT_SUCESSFULLY or
            BusinessCode.CREATED_SUCCESSFULLY
                => StatusCode(StatusCodes.Status201Created, result),

            // 200
            BusinessCode.GET_DATA_SUCCESSFULLY or
            BusinessCode.UPDATE_SUCESSFULLY or
            BusinessCode.DELETE_SUCESSFULLY
                => Ok(result),

            _ => Ok(result)
        };
    }
}