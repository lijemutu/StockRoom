using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockRoomTest
{
    public class BotTest
    {
        [Fact]
        public async Task BotGetEndpoint200()
        {
            // Create an instance of HttpClient to send the request
            var client = new HttpClient();

            // Set the base address of the endpoint you want to test
            client.BaseAddress = new Uri("https://localhost:7221/");

            // Send a GET request to the endpoint and get the response
            var response = await client.GetAsync("StockBot?stockName=appl.us");

            // Ensure that the response status code is OK
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Extract the content of the response as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert that the response content is what you expect
            //Assert.("Hello, world!", responseContent);
        }
    }
}
