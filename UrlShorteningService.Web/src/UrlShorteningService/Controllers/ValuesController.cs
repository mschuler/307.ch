using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using UrlShorteningService.Services;

namespace UrlShorteningService.Controllers
{
    [Route("")]
    public class RedirectionController : Controller
    {
        private readonly IRepository _repository;

        public RedirectionController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var entry = _repository.Get(id);
            var link = entry as LinkEntry;

            if (link != null)
            {
                return new RedirectResult(link.Link, permanent: false);
            }

            Context.Response.StatusCode = 404;
            return View("404");
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("")]
        public IActionResult Create(string url, string link)
        {
            Uri uri;
            if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
            {
                Context.Response.StatusCode = 500;
                return View("500");
            }

            if (string.IsNullOrEmpty(url))
            {
                url = Guid.NewGuid().ToString("n");
            }

            _repository.Add(new LinkEntry { Id = url, Link = uri.ToString() });

            return Redirect("/r/" + url);
        }
    }

    [Route("r")]
    public class ResultController : Controller
    {
        private readonly IRepository _repository;

        public ResultController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public IActionResult Result(string id)
        {
            var entry = _repository.Get(id);
            var link = entry as LinkEntry;

            if (link != null)
            {
                var url = Uri.EscapeDataString("http://307.ch/" + id);
                var qr = string.Format("https://api.qrserver.com/v1/create-qr-code/?data={0}&size=200x200", url);

                var dto = new ResultDto
                {
                    Id = link.Id,
                    Link = link.Link,
                    Qr = qr
                };
                return View("Result", dto);
            }

            Context.Response.StatusCode = 404;
            return View("404");
        }
    }

    public class ResultDto
    {
        public string Id { get; set; }
        public string Link { get; set; }
        public string Qr { get; set; }
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
