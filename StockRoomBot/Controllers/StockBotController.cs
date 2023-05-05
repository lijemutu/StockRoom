using Microsoft.AspNetCore.Mvc;

namespace StockRoomBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockBotController : ControllerBase
    {

        private readonly ILogger<StockBotController> _logger;
        private readonly HttpClient _httpClient;

        public StockBotController(ILogger<StockBotController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [HttpGet(Name = "Stock")]
        public async Task<IActionResult>  Get(string stockName)
        {
            var url = $"https://stooq.com/q/l/?s={stockName}&f=sd2t2ohlcv&h&e=csv";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var csv = await response.Content.ReadAsStringAsync();

                // Convert CSV to JSON
                // Example code: https://stackoverflow.com/a/41736515/979493
                var lines = csv.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
                var headers = lines[0].Split(",");
                var result = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var row = new System.Collections.Generic.Dictionary<string, string>();
                    var values = lines[i].Split(",");
                    for (int j = 0; j < headers.Length; j++)
                    {
                        if (j == headers.Length-1)
                        {
                            headers[j] = headers[j].Replace("\r","");
                            values[j] = values[j].Replace("\r", "");
                        }
                        row.Add(headers[j], values[j]);
                    }
                    result.Add(row);
                }

                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}