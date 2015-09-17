using System;
using Microsoft.AspNet.Mvc;
using UrlShorteningService.Services;

namespace UrlShorteningService.Controllers
{
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
                var url = $"http://307.ch/{id}";
                var escaped = Uri.EscapeDataString(url);
                var qr = $"https://api.qrserver.com/v1/create-qr-code/?data={escaped}&size=200x200";

                var dto = new ResultDto
                {
                    Id = link.Id,
                    Url = url,
                    Link = link.Link,
                    Qr = qr
                };
                return View("Result", dto);
            }

            Context.Response.StatusCode = 404;
            return View("404");
        }

        public class ResultDto
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public string Link { get; set; }
            public string Qr { get; set; }
        }
    }
}
