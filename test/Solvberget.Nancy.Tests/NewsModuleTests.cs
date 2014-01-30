﻿using System;
using System.Collections.Generic;

using FakeItEasy;

using Nancy.Testing;

using Should;
using Solvberget.Core.DTOs;
using Solvberget.Domain.Info;
using Solvberget.Nancy.Modules;

using Xunit;

namespace Solvberget.Nancy.Tests
{
    public class NewsModuleTests
    {
        private readonly INewsRepository _repository;

        private readonly Browser _browser;

        public NewsModuleTests()
        {
            _repository = A.Fake<INewsRepository>();
            _browser = new Browser(config =>
            {
                config.Module<NewsModule>();
                config.Dependency(_repository);
            });
        }

        [Fact]
        public void GetShouldFetchNewsItemsFromRepository()
        {
            // Given
            A.CallTo(() => _repository.GetNewsItems(30)).Returns(new List<NewsItem>
            {
                new NewsItem
                {
                    Title = "Google Offers Android app gets a new look and simpler redemption process.",
                    DescriptionUnescaped = "Blah blah blah..."
                },
                new NewsItem
                {
                    Title = "T-Mobile launches trio of budget Android smartphones and an LTE hotspot.",
                    DescriptionUnescaped = "Blah blah blah..."
                }
            });

            // When
            var response = _browser.Get("/news", with =>
            {
                with.Query("limit", "30");
                with.Accept("application/json");
                with.HttpRequest();
            });

            // Then
            response.Body.DeserializeJson<List<NewsStoryDto>>().Count.ShouldEqual(2);
        }

        [Fact]
        public void Should_handle_requests_without_specific_amount_limit_and_default_to_a_reasonable_amount()
        {
            A.CallTo(() => _repository.GetNewsItems(10)).Returns(new List<NewsItem>
            {
                new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, 
                new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, new NewsItem { Title = "Foo", DescriptionUnescaped = "Bar"}, 
            });

            var response = _browser.Get("/news", with =>
            {
                with.Accept("application/json");
                with.HttpRequest();
            });

            response.Body.DeserializeJson<List<NewsStoryDto>>().Count.ShouldEqual(10);
        }
    }
}