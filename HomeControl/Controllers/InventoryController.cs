using HomeControl.Database;
using HomeControl.DatabaseModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Controllers
{
    [ApiController]
    [Route("Inventory")]
    public class InventoryController(IDatabaseConnectionService db) : Controller
    {
        public class BookStockRequest
        {
            public int ProductId { get; set; }

            public int LocationId { get; set; }

            public decimal Quantity { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> OnGet()
        {
            var stockSelect = db.Select<Stock>();
            stockSelect.Where().Compare(x => x.Quantity, ComparisonOperator.NotEquals, 0m);
            stockSelect.LeftJoin(x => x.Product);
            stockSelect.LeftJoin(x => x.Location);

            return Json(await stockSelect.ExecuteAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            var stockSelect = db.SelectSingle<Stock>(id);
            stockSelect.LeftJoin(x => x.Product);
            stockSelect.LeftJoin(x => x.Location);

            return Json(await stockSelect.ExecuteAsync());
        }

        [HttpPost("BookStock")]
        public async Task<IActionResult> BookStock([FromBody] BookStockRequest request)
        {
            var stockSelect = db.Select<Stock>();
            stockSelect.Where().Compare(x => x.ProductId, ComparisonOperator.Equals, request.ProductId).And().Compare(x => x.LocationId, ComparisonOperator.Equals, request.LocationId);
            stockSelect.LeftJoin(x => x.Product);
            stockSelect.LeftJoin(x => x.Location);

            var stock = await stockSelect.ExecuteAsync();

            Stock stockItem;

            if (stock.Count == 0)
            {
                if (request.Quantity < 0) return BadRequest("Stock Cannot be negativ");

                stockItem = new Stock
                {
                    LocationId = request.LocationId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };

                await db.Insert(stockItem).ExecuteAsync();

                var stockSelectSingle = db.SelectSingle<Stock>(stockItem.Id);
                stockSelectSingle.LeftJoin(x => x.Product);
                stockSelectSingle.LeftJoin(x => x.Location);

                stockItem = await stockSelectSingle.ExecuteAsync();
            }
            else if (stock.Count == 1)
            {
                stockItem = stock[0];

                stockItem.Quantity += request.Quantity;

                if (request.Quantity < 0 && stockItem.Quantity < 0) return BadRequest("Stock Cannot be negativ");

                await db.Update(stockItem).ExecuteAsync();
            }
            else return StatusCode(StatusCodes.Status500InternalServerError);

            return Json(stockItem);
        }
    }
}