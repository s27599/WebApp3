using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class ProductService : IProductService
{
    private readonly IConfiguration _configuration;

    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesProductExist(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", productDto.IdProduct);
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        return reader is not null;
    }

    public async Task<bool> DoesWarehouseExist(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        command.Parameters.AddWithValue("@IdWarehouse", productDto.IdWarehouse);
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        return reader is not null;
    }

    public async Task<bool> IsOrderCreated(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM \"Order\" WHERE IdProduct = @IdProduct AND amount = @amount AND CreatedAt <= GetDate()"; 
        command.Parameters.AddWithValue("@IdProduct", productDto.IdProduct);
        command.Parameters.AddWithValue("@amount", productDto.Amount);
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        return reader is not null;
    }

    public async Task<bool> CheckIfMistakenOrder(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 From Product_Warehouse WHERE IdOrder = (SELECT IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct AND amount = @amount )";
        command.Parameters.AddWithValue("@IdProduct", productDto.IdProduct);
        command.Parameters.AddWithValue("@amount", productDto.Amount);
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        return reader is not null;
    }

    public async  Task UpdateFullfilledAt(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "UPDATE \"Order\" SET FulfilledAt = GetDate() WHERE IdProduct = @IdProduct AND amount = @amount";
        command.Parameters.AddWithValue("@IdProduct", productDto.IdProduct);
        command.Parameters.AddWithValue("@amount", productDto.Amount);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertRecord(ProductDTO productDto)
    { 
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command2 = new SqlCommand();
        await connection.OpenAsync();
        command2.Connection = connection;
        command2.CommandText = "Select Price From Product Where IdProduct = @IdProduct";
        command2.Parameters.AddWithValue("@IdProduct", productDto.IdProduct);
        var price = await command2.ExecuteScalarAsync();
        float.TryParse(price.ToString(), out float priceFloat);
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = 
            "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)" +
            "VALUES (@idWarehouse, @idProduct, (SELECT IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct2 AND amount = @amount2)" +
            ", @amount, @price, GETDATE())";
        command.Parameters.AddWithValue("@idWarehouse", productDto.IdWarehouse);
        command.Parameters.AddWithValue("@idProduct", productDto.IdProduct);
        command.Parameters.AddWithValue("@IdProduct2", productDto.IdProduct);
        command.Parameters.AddWithValue("@amount2", productDto.Amount);
        command.Parameters.AddWithValue("@amount", productDto.Amount);
        command.Parameters.AddWithValue("@price", priceFloat*productDto.Amount);
        await command.ExecuteNonQueryAsync();
       
    }

    public async Task<int> GetID(ProductDTO productDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdOrder = " +
                              "(SELECT IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct AND amount = @amount)";
        command.Parameters.AddWithValue("IdProduct", productDto.IdProduct);
        command.Parameters.AddWithValue("amount", productDto.Amount);
        await connection.OpenAsync();
        var reader =  await command.ExecuteScalarAsync();

        int.TryParse(reader.ToString(),out int id);
        return id;
    }
}