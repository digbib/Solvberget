﻿using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.LightningCache.Extensions;
using Solvberget.Core.DTOs;
using Solvberget.Domain.Documents;
using Solvberget.Domain.Documents.Reviews;
using Solvberget.Domain.Favorites;
using Solvberget.Domain.Utils;
using Solvberget.Nancy.Authentication;
using Solvberget.Nancy.Mapping;
using Solvberget.Domain.Aleph;
using Solvberget.Domain.Documents.Images;
using Solvberget.Domain.Documents.Ratings;

namespace Solvberget.Nancy.Modules
{
    public class DocumentModule : NancyModule
    {
        public DocumentModule(IRepository documents, IImageRepository images, IRatingRepository ratings, IReviewRepository reviews, IFavoritesRepository favorites, IEnvironmentPathProvider pathProvider)
            : base("/documents")
        {
            Get["/{id}/thumbnail"] = args =>
            {
                var doc = documents.GetDocument(args.id, true);
                string img = images.GetDocumentImage(args.id);

                if (String.IsNullOrEmpty(img))
                {
                    return ResolvePlaceHolderImageForDocumentType(pathProvider, doc);
                }

                return Response.AsFile(Path.Combine(pathProvider.GetImageCachePath(), img));
            };

            Get["/{id}"] = args =>
            {
                Document document = documents.GetDocument(args.id, false);
                return Response.AsJson(DtoMaps.Map(document, favorites, Context.GetUserInfo()));
            };
            
            Get["/{id}/rating"] = args =>
            {
                try
                {
                    DocumentRating rating = ratings.GetDocumentRating(args.id);

                    return Response.AsJson(new DocumentRatingDto
                    {
                        MaxScore = rating.MaxScore,
                        Score = rating.Score,
                        Source = rating.Source,
                        SourceUrl = rating.SourceUrl,
                        HasRating = true
                    }).AsCacheable(DateTime.Now.AddDays(1));
                }
                catch
                {
                    return new DocumentRatingDto {Success = true, HasRating = false};
                }
            };

            Get["/{id}/review"] = args =>
            {
                string review = reviews.GetDocumentReview(args.id);
                return Response.AsJson(new DocumentReviewDto{ Review = review, Url = "" }).AsCacheable(DateTime.Now.AddDays(1));
            };

            Get["/search"] = _ =>
            {
                string query = Request.Query.query.HasValue ? Request.Query.query : null;

                if (null == query) throw new InvalidOperationException("Ingenting å søke etter.");

                return Response.AsJson(documents.Search(query).Select(doc => DtoMaps.Map(doc, favorites, Context.GetUserInfo())).ToArray()).AsCacheable(DateTime.Now.AddHours(12));
            };
        }

        private dynamic ResolvePlaceHolderImageForDocumentType(IEnvironmentPathProvider pathProvider, dynamic doc)
        {
            if (doc is Cd)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Cd.png"));
            }
            if (doc is Book)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Book.png"));
            }
            if (doc is Film)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Film.png"));
            }
            if (doc is SheetMusic)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "SheetMusic.png"));
            }
            if (doc is Journal)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Journal.png"));
            }
            if (doc is Game)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Game.png"));
            }
            if (doc is AudioBook)
            {
                return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "AudioBook.png"));
            }
            return Response.AsFile(Path.Combine(pathProvider.GetPlaceHolderImagesPath(), "Document.png"));
        }
    }
}
