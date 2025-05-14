using Microsoft.AspNetCore.Mvc;
using ProductService.Application; // Required to use ProductAppService and ProductDto

namespace ProductService.Api.Controllers;

/// <summary>
/// API endpoints for managing products.
/// This controller belongs to the API layer and delegates work to the Application layer (ProductAppService).
/// </summary>
[ApiController]
[Route("api/[controller]")] // Defines the base route: /api/products
public class ProductsController : ControllerBase
{
    private readonly ProductAppService _productAppService;
    // Consider injecting an interface (e.g., IProductAppService) instead of the concrete class
    // for better testability and adherence to dependency inversion, but concrete class is simpler for now.

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productAppService">The application service instance provided by DI.</param>
    public ProductsController(ProductAppService productAppService)
    {
        _productAppService = productAppService ?? throw new ArgumentNullException(nameof(productAppService));
    }

    /// <summary>
    /// Gets all products.
    /// Corresponds to GET /api/products
    /// </summary>
    /// <returns>A list of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)] // Swagger/OpenAPI documentation
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var products = await _productAppService.GetAllProductsAsync();
        
        // Check for our variant environment variable
        var serviceVariant = Environment.GetEnvironmentVariable("SERVICE_VARIANT");

        if (serviceVariant == "variantB")
        {
            // Example: Add a suffix to product names for variant B
            var variantProducts = products.Select(p => p with { Name = p.Name + " (Variant B)" });
            return Ok(variantProducts);
        }
        
        return Ok(products); // Returns HTTP 200 OK with the list of DTOs
    }

    /// <summary>
    /// Gets a specific product by its unique identifier.
    /// Corresponds to GET /api/products/{id}
    /// </summary>
    /// <param name="id">The GUID identifier of the product.</param>
    /// <returns>The requested product or a Not Found response.</returns>
    [HttpGet("{id:guid}")] // Route constraint ensures 'id' is a valid GUID
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(404)] // Not Found
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var product = await _productAppService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(); // Returns HTTP 404 Not Found
        }

        return Ok(product); // Returns HTTP 200 OK with the ProductDto
    }

    /// <summary>
    /// Creates a new product.
    /// Corresponds to POST /api/products
    /// </summary>
    /// <param name="request">The details for the product to create.</param>
    /// <returns>A response indicating the creation and location of the new product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), 201)] // Created
    [ProducesResponseType(400)] // Bad Request (e.g., validation errors)
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Note: In a real app, add input validation here or using framework features (FluentValidation, DataAnnotations)
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        try
        {
            var createdProduct = await _productAppService.CreateProductAsync(
                request.Name,
                request.Description,
                request.Price);

            // Return HTTP 201 Created status
            // Includes a 'Location' header pointing to the new resource URL (using the GetProductById action)
            // Also includes the created product DTO in the response body
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (ArgumentException ex) // Catch potential validation errors from domain/app service
        {
            // Map domain/application exceptions to appropriate HTTP status codes
            return BadRequest(ex.Message);
        }
        // Consider more specific exception handling for production code
    }
}

/// <summary>
/// Represents the request model for creating a new product.
/// This is specific to the API layer.
/// </summary>
/// <param name="Name">The desired name of the product.</param>
/// <param name="Description">The desired description.</param>
/// <param name="Price">The desired price.</param>
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price
);