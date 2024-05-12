using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IProductService
{
    Task<bool> DoesProductExist(ProductDTO productDto);
    Task<bool> DoesWarehouseExist(ProductDTO productDto);
    Task<bool> IsOrderCreated(ProductDTO productDto);
    Task<bool> CheckIfMistakenOrder(ProductDTO productDto);
    Task UpdateFullfilledAt(ProductDTO productDto);
    Task InsertRecord(ProductDTO productDto);
    Task<int> GetID(ProductDTO productDto);
}