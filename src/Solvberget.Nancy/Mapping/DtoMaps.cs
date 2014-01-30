﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Core;
using Solvberget.Core.DTOs;
using Solvberget.Domain.Aleph;
using Solvberget.Domain.Documents;
using Solvberget.Domain.Events;
using Solvberget.Domain.Favorites;
using Solvberget.Domain.Lists;
using Solvberget.Domain.Users;
using Solvberget.Domain.Utils;

namespace Solvberget.Nancy.Mapping
{
    public static class DtoMaps
    {
        public static LibrarylistDto Map(LibraryList list, IRepository documents = null)
        {
            if (null != documents)
            {
                return new LibrarylistDto
                {
                    Id = list.Id,
                    Name = list.Name,
                    Documents = list.Documents.Count > 0
                        ? list.Documents.Select(Map).ToList()
                        : list.DocumentNumbers.Keys.Select(dn => Map(documents.GetDocument(dn, true))).ToList()
                };
            }

            return new LibrarylistDto
            {
                Id = list.Id,
                Name = list.Name
            };
        }

        public static ReservationDto Map(Reservation reservation, IRepository documents)
        {
            DateTime holdEnd;
            DateTime holdFrom;
            DateTime holdTo;

            DateTime.TryParse(reservation.HoldRequestEnd, out holdEnd);
            DateTime.TryParse(reservation.HoldRequestFrom, out holdFrom);
            DateTime.TryParse(reservation.HoldRequestTo, out holdTo);

            return new ReservationDto
            {
                Document = Map(documents.GetDocument(reservation.DocumentNumber, true)),
                Reserved = holdFrom,
                ReadyForPickup = reservation.Status != "In processz38-status", // business logic should not be here! :(
                PickupLocation = reservation.PickupLocation,
                PickupDeadline = holdTo
            };
        }

        public static DocumentDto Map(Document document)
        {
            return Map(document, null, null);
        }

        public static DocumentDto Map(Document document, IFavoritesRepository favorites, UserInfo user)
        {
            DocumentDto dto;

            if (document is Book)
            {
                dto = Map((Book)document);
            }
            else if (document is Film)
            {
                dto = Map((Film) document);
            }
            else if (document is Cd)
            {
                dto = Map((Cd) document);
            }
            else if (document is AudioBook)
            {
                dto = Map((AudioBook) document);
            }
            else if (document is SheetMusic)
            {
                dto = Map((SheetMusic) document);
            }
            else if (document is Game)
            {
                dto = Map((Game) document);
            }
            else if (document is Journal)
            {
                dto = Map((Journal) document);
            }
            else
            {
                dto = new DocumentDto(); // todo other types
            }


            dto.WebAppUrl = Bootstrapper.Container != null ? Bootstrapper.Container.Resolve<IEnvironmentPathProvider>().GetWebAppDocumentDetailsPath(document) : string.Empty;
            dto.Id = document.DocumentNumber;
            dto.Type = document.DocType;
            dto.Title = document.Title;
            if(null == dto.SubTitle) dto.SubTitle = document.CompressedSubTitle; // default only if specific type does not map it
            dto.Availability = MapAvailability(document).ToArray();
            dto.Year = document.PublishedYear;
            dto.Publisher = document.Publisher;
            dto.Language = document.Language;
            dto.Languages = null != document.Languages ? document.Languages.ToArray() : new string[0];
            if(null != user && null != favorites) dto.IsFavorite = favorites.IsFavorite(document, user);
            if (null != user) dto.IsReserved = null != user.Reservations && user.Reservations.Any(r => r.DocumentNumber == document.DocumentNumber);

            return dto;
        }

        public static DocumentDto Map(Journal journal)
        {
            var dto = new JournalDto();
            if (journal.Subject != null) dto.Subjects = journal.Subject.ToArray();
            
            return dto;
        }

        public static DocumentDto Map(Game game)
        {
            var dto = new GameDto();
            dto.Platform = game.LocationCode; // nope
            return dto;
        }

        public static DocumentDto Map(SheetMusic sheetMusic)
        {
            var dto = new SheetMusicDto();

            if(null != sheetMusic.Composer) dto.ComposerName = sheetMusic.Composer.Name;
            dto.CompositionType = sheetMusic.CompositionType;
            dto.NumberOfPagesAndParts = sheetMusic.NumberOfPagesAndNumberOfParts;
            if(null != sheetMusic.MusicalLineup) dto.MusicalLineup = sheetMusic.MusicalLineup.ToArray();
            dto.SubTitle = sheetMusic.SubTitle;

            return dto;
        }

        public static DocumentDto Map(Cd cd)
        {
            var cdDto = new CdDto();

            if(null != cd.ArtistOrComposer) cdDto.ArtistOrComposerName = cd.ArtistOrComposer.Name;
            if(null != cd.CompositionTypeOrGenre) cdDto.CompositionTypesOrGenres = cd.CompositionTypeOrGenre.ToArray();

            return cdDto;
        }

        public static DocumentDto Map(Film film)
        {
            var filmDto = new FilmDto();
            
            if(null != film.Actors) filmDto.ActorNames = film.Actors.Select(a => a.Name).ToArray();
            
            filmDto.AgeLimit = film.AgeLimit;
            
            if(null != film.Genre) filmDto.Genres = film.Genre.ToArray();
            
            filmDto.MediaInfo = film.TypeAndNumberOfDiscs;
            
            if(null != film.ReferredPersons) filmDto.ReferredPeopleNames = film.ReferredPersons.Select(p => p.Name).ToArray();
            if(null != film.ReferencedPlaces) filmDto.ReferencedPlaces = film.ReferencedPlaces.ToArray();
            if(null != film.SubtitleLanguage) filmDto.SubtitleLanguages = film.SubtitleLanguage.ToArray();
            if(null != film.InvolvedPersons) filmDto.InvolvedPersonNames = film.InvolvedPersons.Select(p => string.Format("{0} ({1})", p.Name, p.Role)).ToArray();
            if(null != film.ResponsiblePersons) filmDto.ResponsiblePersonNames = film.ResponsiblePersons.ToArray();

            return filmDto;
        }

        private static DocumentDto Map(Book book)
        {
            var bookDto = new BookDto();

            bookDto.Classification = book.ClassificationNr;
            MapBookProperties(book, bookDto);

            return bookDto;
        }

        private static DocumentDto Map(AudioBook audioBook)
        {
            var bookDto = new AudioBookDto();

            bookDto.Classification = audioBook.ClassificationNumber;
            MapBookProperties(audioBook, bookDto);

            return bookDto;
        }

        private static void MapBookProperties(dynamic bookOrAudioBook, BookDto bookDto)
        {
            bookDto.AuthorName = bookOrAudioBook.Author.Name;
            bookDto.Language = bookOrAudioBook.Language;

            if (!String.IsNullOrEmpty(bookOrAudioBook.SeriesTitle))
            {
                bookDto.Series = new BookSeriesDto
                {
                    Title = bookOrAudioBook.SeriesTitle,
                    SequenceNo = bookOrAudioBook.SeriesNumber
                };
            }
        }

        private static IEnumerable<DocumentAvailabilityDto> MapAvailability(Document document)
        {
            if (null == document.AvailabilityInfo) yield break;

            foreach (var availability in document.AvailabilityInfo)
            {
                if (null == availability) continue;

                var dto = new DocumentAvailabilityDto
                {
                    Branch = availability.Branch,
                    AvailableCount = availability.AvailableCount,
                    TotalCount = availability.TotalCount,

                    Department = availability.Department.DefaultIfEmpty("").Aggregate((acc, dep) =>
                    {
                        if (String.IsNullOrEmpty(acc)) return dep;
                        return acc + " - " + dep;
                    }),

                    Collection = availability.PlacementCode,
                    Location = document.LocationCode
                };
                
                DateTime date;

                if (DateTime.TryParse(availability.EarliestAvailableDateFormatted, out date))
                {
                    dto.EstimatedAvailableDate = date;
                }

                yield return dto;
            }

        }
    }
}
