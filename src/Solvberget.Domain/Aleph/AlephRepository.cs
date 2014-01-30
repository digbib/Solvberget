﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Solvberget.Domain.Documents;
using Solvberget.Domain.Documents.Images;
using Solvberget.Domain.Events;
using Solvberget.Domain.Users;
using Solvberget.Domain.Utils;

namespace Solvberget.Domain.Aleph
{
    public class AlephRepository : IRepository
    {

        private readonly StorageHelper _storageHelper;
        private readonly RulesRepository _rulesRepository;
        private readonly ImageRepository _imageRepository;

        private const string UserPinSucessReply = "Passordet er sendt";

        public AlephRepository(IEnvironmentPathProvider environment)
        {
            var pathToImageCache = environment.GetImageCachePath();
            var pathToRulesFolder = environment.GetRulesPath();

            _storageHelper = new StorageHelper(pathToImageCache);
            _imageRepository = new ImageRepository(null, environment);
            if (pathToRulesFolder != null)
                _rulesRepository = new RulesRepository(pathToRulesFolder);

        }

        public List<Document> Search(string value)
        {
            dynamic result = new ExpandoObject();
            const Operation function = Operation.KeywordSearch;
            var options = new Dictionary<string, string> { { "request", value } };
            
            XDocument doc = GetDocumentFromAleph(function, options);

            if (doc != null && doc.Root != null)
            {
                result.SetNumber = doc.Root.Elements("set_number").Select(x => x.Value).FirstOrDefault();
                result.NumberOfRecords = doc.Root.Elements("no_records").Select(x => x.Value).FirstOrDefault();
                return result.SetNumber != null ? GetSearchResults(result) : new List<Document>();

            }

            return new List<Document>();
        }

        public Document GetDocument(string documentNumber, bool isLight)
        {
            const Operation function = Operation.FindDocument;
            var options = new Dictionary<string, string> { { "doc_number", documentNumber } };

            var url = GetUrl(function, options);

            XDocument doc = GetDocumentFromAleph(function, options);

            if (doc != null && doc.Root != null)
            {
                var xmlResult = doc.Root.Elements("record").Select(x => x).FirstOrDefault();
                if (xmlResult != null)
                {

                    //Add Aleph document information
                    var docToReturn = PopulateDocument(xmlResult, isLight);

                    //We add the docnumber here because it is not in the result 
                    //when getting the document it self from Aleph
                    docToReturn.DocumentNumber = documentNumber;

                    //Add Alpeh location and availability information if not light
                    if (!isLight)
                        GenerateDocumentLocationAndAvailabilityInfo(docToReturn);

                    //Try to get ThumbnailUrl, and also ImageUrl if not light.
                    docToReturn.ThumbnailUrl = _storageHelper.GetLocalImageFileCacheUrl(docToReturn.DocumentNumber, true);
                    if (!isLight)
                        docToReturn.ImageUrl = _storageHelper.GetLocalImageFileCacheUrl(docToReturn.DocumentNumber, false);

                    return docToReturn;

                }
            }

            return null;

        }

        private XDocument GetDocumentFromAleph(Operation function, Dictionary<string, string> options)
        {
            try
            {
                var url = GetUrl(function, options);
                return RepositoryUtils.GetXmlFromStream(url);
            }
            catch (WebException ex)
            {
                throw new AlephException("Aleph is currently unavailable");
            }
        }

        public UserInfo GetUserInformation(string userId, string verification)
        {
            var user = new UserInfo { BorrowerId = userId };
            
            if (!AuthenticateUser(ref user, userId, verification))
            {
                return null;
            }

            const Operation function = Operation.UserInformation;
            var options = new Dictionary<string, string> { { "bor_id", userId }, { "verification", verification } };
           
            var userXDoc = GetDocumentFromAleph(function, options);

            user.FillProperties(userXDoc.ToString());
            
            return user;
        }

        public RequestReply CancelReservation(string documentItemNumber, string documentItemSequence, string cancellationSequence)
        {
            const Operation function = Operation.CancelReservation;
            var options = new Dictionary<string, string> { { "doc_number", documentItemNumber }, { "item_sequence", documentItemSequence }, { "sequence", cancellationSequence } };
            
            var reservationReplyXml = GetDocumentFromAleph(function, options);

            if (reservationReplyXml != null && reservationReplyXml.Root != null)
            {

                var item = reservationReplyXml.Root.Element("reply") ?? reservationReplyXml.Root.Element("error");
                if (item != null)
                    return item.Value.Equals("ok") ? new RequestReply { Success = true, Reply = "Reservasjonen ble fjernet!" } : new RequestReply { Success = false, Reply = item.Value };
            }
            return new RequestReply { Success = false, Reply = "Feil: Kan ikke fjerne valgt dokument akkurat nå." };
        }

        public RequestReply RequestReservation(string documentNumber, string userId, string branch)
        {

            if (documentNumber == null || userId == null || branch == null)
                return new RequestReply { Success = false, Reply = "Feil: Operasjonen mangler parametre." };


            if (branch.Equals("Hovedbibl"))
                branch = "Hovedbibl.";
            var docItems = GetDocumentItems(documentNumber).ToList();

            var docItem = docItems.FirstOrDefault(documentItem => documentItem.Branch.Equals(branch) && documentItem.IsReservable);
            if (docItem != null)
            {
                var alephReturnMessage = GetReserveRequest(docItem.ItemAdmKey, docItem.ItemKeySequence, userId);
                return alephReturnMessage.Equals("ok") ? new RequestReply { Success = true, Reply = "Reservasjonen var vellykket!" } : new RequestReply { Success = false, Reply = alephReturnMessage };
            }
            return new RequestReply { Success = false, Reply = "Feil: Dokumentene er for tiden ikke tilgjengelig for reservering." };
        }

        private string GetReserveRequest(string documentAdm, string itemSequence, string userId)
        {
            const Operation function = Operation.ReserveDocument;
            var options = new Dictionary<string, string> { { "doc_number", documentAdm }, { "item_sequence", itemSequence }, { "bor_id", userId } };
            
            var docItemsXml = GetDocumentFromAleph(function, options);

            if (docItemsXml != null && docItemsXml.Root != null)
            {
                var item = docItemsXml.Root.Element("reply") ?? docItemsXml.Root.Element("error");
                if (item != null)
                    return item.Value;
            }
            return "Feil: Klarte ikke å hente ut ønsket informasjon fra returnert xml-ark.";
        }

        public RequestReply RequestRenewalOfLoan(string documentNumber, string itemSecq, string barcode, string libraryUserId)
        {
            var renewalRequest = GetLoanRenewalRequest(documentNumber, itemSecq, barcode, libraryUserId);


            if (renewalRequest != null)
            {
                if (renewalRequest.Equals("ok"))
                {
                    return new RequestReply { Success = true, Reply = "Lånetiden er utvidet" };
                }
                return new RequestReply { Success = false, Reply = renewalRequest };
            }
            return new RequestReply { Success = false, Reply = "Feil: Dokumentet er for tiden ikke tilgjengelig for utviding av lånetid" };
        }

        public RequestReply RequestPinCodeToSms(string userId)
        {

            if (String.IsNullOrEmpty(userId)) 
                return new RequestReply { Success = false, Reply = "Vennligst oppgi et lånenummer." };

            //REQUEST
            var request = WebRequest.Create(Properties.Settings.Default.PinToSmsUrl);
            request.Method = "POST";
            var postData = "bor_id=" + userId;
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            //RESPONSE
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            var responseFromServer = reader.ReadToEnd();
                        
                            if (responseFromServer.Contains(UserPinSucessReply))
                                return new RequestReply { Success = true, Reply = "Din pin-kode vil bli tilsendt via SMS." };

                            return new RequestReply { Success = false, Reply = "Forespørselen kunne ikke utføres. Vennligst sjekk lånenummeret." };

                        }
                }

                return new RequestReply { Success = false, Reply = "Forespørselen kunne ikke utføres." };

            }

        }

        private string GetLoanRenewalRequest(string documentNr, string itemSequence, string itemBarcode, string libraryUserId)
        {
            const Operation function = Operation.RenewLoan;
            var options = new Dictionary<string, string> { { "doc_number", documentNr }, { "item_sequence", itemSequence }, { "bor_id", libraryUserId }, { "item_barcode", itemBarcode } };
            
            var docItemsXml = GetDocumentFromAleph(function, options);

            if (docItemsXml != null && docItemsXml.Root != null)
            {

                var xElement = docItemsXml.Root.Element("reply");
                if (xElement != null) return xElement.Value;

                xElement = docItemsXml.Root.Element("error-text-1");
                if (xElement != null) return "Lånet kan ikke utvides flere ganger";

                xElement = docItemsXml.Root.Element("error-text-2");
                if (xElement != null) return "Lånet er resistrert som mistet";

                xElement = docItemsXml.Root.Element("error");
                if (xElement != null)
                {
                    return xElement.Value == "New due date must be bigger than current's loan due date" ? "Lånetiden kan ikke utvides mer enn den er nå" : "Det har oppstått en feil, vennligst kontakt biblioteket";
                }
            }

            return "Det har oppstått en feil, vennligst kontakt biblioteket";
        }

        private void GenerateDocumentLocationAndAvailabilityInfo(Document document)
        {
            var documentItems = GetDocumentItems(document.DocumentNumber);
            document.AvailabilityInfo = AvailabilityRepository.GenerateLocationAndAvailabilityInfo(documentItems, document);
        }

        private IEnumerable<DocumentItem> GetDocumentItems(string documentNumber)
        {
            var function = Operation.DocumentItems;
            var options = new Dictionary<string, string> { { "doc_number", documentNumber } };
            
            var docItemsXml = GetDocumentFromAleph(function, options);

            function = Operation.CircStatus;
            options = new Dictionary<string, string> { { "sys_no", documentNumber } };
            
            var docCircItemXml = GetDocumentFromAleph(function, options);

            return DocumentItemRepository.GetDocumentItemsFromXml(docItemsXml.ToString(), docCircItemXml.ToString(), _rulesRepository);
        }

        private bool AuthenticateUser(ref UserInfo user, string userId, string verification)
        {

            const Operation function = Operation.AuthenticateUser;
            var options = new Dictionary<string, string> { { "bor_id", userId }, { "verification", verification } };
            
            var authenticationDoc = GetDocumentFromAleph(function, options);

            if (authenticationDoc != null && authenticationDoc.Root != null)
            {

                var xElement = authenticationDoc.Root.DescendantsAndSelf("z303").FirstOrDefault();
                user.IsAuthorized = xElement != null;

            }

            return user.IsAuthorized;
        }

        private List<Document> GetSearchResults(dynamic result)
        {
            var numberOfRecords = int.Parse(result.NumberOfRecords);

            var paralellRequestUrls = new List<string>();

            var numberOfSections = numberOfRecords/100;
            for (var i = 0; i <= numberOfSections; i++)
            {
                var range = string.Format("{0}-{1}", (i*100).ToString("000000000"), (i*100 + 99).ToString("000000000"));
                paralellRequestUrls.Add(range);
                
                if (i > 10) break;
            }

            var partialLists = new List<Document>[paralellRequestUrls.Count];
            Parallel.ForEach(paralellRequestUrls, new ParallelOptions {MaxDegreeOfParallelism = 5}, (range, loopstate, index) =>
            {
                var options = new Dictionary<string, string> { { "set_number", result.SetNumber }, { "set_entry", range } };
                
                var doc = GetDocumentFromAleph(Operation.PresentSetNumber, options);
                if (doc == null || doc.Root == null) return;

                var tmpList = new List<Document>();
                var xmlResult = doc.Root.Elements("record").Select(x => PopulateDocument(x, true)).Where(xDoc => xDoc.Title != null);
                foreach (var document in xmlResult)
                {
                    document.ThumbnailUrl = _storageHelper.GetLocalImageFileCacheUrl(document.DocumentNumber, true);
                    tmpList.Add(document);
                }
                partialLists[index] = tmpList;
            });

            return partialLists.SelectMany(l => l).ToList();
        }

        private Document PopulateDocument(XElement record, bool populateLight)
        {
            var xmlDoc = XDocument.Parse(record.ToString());
            var nodes = xmlDoc.Root.Descendants("oai_marc");

            var docTypeString = Document.GetVarfield(nodes, "019", "b");

            Document returnDocument;

            if (docTypeString != null)
            {
                var className = GetDocumentType(docTypeString.Split(','));

                var type = Type.GetType(className);

                var methodInfo = type.GetMethod(populateLight ? "GetObjectFromFindDocXmlBsMarcLight" : "GetObjectFromFindDocXmlBsMarc");

                returnDocument = (Document)methodInfo.Invoke(type, BindingFlags.InvokeMethod | BindingFlags.Default, null, new object[] { record.ToString() }, CultureInfo.CurrentCulture);

            }
            else
            {
                returnDocument = Document.GetObjectFromFindDocXmlBsMarcLight(record.ToString());
            }
            //new Thread(() => _imageRepository.GetDocumentImage(returnDocument.DocumentNumber, null, returnDocument, false)).Start();
            return returnDocument;
        }

        private string GetUrl(Operation function, Dictionary<string, string> options)
        {
            var sb = new StringBuilder();
            sb.Append(Properties.Settings.Default.AlephServerUrl);
            sb.Append(GetOperationPrefix(function));
            foreach (var option in options)
            {
                sb.Append(string.Format("&{0}={1}", option.Key, option.Value));
            }
            return sb.ToString();
        }

        private static string GetOperationPrefix(Operation op)
        {
            switch ((int)op)
            {
                case 0:
                    return "op=item-data&base=NOR01";
                case 1:
                    return "op=present&base=NOR50";
                case 2:
                    return "op=find&base=NOR01";
                case 3:
                    return "op=find-doc&base=NOR01";
                case 4:
                    return "op=bor-auth&library=nor50";
                case 5:
                    return "op=bor-info&library=nor50";
                case 6:
                    return "op=hold-req&library=NOR50";
                case 7:
                    return "op=circ-status&library=NOR01";
                case 8:
                    return "op=renew&library=NOR50";
                case 9:
                    return "op=hold-req-cancel&library=NOR50";
                default:
                    return null;
            }
        }

        private enum Operation { DocumentItems, PresentSetNumber, KeywordSearch, FindDocument, AuthenticateUser, UserInformation, ReserveDocument, CircStatus, RenewLoan, CancelReservation }

        private static string GetDocumentType(IEnumerable<string> documentTypeCodes)
        {

            var dtc = new HashSet<string>(documentTypeCodes.Select(x => x.Trim()));

            if (dtc.Contains("l"))
                return typeof(Book).FullName;
            else if (dtc.Any(x => x.StartsWith("e")))
                return typeof(Film).FullName;
            else if (dtc.Contains("dc") && dtc.Contains("dg"))
                return typeof(Cd).FullName;
            else if (dtc.Contains("di"))
                return typeof(AudioBook).FullName;
            else if (dtc.Contains("c"))
                return typeof(SheetMusic).FullName;
            else if (dtc.Contains("dh"))
                return typeof(LanguageCourse).FullName;
            else if (dtc.Contains("j"))
                return typeof(Journal).FullName;
            else if (dtc.Any(x => x.StartsWith("m")))
                return typeof (Game).FullName;

            return typeof(Document).FullName;

        }

    }
}
