using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("/api")] 
public class WarehouseController : ControllerBase
{
    private readonly IProductService _productService;

    public WarehouseController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> PostProduct(ProductDTO productDto)
    {
        if (!await _productService.DoesProductExist(productDto) ||
            !await _productService.DoesWarehouseExist(productDto))
        {
            return NotFound("Product or Address doesn't exist.");
        }
        if(!await _productService.IsOrderCreated(productDto))
        {
            return NotFound("Order doesn't exist.");
        }

        if (!await _productService.CheckIfMistakenOrder(productDto))
        {
            return BadRequest("Order already exists.");
        }

        try
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _productService.UpdateFullfilledAt(productDto);
                await _productService.InsertRecord(productDto);
                scope.Complete();
            }
        }
        catch (TransactionAbortedException e)
        {
            Console.WriteLine("Transaction aborted: {0}", e.Message);
            throw;
        }
        return Ok(await _productService.GetID(productDto));
    }
}