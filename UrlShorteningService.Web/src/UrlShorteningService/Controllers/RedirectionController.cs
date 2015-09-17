using System;
using Microsoft.AspNet.Mvc;
using UrlShorteningService.Services;

namespace UrlShorteningService.Controllers
{
    [Route("")]
    public class RedirectionController : Controller
    {
        private readonly IRepository _repository;
        private readonly IIdGenerator _idGenerator;
        private readonly IBase58Converter _base58Converter;
        private readonly IAdminCodeGenerator _adminCodeGenerator;

        public RedirectionController(IRepository repository, IIdGenerator idGenerator, IBase58Converter base58Converter, IAdminCodeGenerator adminCodeGenerator)
        {
            _repository = repository;
            _idGenerator = idGenerator;
            _base58Converter = base58Converter;
            _adminCodeGenerator = adminCodeGenerator;
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
                var id = _idGenerator.NextId();
                url = _base58Converter.ToString(id);
            }

            _repository.Add(new LinkEntry
            {
                Id = url,
                AdminCode = _adminCodeGenerator.Generate(),
                Link = uri.ToString()
            });

            return Redirect("/r/" + url);
        }
    }
}