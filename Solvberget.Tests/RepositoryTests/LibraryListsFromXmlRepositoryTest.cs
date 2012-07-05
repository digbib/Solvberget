﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Solvberget.Domain.Implementation;

namespace Solvberget.Service.Tests.RepositoryTests
{
    [TestFixture]
    internal class LibraryListsFromXmlRepositoryTest
    {
        private const string PathString = @"..\..\..\Solvberget.Service\bin\App_Data\librarylists\";
        private LibraryListsFromXmlRepository _listRepository;

        [TestFixtureSetUp]
        public void Init()
        {
            var path = Path.Combine(Environment.CurrentDirectory, PathString);
            _listRepository = new LibraryListsFromXmlRepository(path);
        }

        [Test]
        public void TestCorrectFileCount()
        {
            var result = _listRepository.GetLists();
            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void TestListNameAndContent()
        {
            var list = _listRepository.GetLists().First();
            Assert.AreEqual("Eksempelliste 1", list.Name);
            Assert.AreEqual(1, list.Priority);
            Assert.AreEqual(5, list.DocumentNumbers.Count);
            Assert.AreEqual("000588841", list.DocumentNumbers.ElementAt(0));
            Assert.AreEqual("000588844", list.DocumentNumbers.ElementAt(1));
            Assert.AreEqual("000588843", list.DocumentNumbers.ElementAt(2));
            Assert.AreEqual("000598029", list.DocumentNumbers.ElementAt(3));
            Assert.AreEqual("000567325", list.DocumentNumbers.ElementAt(4));
        }

        [Test]
        public void TestGetListsWithLimit()
        {
            var lists1 = _listRepository.GetLists(2);
            Assert.AreEqual(2, lists1.Count);
            Assert.AreEqual("Eksempelliste 1", lists1.ElementAt(0).Name);
            Assert.AreEqual(1, lists1.ElementAt(0).Priority);
            Assert.AreEqual("Eksempelliste 2", lists1.ElementAt(1).Name);
            Assert.AreEqual(2, lists1.ElementAt(1).Priority);

            //Case where limit is higher than file conunt in folder
            var lists2 = _listRepository.GetLists(6);
            Assert.AreEqual(5, lists2.Count);
        }
    }
}
