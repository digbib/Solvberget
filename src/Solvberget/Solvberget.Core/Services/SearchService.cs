﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solvberget.Core.DTOs;
using Solvberget.Core.DTOs.Deprecated.DTO;
using Solvberget.Core.Properties;
using Solvberget.Core.Services.Interfaces;

namespace Solvberget.Core.Services
{
    class SearchService : ISearchService
    {
        private readonly IStringDownloader _stringDownloader;

        public SearchService(IStringDownloader stringDownloader)
        {
            _stringDownloader = stringDownloader;
        }

        public async Task<IEnumerable<DocumentDto>>  Search(string query)
        {
            try
            {
                var response = await _stringDownloader.Download(Resources.ServiceUrl + string.Format(Resources.ServiceUrl_Search, query));
                return JsonConvert.DeserializeObject<List<DocumentDto>>(response);
            }
            catch (Exception e)
            {
                return new List<DocumentDto>
                {
                    new DocumentDto
                    {
                        Title = "Kunne ikke hente resultater"
                    }
                };
            }
        }
    }
}
