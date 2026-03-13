using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Mvc;

[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);
        return StatusFromResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _categoryService.SearchCategoriesAsync(keyword, pageNumber, pageSize);
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