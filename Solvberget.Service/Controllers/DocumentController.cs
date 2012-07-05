﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Solvberget.Domain;
using Solvberget.Domain.Abstract;

namespace Solvberget.Service.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IRepository _repository;
        private readonly ISpellingDictionary _spellingRepository;
        private readonly IImageRepository _imageRepository;

        public DocumentController(IRepository repository, ISpellingDictionary spellingRepository, IImageRepository imageRepository)
        {
            _repository = repository;
            _spellingRepository = spellingRepository;
            _imageRepository = imageRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Search(string id)
        {
            var result = _repository.Search(id);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDocument(string id)
        {
            var result = _repository.GetDocument(id);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDocumentImage(string id)
        {
            var result = _imageRepository.GetDocumentImage(id);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SpellingDictionaryLookup(string value)
        {
            var result = _spellingRepository.Lookup(value);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SuggestionList ()
        {
            return this.Json(_spellingRepository.SuggestionList(), JsonRequestBehavior.AllowGet);
        }

    }

}
