using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

namespace UrlShorteningService.Controllers
{
    [Route("")]
    public class RedirectionController : Controller
    {
        private readonly IDictionary<string, string> _redirects = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "google", "http://www.google.com" },
            { "20", "http://www.20min.ch" }
        };

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            string url;

            if (_redirects.TryGetValue(id, out url))
            {
                return new RedirectResult(url, permanent: false);
            }

            Context.Response.StatusCode = 404;
            return View("404");
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }
    }

    [Route("api/v1/[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/values
        [HttpGet("")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
