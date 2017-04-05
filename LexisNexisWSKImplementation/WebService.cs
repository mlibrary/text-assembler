// COPYRIGHT  2015-16
// MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES
// ALL RIGHTS RESERVED
//
// PERMISSION IS GRANTED TO USE, COPY, CREATE DERIVATIVE WORKS AND REDISTRIBUTE
// THIS SOFTWARE AND SUCH DERIVATIVE WORKS FOR ANY PURPOSE, SO LONG AS THE NAME
// OF MICHIGAN STATE UNIVERSITY IS NOT USED IN ANY ADVERTISING OR PUBLICITY
// PERTAINING TO THE USE OR DISTRIBUTION OF THIS SOFTWARE WITHOUT SPECIFIC,
// WRITTEN PRIOR AUTHORIZATION.  IF THE ABOVE COPYRIGHT NOTICE OR ANY OTHER
// IDENTIFICATION OF MICHIGAN STATE UNIVERSITY IS INCLUDED IN ANY COPY OF ANY
// PORTION OF THIS SOFTWARE, THEN THE DISCLAIMER BELOW MUST ALSO BE INCLUDED.
//
// THIS SOFTWARE IS PROVIDED AS IS, WITHOUT REPRESENTATION FROM MICHIGAN STATE
// UNIVERSITY AS TO ITS FITNESS FOR ANY PURPOSE, AND WITHOUT WARRANTY BY
// MICHIGAN STATE UNIVERSITY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE. THE MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES SHALL
// NOT BE LIABLE FOR ANY DAMAGES, INCLUDING SPECIAL, INDIRECT, INCIDENTAL, OR
// CONSEQUENTIAL DAMAGES, WITH RESPECT TO ANY CLAIM ARISING OUT OF OR IN
// CONNECTION WITH THE USE OF THE SOFTWARE, EVEN IF IT HAS BEEN OR IS HEREAFTER
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
//
// Written by Megan Schanz, 2015-16
// (c) Michigan State University Board of Trustees
// Licensed under GNU General Public License (GPL) Version 2.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;
using System.Net;
using System.Xml;
using LexisNexisWSKImplementation.LNWebReference;
using System.Configuration;
using System.Text;

namespace LexisNexisWSKImplementation
{

    /// <summary>
    /// Performs web service operations against the Lexis Nexis web service reference
    /// </summary>
    public class WebService
    {
        #region Fields

        public string wskID {get; private set;}
        private string wskPassword;
        private string wskEndPoint;
        private static WebService instance;
        private string wskSecurityToken;
        private DateTime wskExpirationTime;

        private int WS_CALL
        {
            get { return AppLookups.getLookupByDescription("Web Service Call").AppLookupCd; }
        }

        #endregion

        #region Construtor

        /// <summary>
        /// Privater constructor for the class to initialize some of the varlaibles
        /// </summary>
        private WebService()
        {
            wskID = "";
            wskPassword = "";
        }

        /// <summary>
        /// Public constructor for the class using the Singleton design pattern to enforce that only 1 web service
        /// instance exists for the session.
        /// </summary>
        public static WebService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WebService();
                }
                return instance;
            }

        }

        #endregion

        #region Web Service Function
      

        /// <summary>
        /// Attempts to authenticate against the Lexis Nexis web service. Will either use the credentials
        /// provided or will use what is in the configuration file.
        /// If any errors occur, they will throw an exception.
        /// </summary>
        /// <param name="wskID">Optional string parameter containing the user name for the web service account</param>
        /// <param name="wskPassword">Optional string parameter containing the password for the web service account</param>
        public void authenticate(string ID = "", string Password = "")
        {
            // Declare variable for the namespace, may not be used
            string wskNamespace = "";

            // Only reset the credentials if not already set in the object
            // this would occur on other functions if the seession expired and this was just called to get a new session
            if (!wskID.Equals(string.Empty)) { wskID = ID; }
            if (!wskPassword.Equals(string.Empty)) { wskPassword = Password; }
            

            // See if the username/password was provided, otherwise get it from the config file
            if (ID.Trim().Equals(string.Empty) || Password.Trim().Equals(string.Empty))
            {
                // Get Authentication values from configuration file
                wskID = ConfigurationManager.AppSettings["wskID"];
                wskPassword = ConfigurationManager.AppSettings["wskPassword"];

                // Get the configured endpoint
                wskEndPoint = ConfigurationManager.AppSettings["wskEndPoint"];

                // Catch the scenario when the config does not have the credentials set either
                if (wskID.Trim().Equals(string.Empty) || wskPassword.Trim().Equals(string.Empty))
                {
                    throw new Exception("No login credentials were provided and no defaults were set in the configurations.");
                }
            }
        
            wskNamespace = ConfigurationManager.AppSettings["wskNamespace"];


            //************************ Authenticate ************************

            // Create the request and response objects
            Authenticate authReq = new Authenticate();
            AuthenticateResponse authResp = null;

            // Populate the request with information
            authReq.authId = wskID;
            authReq.password = wskPassword;
            if (wskNamespace != "")
            {
                authReq.@namespace = wskNamespace;
            }

            AuthenticationSoapBinding authBind = null;
            try
            {
                // Create the client for the web service
                authBind = new AuthenticationSoapBinding();
                //string endpoint =  bind.Endpoint.ListenUri.ToString();  // not used, just saving for reference

                // Fix endpoint URL
                authBind.Url = fixEndPointURL(authBind.Url, wskEndPoint);

                // Make the Web Service call
                authResp = authBind.Authenticate(authReq);

                // Use the response object to check if it was populated
                //// Data in the response object
                //// authResp.binarySecurityToken
                //// authResp.expiration
                //// authResp.userInformation.locale
                if (authResp.binarySecurityToken == null)
                {
                    throw new Exception("No data was in the response object from the web service when some was expected.");
                }
                wskSecurityToken = authResp.binarySecurityToken.ToString();
                wskExpirationTime = authResp.expiration;

            }
            catch (WebException we)
            {
                // Possibly an HTTP Error?
                throw we;
            }
            catch (SoapException se)
            {
                // WSK has returned a Fault message (i.e. an error has occurred)
                // Extract the WSK-specific error code

                // Add a mapping of the XML prefix "p:" to the XML Namespace of WSK
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(se.Detail.OwnerDocument.NameTable);
                nsmgr.AddNamespace("p", "http://services.v1.wsapi.lexisnexis.com.proxy1.cl.msu.edu");

                // Search the Fault message's detail for the  element
                XmlNode nd = se.Detail.SelectSingleNode("//p:errorCode", nsmgr);
                if (nd != null)
                {
                    throw new Exception(nd.InnerText);
                }
                else
                {
                    throw se;
                }
            }
            catch (Exception exc)
            {
                // General Exception.
                throw exc;
            }
            finally
            {
                authBind = null;

            }

            return;
        }
       
        
        public Tuple<List<SearchResult>,string> adhocSearch(string searchText, string[] sourceID, DateTime? fromDate, DateTime? toDate, SearchMethod method, out int totalResults,ref bool limitedRange )
        {
            Tuple<List<SearchResult>, string> returnVal = new Tuple<List<SearchResult>, string>(null,"");
            totalResults = 0;
            DateTime fromParam;
            DateTime toParam;

            // Validate input
            if (searchText.Trim().Equals(string.Empty))
            {
                throw new Exception("No search criteria was provided.");
            }

            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticate();
            }

            // set up date parameters if none provided
            if (fromDate == null)
            {
                string dtParam = AppParams.getParameterByName("BEGIN_DT").AppParamValue;
                string[] dtArray = dtParam.Split('/');
                fromParam = new DateTime(Convert.ToInt32(dtArray[2]), Convert.ToInt32(dtArray[1]), Convert.ToInt32(dtArray[0]));
            }
            else 
            {
                fromParam = fromDate ?? DateTime.Today;
            }
            toParam = toDate ?? DateTime.Today;

            try
            {
                returnVal = adhocSearchRange(searchText, sourceID, fromParam, toParam, method, out totalResults);
            }
            catch (Exception e)
            {

                // check if the search was too general and the date range can not be split anymore
                if (e.Message.Contains("SEARCH_TOO_GENERAL") && (toParam).Subtract(fromParam).Days <= 1)
                {
                    throw new Exception("SEARCHTOO_GENERAL"); // nothing more can be done, stop processing and return
                }
                // check if some other error occured
                if (!e.Message.Contains("SEARCH_TOO_GENERAL"))
                {
                    /// Originally would try the search again to make sure it wasn't just a hiccup in the service, but it was never resolved quickly enough
                    /// that calling it again immediately worked so I commented this part out
                    //if (retryCount == 0)
                    //{
                        
                    //    returnVal = adhocSearch(searchText, sourceID, fromParam, toParam, method, out totalResults);
                    //    retryCount++;
                    //}
                    throw e;
                }
                // else the search was too general but can be split into smaller chunks (for adhoc limit to just one day)
                else
                {
                    /// Originally would try splitting it in half, but did not want to cause too many searches to be run during the day so I limited it to 1 day but commented the code
                    /// for future reference
                    //DateTime middle = fromParam.AddDays(Convert.ToDouble(Math.Floor(Convert.ToDecimal(toParam.Subtract(fromParam).Days)) / Convert.ToDecimal(2)));
                    if ((toParam).Subtract(fromParam).Days <= 1) throw e;
                    limitedRange = true;
                    returnVal = adhocSearch(searchText, sourceID, toParam, toParam, method, out totalResults, ref limitedRange); // retry the search from the  middle to the end of the date range
                    
                }
            }

            return returnVal;
        }
        
        /// <summary>
        /// Performs an ad hoc search returning the full text of the documents.
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="sourceID">Source ID to use for the search</param>
        /// <param name="fromDate">Start date of the search</param>
        /// <param name="toDate">End date of the search</param>
        /// <returns>List of search result objects</returns>
        private Tuple<List<SearchResult>,string> adhocSearchRange(string searchText, string[] sourceID, DateTime fromDate, DateTime toDate, SearchMethod method, out int totalResults)
        {
            totalResults = 0;
            Tuple<List<SearchResult>,string> results = new Tuple<List<SearchResult>,string>(null,"");
            List<SearchResult> resultList = new List<SearchResult>();

            // Validate input
            if (searchText.Trim().Equals(string.Empty))
            {
                throw new Exception("No search criteria was provided.");
            }

            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticate();
            }

            // Determine start and end index for the search 
            int endIndex = Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_ADHOC_SRCH").AppParamValue);
            int startIndex = 1;

            // make sure we don't go over the search cap
            if (endIndex >= Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_SRCH").AppParamValue))
            {
                endIndex = Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_SRCH").AppParamValue);
            }

            // Make sure we're not over the document limit
            if (endIndex >= Convert.ToInt32(AppParams.getParameterByName("DOC_PR_SRCH").AppParamValue))
            {
                endIndex = Convert.ToInt32(AppParams.getParameterByName("DOC_PR_SRCH").AppParamValue);
            }

            //Create the request and response objects
            Search searchReq = new Search();
            SearchResponse searchResp = new SearchResponse();
            //RetrievalResponse gdbrResp = new RetrievalResponse();


            //Populate the request with information
            searchReq.binarySecurityToken = wskSecurityToken;
            searchReq.locale = Locale.enUS;
            searchReq.projectId = AppParams.getParameterByName("PRJ_ID").AppParamValue;
            searchReq.query = searchText;
            searchReq.useCSP = false;

            // Set the search options
            SearchOptions so = new SearchOptions();
            so.searchMethod = method; 
            so.sortOrder = SortOrder.Date;
            searchReq.searchOptions = so;

            // Set the date restriction if provided  
            DateRestriction dr = new DateRestriction();
            dr.endDateSpecified = true;
            dr.endDate = toDate;
            dr.startDateSpecified = true;
            dr.startDate = fromDate;
            searchReq.searchOptions.dateRestriction = dr;


            // Set the source to search
            SourceInformationChoice sic = new SourceInformationChoice();

            /// We have no access to any secured sources so we do not need to loop through all sources to verify that
            //Tuple<string,string> secureSourceDetails =  getSourceDetails(sourceID);
            //sic.securedSourceId = secureSourceDetails.Item2;

            //if (sourceID.Count() > 1 && sic.securedSourceId != null)
            //{
            //    throw new Exception(string.Format("One or more of the sources (including {0}) you selected was secured. Please select only a single secured source or multiple un-secured sources for searching.",secureSourceDetails.Item1));
            //}
            //if (sic.securedSourceId == null)
            //{
            //    sic.sourceIdList = sourceID;
            //}
            sic.sourceIdList = sourceID;
            searchReq.sourceInformation = sic;

            // Set the retrieval options
            RetrievalOptions ro = new RetrievalOptions();
            ro.documentMarkup = DocumentMarkup.Display;
            ro.documentMarkupSpecified = true;
            ro.documentView = DocumentView.FullTextWithTerms;
            ro.documentViewSpecified = true;
            Range range = new Range();
            range.begin = startIndex.ToString();
            range.end = endIndex.ToString();
            ro.documentRange = range;
            searchReq.retrievalOptions = ro;

            SearchSoapBinding searchBind = null;
            RetrievalSoapBinding retrieveBind;
            try
            {
                //Create the binding  
                searchBind = new SearchSoapBinding();

                // Adjust the endpoint URL to point to the correct environment
                searchBind.Url = fixEndPointURL(searchBind.Url, wskEndPoint);


                // Make the Web Service call
                DBManager.Instance.logError("Call to 'Search'", WS_CALL, "ADHOC");
                searchResp = searchBind.Search(searchReq);
                try
                {
                    totalResults = Convert.ToInt32(searchResp.documentsFound);
                }
                catch (FormatException)
                {
                    totalResults = 0;
                }

                //************************ GetDocumentsByDocumentID ************************

                // Get all of the document IDs
                String[] docIds = new String[searchResp.documentContainerList.Length];
                for (int i = 0; i < searchResp.documentContainerList.Length; i++)
                {
                    docIds[i] = searchResp.documentContainerList[i].documentId;
                }

                if (docIds.Length == 0)
                {
                    return results;
                }
                //Create the request and response objects
                GetDocumentsByDocumentId getByDocIdReq = new GetDocumentsByDocumentId();
                RetrievalResponse getByDocIdResp = null;

                // Populate the request with information
                getByDocIdReq.binarySecurityToken = wskSecurityToken;

                //Add doc Ids to be retrieved
                getByDocIdReq.documentIdList = docIds;

                RetrievalOptions1 byDocIdRo = new RetrievalOptions1();
                byDocIdRo.documentMarkup = DocumentMarkup.Display;
                byDocIdRo.documentMarkupSpecified = true;
                byDocIdRo.documentView = DocumentView.FullTextWithTerms;
                byDocIdRo.documentViewSpecified = true;
                getByDocIdReq.retrievalOptions = byDocIdRo;


                //Create the binding
                retrieveBind = new RetrievalSoapBinding();

                //Adjust the endpoint URL to point to the correct environment
                retrieveBind.Url = fixEndPointURL(retrieveBind.Url, wskEndPoint);

                //Make the Web Service call
                getByDocIdResp = retrieveBind.GetDocumentsByDocumentId(getByDocIdReq);

                //Use the response object

                DocumentContainer[] docContainers = getByDocIdResp.documentContainerList;

                if (docContainers != null)
                {
                    for (int i = 0; i < docContainers.Length; i++)
                    {
                        string field = "";
                        SearchResult record = new SearchResult();
                        DocumentContainer container = docContainers[i];

                        record.resultHTML = Encoding.Default.GetString(container.document).Replace(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""xhtml11-flat.dtd"">", "");
                        record.resultHTML = ASCIIEncoding.ASCII.GetString(container.document).Replace(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""xhtml11-flat.dtd"">", "");
                        String sDoc = Encoding.UTF8.GetString(container.document).Replace(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""xhtml11-flat.dtd"">", "");

                        XmlDocument currDoc = new XmlDocument();
                        currDoc.LoadXml(sDoc);
                        XmlNode body = currDoc.GetElementsByTagName("body")[0];

                        try { field = body.SelectNodes("//div[@class='HEADLINE']")[0].InnerText.ToString(); }
                        catch { field = "Not Found"; }
                        record.resultHeadline = field;

                        try { field = body.SelectNodes("//div[@class='PUB']")[0].InnerText.ToString(); }
                        catch { field = "Not Found"; }
                        record.resultPublisher = field;

                        try { field = body.SelectNodes("//div[@class='PUB-DATE']")[0].InnerText.ToString(); }
                        catch { field = "Not Found"; }
                        record.resultPublishDate = field;

                        try { field = body.SelectNodes("//div[@class='LENGTH']")[0].InnerText.ToString(); }
                        catch { field = "Not Found"; }
                        record.resultLength = field;

                        try { field = body.SelectNodes("//div[@class='LINK']//span")[0].InnerText.ToString(); }
                        catch { field = "Not Found"; }
                        record.resultLink = field;

                        resultList.Add(record);
                    }

                    // reverse the results set and set date range message
                    resultList.Reverse();
                    results = new Tuple<List<SearchResult>, string>(resultList, string.Format(" in the date range from {0} to {1}", fromDate.ToString("d"), toDate.ToString("d")) );

                }
            }
            catch (WebException we)
            {
                // Possibly an HTTP Error?
                throw we;

            }
            catch (SoapException se)
            {

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(se.Detail.OwnerDocument.NameTable);
                nsmgr.AddNamespace("p", "https://services.v1.wsapi.lexisnexis.com");
                XmlNode nd = se.Detail.SelectSingleNode("//p:errorCode", nsmgr);
                if (nd != null)
                {
                    throw new Exception(nd.InnerText);
                }
                else
                {
                    throw se;
                }

            }
            catch (Exception exc)
            {
                throw exc;

            }
            finally
            {
                retrieveBind = null;
                searchReq = null;
                searchBind = null;
            }
            return results;
        }

        
        #endregion

        #region Helper Function

        /// <summary>
        /// Sets the correct environment in the endpoint URL
        /// </summary>
        /// <param name="url">Initial URL</param>
        /// <param name="env">Environment</param>
        /// <returns>Corrected URL</returns>
        private string fixEndPointURL(string url, string env)
        {
            // Strip the existing server name from the URL - remove all characters between
            // the "//" and the next "/"
            int startChr = url.IndexOf("//") + 2;
            int endChr = url.IndexOf("/", startChr);
            url = url.Remove(startChr, endChr - startChr);
            // Insert the correct server name.
            url = url.Insert(startChr, env);

            return url;
        }

        /// <summary>
        /// Determines if there is still an active web service connection with Lexis Nexis based on the security token
        /// and the expiration date.
        /// </summary>
        /// <returns>True/False depending on if the session is active or not</returns>
        private bool isActiveSession()
        {
            // Do we want to move this check to the submit button and force users back to the login screen on time
            // out instead of just re-using the same credentials automatically?
            if (wskSecurityToken == null || wskSecurityToken.Equals(string.Empty)) return false;
            if (wskExpirationTime <= DateTime.Now) return false;
            return true;
        }
      
        /// <summary>
        /// Gets the secured source ID from the web service for a given source ID
        /// </summary>
        /// <param name="sourceID">Source ID</param>
        /// <returns>Secured Source ID</returns>
        private Tuple<string,string> getSourceDetails(string[] sourceID)
        {
            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticate();
            }
            foreach (string source in sourceID)
            {
                //	Create the request and response objects
                GetSourceDetails1 getSourceDetailsReq = new GetSourceDetails1();
                GetSourceDetailsResponse1 getSourceDetailsResp = null;

                // Populate the request with information
                getSourceDetailsReq.binarySecurityToken = wskSecurityToken;
                //getSourceDetailsReq.setIncludeSourceElement(Boolean.TRUE);
                getSourceDetailsReq.sourceId = source;

                SourceSoapBinding sourceBind = null;


                try
                {
                    // Change the endpoint according to your WSK environment
                    sourceBind = new SourceSoapBinding();
                    sourceBind.Url = fixEndPointURL(sourceBind.Url, wskEndPoint);

                    // Make the Web Service call
                    getSourceDetailsResp = sourceBind.GetSourceDetails(getSourceDetailsReq);

                    if (getSourceDetailsResp.securedSource != null)
                    {
                        return new Tuple<string,string> (source, getSourceDetailsResp.securedSource.securedSourceId);
                    }

                    //byte[][] guide = getSourceDetailsResp.sourceGuideList;
                    //Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(guide[0]));

                }
                catch (WebException we)
                {
                    // Possibly an HTTP Error?
                    throw we;

                }
                catch (SoapException se)
                {
                    // if it's an invalid request we are going to ignore it since source details are not required
                    if (se.Message.Contains("INVALID") || se.Message.Contains("Invalid"))
                    {
                        return new Tuple<string, string>(null, null); 
                    }
                    // WSK has returned a Fault message (i.e. an error has occurred)
                    // Extract the WSK-specific error code

                    // Add a mapping of the XML prefix "p:" to the XML Namespace of WSK
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(se.Detail.OwnerDocument.NameTable);
                    nsmgr.AddNamespace("p", "http://services.v1.wsapi.lexisnexis.com");

                    // Search the Fault message's detail for the  element
                    XmlNode nd = se.Detail.SelectSingleNode("//p:errorCode", nsmgr);
                    if (nd != null)
                    {
                        throw new Exception(nd.InnerText);
                    }
                    else
                    {
                        throw se;
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    sourceBind = null;
                }
            }
            return new Tuple<string,string>(null,null);
        }
        #endregion
    }
}