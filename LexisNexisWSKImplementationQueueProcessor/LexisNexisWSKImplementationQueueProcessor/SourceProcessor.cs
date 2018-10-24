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
using System.Text;
using LexisNexisWSKImplementationQueueProcessor.LNWebReference;
using System.Configuration;
using System.Web.Services.Protocols;
using System.Net;
using System.Xml;

namespace LexisNexisWSKImplementationQueueProcessor
{
    class SourceProcessor
    {
        #region Fields
        private List<Source> sourceList;

        private string securityToken;
        private DateTime expirationTime;

        private int errorCode;
        private string errorLocation;

        private int UI_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("UI Error").AppLookupCd; }
        }
        private int WS_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("Web Service Error").AppLookupCd; }
        }
        private int DB_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("Database Error").AppLookupCd; }
        }
        private int WS_TRACE_CODE
        {
            get { return AppLookups.getLookupByDescription("Web Service Call").AppLookupCd; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor for the class, currently does nothing
        /// </summary>
        public SourceProcessor()
        {

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates all of the available search sources by querying them in the web service then updating them in the database
        /// </summary>
        /// <returns>Tuple with the error code and message if any apply</returns>
        public Tuple<int,string> updateSources()
        {
            Tuple<int, string> results = new Tuple<int, string>(0, "");
            errorLocation = "";
            errorCode = 0;

            try
            {
                // Test the DB Connection
                errorLocation = "testing the connection to the application's database";
                errorCode = DB_ERROR_CODE;

                if (!DBManager.Instance.testConnection())
                {
                    return new Tuple<int, string>(errorCode, "Unable to connect to the application's database.");
                }



                // Authenticate with the web service
                errorLocation = "authenticating with the web service";
                errorCode = WS_ERROR_CODE;

                authenticateWebService(); // this will throw an exception if any errors occur

                // Get the updated search sources
                errorLocation = "retrieving the available search sources";
                errorCode = WS_ERROR_CODE;

                getSources();

                // Update the sources in the database
                DBManager.Instance.setSearchSources(sourceList);
                


            }
            catch (Exception ex)
            {
                results = new Tuple<int, string>(errorCode, string.Format("An error occurred while {0}. Exception Type: {1}. Error: {2}. Stack Trace: {3}", errorLocation, ex.GetType().ToString(), ex.Message, ex.StackTrace));
            }
            finally
            {

            }

            return results;
        }
       
        /// <summary>
        /// Gets the details for a given source ID, if it is a secured source, it will return the secured source ID,
        /// otherwise it will return null
        /// </summary>
        /// <param name="sourceID">Source ID</param>
        /// <returns>Secured Source ID or NULL</returns>
        public string getSourceDetails(string sourceID)
        {
            bool traceEnabled = Convert.ToInt32(AppParams.getParameterByName("WS_TRACE_LOGGING").AppParamValue) == 1 ? true : false;

            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticateWebService();
            }

            //	Create the request and response objects
            GetSourceDetails1 getSourceDetailsReq = new GetSourceDetails1();
            GetSourceDetailsResponse1 getSourceDetailsResp = null;

            // Populate the request with information
            getSourceDetailsReq.binarySecurityToken = securityToken;
            //getSourceDetailsReq.setIncludeSourceElement(Boolean.TRUE);
            getSourceDetailsReq.sourceId = sourceID;

            SourceSoapBinding sourceBind = null;

            try
            {
                // Change the endpoint according to your WSK environment
                sourceBind = new SourceSoapBinding();
                sourceBind.Url = fixEndPointURL(sourceBind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                // Make the Web Service call
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Calling: `GetSourceDetails`. Source ID: {0}. ", sourceID), WS_TRACE_CODE, "SYSTEM");
                }
                getSourceDetailsResp = sourceBind.GetSourceDetails(getSourceDetailsReq);

                if (getSourceDetailsResp.securedSource == null)
                {
                    return null;
                }
                else
                {
                    return getSourceDetailsResp.securedSource.securedSourceId;
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
                    return null;
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
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Authenticates against the Lexis Nexis web service, any errors that occur
        /// will be thrown to the caller function.
        /// </summary>
        private void authenticateWebService()
        {
            if (!isActiveSession())
            {
                //************************ Authenticate ************************
                bool traceEnabled = Convert.ToInt32(AppParams.getParameterByName("WS_TRACE_LOGGING").AppParamValue) == 1 ? true : false;

                // Create the request and response objects
                Authenticate authReq = new Authenticate();
                AuthenticateResponse authResp = null;

                // Populate the request with information
                authReq.authId = ConfigurationManager.AppSettings["wskID"];
                authReq.password = ConfigurationManager.AppSettings["wskPassword"];



                AuthenticationSoapBinding bind = null;
                try
                {
                    // Create the client for the web service, .NET 3.0+ WCF Service
                    bind = new AuthenticationSoapBinding();

                    // Adjust the endpoint URL to point to the correct environment
                    bind.Url = fixEndPointURL(bind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                    // Make the Web Service call
                    if (traceEnabled)
                    {
                        DBManager.Instance.logError(string.Format("Calling: `Authenticate`. URL: {0}.", bind.Url), WS_TRACE_CODE, "SYSTEM");
                    }
                    authResp = bind.Authenticate(authReq);

                    // Use the response object to check if it was populated
                    if (authResp.binarySecurityToken == null)
                    {
                        throw new Exception("No data was in the response object from the web service when some was expected.");
                    }
                    securityToken = authResp.binarySecurityToken.ToString();
                    expirationTime = authResp.expiration;

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
                    // General Exception.
                    throw exc;
                }
                finally
                {
                    bind = null;

                }

                return;
            }
        }

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
            if (securityToken == null || securityToken.Equals(string.Empty)) return false;
            if (expirationTime <= DateTime.Now) return false;
            return true;
        }

        /// <summary>
        /// Gets all of the sources for the given folder and folder name, will recursivly search
        /// the folders for all sources under them
        /// </summary>
        /// <param name="folderID">ID of the folder to search within</param>
        /// <param name="folderName">Name of the folder to search within, only used for display purposes</param>
        private void getSourcesFromFolder(string folderID, string folderName)
        {
            bool traceEnabled = Convert.ToInt32(AppParams.getParameterByName("WS_TRACE_LOGGING").AppParamValue) == 1 ? true : false;

            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticateWebService();
            }

            //	Create the request and response objects
            BrowseSources browseReq = new BrowseSources();
            BrowseSourcesResponse browseResp = null;

            // Populate the request with information
            browseReq.binarySecurityToken = securityToken;
            browseReq.locale = Locale.enUS;
            if (!folderID.Trim().Equals(string.Empty))
            {
                browseReq.folderId = folderID;
            }
            SourceSoapBinding sourceBind = null;

            try
            {
                // Change the endpoint according to your WSK environment
                sourceBind = new SourceSoapBinding();

                sourceBind.Url = fixEndPointURL(sourceBind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                // Make the Web Service call
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Calling: `BrowseSources`. Folder ID: {0}. Folder Name: {1}", folderID, folderName), WS_TRACE_CODE, "SYSTEM");
                }
                browseResp = sourceBind.BrowseSources(browseReq);



                // Add the sources to our source list
                if (browseResp.sourceList != null)
                {
                    Source2[] sources = browseResp.sourceList;
                    for (int i = 0; i < sources.Length; i++)
                    {
                        // Note: commenting out since we want to represent each source in their folder and may appear in multiples
                        // check if source is a duplicate
                        //Source src = sourceList.FirstOrDefault(x => x.sourceID == sources[i].sourceId);
                        // no duplicate record found
                        //if (sourceList.FirstOrDefault(x => x.sourceID == sources[i].sourceId) == null)
                        //{
                        //    sourceList.Add(new Source(sources[i].name, sources[i].sourceId, folderName)); 
                        //}
                        //// duplicate record found (for now pulling old record details, but maybe we need to not add these at all)
                        //else
                        //{
                            sourceList.Add(new Source(sources[i].name, sources[i].sourceId, folderName));
                        //}

                    }
                }

                // go through the remaining source folders
                if (browseResp.folderList != null)
                {
                    string folder = folderName;
                    if (folder != "") folder = folder + "\\";
                    Folder1[] folders = browseResp.folderList;

                    for (int i = 0; i < folders.Length; i++)
                    {
                        this.getSourcesFromFolder(folders[i].folderId, folder + folders[i].name);
                    }
                }
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
                browseReq = null;
                sourceBind = null;
            }
        }

        /// <summary>
        /// Attempts to retrieve all of the available sources that can be searched against from the web service.
        /// If any errors occur, they will throw an exception.
        /// </summary>
        /// <returns>List of the source objects (which contains the name and ID of the source)</returns>
        private void getSources()
        {
            sourceList = new List<Source>();

            try
            {
                getSourcesFromFolder("", ""); // this takes about 2 minutes to complete
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion
    }
}
