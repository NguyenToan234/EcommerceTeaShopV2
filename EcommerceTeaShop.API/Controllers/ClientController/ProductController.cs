using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/product")]
[ApiController]

public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(int pageNumber = 1, int pageSize = 10)
    {
        var result = await _productService.GetProductsAsync(pageNumber, pageSize);
        return StatusFromResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return StatusFromResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _productService.SearchProductsAsync(keyword, pageNumber, pageSize);
        return StatusFromResult(result);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _productService.GetProductsByCategoryAsync(categoryId, pageNumber, pageSize);
        return StatusFromResult(result);
    }

    private IActionResult StatusFromResult(ResponseDTO result)
    {
        if (result == null)
            return StatusCode(500, "Server error");

        return result.BusinessCode switch
        {
            // 400
            BusinessCode.VALIDATION_FAILED or
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
                => StatusCode(201, result),

            // 200
            BusinessCode.GET_DATA_SUCCESSFULLY or
            BusinessCode.UPDATE_SUCESSFULLY or
            BusinessCode.DELETE_SUCESSFULLY
                => Ok(result),

            _ => Ok(result)
        };
    }
}