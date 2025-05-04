using ProductService.Domain; // Required to access IProductRepository and Product entity

namespace ProductService.Application;

/// <summary>
/// Application service responsible for orchestrating product-related use cases.
/// It interacts with the domain layer (specifically repository interfaces)
/// but does not contain core business logic itself.
/// </summary>
public class ProductAppService // We can define an IProductAppService interface later if needed for abstraction
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductAppService"/> class.
    /// Dependency Injection will provide the concrete implementation of IProductRepository.
    /// </summary>
    /// <param name="productRepository">The product repository.</param>
    public ProductAppService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <summary>
    /// Retrieves a product by its ID and maps it to a ProductDto.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A ProductDto if found; otherwise, null.</returns>
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return null;
        }

        // Manual mapping from Domain Entity to Application DTO
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price
        );
    }

    /// <summary>
    /// Retrieves all products and maps them to ProductDtos.
    /// </summary>
    /// <returns>A collection of ProductDtos.</returns>
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();

        // Manual mapping from Domain Entities to Application DTOs
        return products.Select(product => new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price
        ));
        // Note: For more complex mapping, consider using libraries like AutoMapper.
    }

    /// <summary>
    /// Creates a new product based on the provided details.
    /// </summary>
    /// <param name="name">Product name.</param>
    /// <param name="description">Product description.</param>
    /// <param name="price">Product price.</param>
    /// <returns>The DTO of the newly created product.</returns>
    public async Task<ProductDto> CreateProductAsync(string name, string description, decimal price)
    {
        // Generate a new ID for the product
        var productId = Guid.NewGuid();

        // Create the domain entity using its constructor, which enforces invariants
        var product = new Product(productId, name, description, price);

        // Persist the new product using the repository
        await _productRepository.AddAsync(product);

        // Map the newly created domain entity back to a DTO to return
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price
        );
    }
}