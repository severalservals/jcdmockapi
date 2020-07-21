using NUnit.Framework;
using JcdMockApi; 
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace JcdMockApiTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var client = new Transport(new JCDMockAPI(new Uri("http://localhost:5000")));
            var result = await client.WombatsWithHttpMessagesAsync();

            // If we want to do authentication...
            //var credentials = new TokenCredentials("<bearer token>");

            //             var client = new JcdMockApi(new Uri("http://localhost:50960", UriKind.Absolute), credentials);
            // var result = await client.CreateBookingWithHttpMessagesAsync("12345", new BookFast.Client.Models.BookingData
            //              {
            //                  FromDate = DateTime.Parse("2016-05-01"),
            //                  ToDate = DateTime.Parse("2016-05-08")
            //              });

            Console.WriteLine(result.Body); 
            string[] wombats = JsonSerializer.Deserialize<string[]>(result.Body); 
            Assert.IsTrue(wombats[2] == "Plain-nosed Wombat");
        }
    }
}