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

using LexisNexisWSKImplementationQueueProcessor.LNWebReference;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Services.Protocols;
using System.Net;
using System.Xml;
using System.Configuration;
using System.Text;
using System.IO;
using Ionic.Zip;

using Mono.Unix.Native;


namespace LexisNexisWSKImplementationQueueProcessor
{
    /// <summary>
    /// Processes the search queue for the hour
    /// </summary>
     class QueueProcessor
    {
        #region Fields
        private string securityToken;
        private DateTime expirationTime;

        private int errorCode;
        private string errorLocation;

        private List<SearchRequest> searchQueue;

        private int UI_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("UI Error").AppLookupCd; }
        }
        private int WS_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("Web Service Error").AppLookupCd; }
        }
        private int WS_TRACE_CODE
        {
            get { return AppLookups.getLookupByDescription("Web Service Call").AppLookupCd; }
        }
        private int UI_TRACE_CODE
        {
            get { return AppLookups.getLookupByDescription("UI Call").AppLookupCd; }
        }
        private int DB_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("Database Error").AppLookupCd; }
        }
        private bool traceEnabled;

        private SearchRequest request;

         // fields needed for recussive search function
         private int endIndex;
         private int docPerReq;
         private int srchPerReq;
        #endregion


        #region Constructor
         /// <summary>
         /// Constructor for the class, currently does nothing
         /// </summary>
        public QueueProcessor()
        {

        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Processes the search queue and returns any errors that occur
        /// </summary>
        /// <returns>Error Code (0 for no failures) and Error Message (empty string for no failures)</returns>
        public Tuple<int,string> processQueue()
        {
            Tuple<int, string> results = new Tuple<int, string>(0,"");
            errorLocation = "";
            errorCode = 0;

            traceEnabled = Convert.ToInt32(AppParams.getParameterByName("WS_TRACE_LOGGING").AppParamValue) == 1 ? true : false;

            try
            {
                // Test the DB Connection
                errorLocation = "testing the connection to the application's database";
                errorCode = DB_ERROR_CODE;

                if (!DBManager.Instance.testConnection())
                {
                    return new Tuple<int, string>(errorCode, "Unable to connect to the application's database.");
                }

                // Check the application's run status
                errorLocation = "verifying the application's run status in the database";
                if (isAppRunning())
                {
                    // log that the application is already running then return
                    Logger.Instance.logMessage("Another instance of the application is already running.");
                    return results;
                }

                // Set the run status to running
                errorLocation = "updating the run status";
                setAppRunStatus(true);

                // Ensure we are within the time window allowed for processing
                errorLocation = "verifying the processing window";
                errorCode = UI_ERROR_CODE;

                if (!verifyProcessingWindow())
                {
                    Logger.Instance.logMessage("This process is not within the valid processing window allowed.");
                    return results;
                }

                // Ensure we have remaining searches allowed this hour
                errorLocation = "ensuring there are remaining searches for the current hour";

                if (DBManager.Instance.getRemainingSearches().Item1 < 1)
                {
                    Logger.Instance.logMessage("There are no remaining searches for this hour.");
                    return results;
                }

                // Get the current search queue 
                errorLocation = "retrieving the current search queue";
                errorCode = DB_ERROR_CODE;

                searchQueue = DBManager.Instance.getSearchQueue(); // this will throw an exception if anny errors occur

                errorCode = UI_ERROR_CODE;
                if (searchQueue != null)
                {
                    if (searchQueue.Count == 0)
                    {
                        Logger.Instance.logMessage("There are no searches in the queue to process.");
                        return results;
                    }
                }

                // Authenticate with the web service
                errorLocation = "authenticating with the web service";
                errorCode = WS_ERROR_CODE;

                authenticateWebService(); // this will throw an exception if any errors occur           

                // Loop through the search queue until the remaining searches is 0
                //// For each search item: run the search, save the results, update the database with the stats, incremened 
                //// and update the number of searches performed
                errorLocation = "performing the searches from the queue";

                foreach (SearchRequest search in searchQueue)
                {
                    request = search;
                    bool changed = false;
                    if (DBManager.Instance.getRemainingSearches().Item1 > 0)
                    {
                        // verify the search is still active before starting processing
                        if (DBManager.Instance.getSearchQueue().Any(o => o.searchFullName == request.searchFullName))
                        {
                            // check the date parameters to see if no date range was selected and set the range manually
                            if (request.currEndDate == null)
                            {
                                request.currEndDate = DateTime.Today;
                                changed = true;
                            }
                            if (request.currStartDate == null)
                            {
                                // try to pull the begining date from the APPL_PARAM table, if not able to use 1/1/1800 instead
                                DateTime begDt = new DateTime(1800, 1, 1);
                                try
                                {
                                    string dtParam = AppParams.getParameterByName("BEGIN_DT").AppParamValue;
                                    string[] dtArray = dtParam.Split('/');
                                    begDt = new DateTime(Convert.ToInt32(dtArray[2]), Convert.ToInt32(dtArray[1]), Convert.ToInt32(dtArray[0]));
                                }
                                catch { }
                                request.currStartDate = begDt;
                                changed = true;
                            }
                            if (!string.IsNullOrEmpty(request.searchLNID))
                            {
                                // clear out the previous saved search ID since it will have expired from the previous run
                                request.searchLNID = null;
                                changed = true;
                            }

                            if (changed)
                            {
                                DBManager.Instance.updateSearch(request);
                            }


                            // process the search
                            try
                            {
                                processSearch();

                                // if search is complete, check if there are 0 results and if so update it in the DB
                                if (request.searchNumberResults == 0 && request.searchPercentComplete == 1)
                                {
                                    request.searchResultLocation = "";
                                    request.searchNumberResults = 0;
                                    request.searchPercentComplete = 1m;
                                    request.searchStatus = AppLookups.getLookupByDescription("Complete").AppLookupCd;
                                    request.emailed = true;

                                    // send email that the search completed with 0 results
                                    string body = string.Format(@"Your queued search completed with 0 results. Please log on to https://lexnex.lib.msu.edu to refine your search and queue it again.	                                                                    

For assistance with refining your search contact Hui Hua Chua (http://staff.lib.msu.edu/chua/)
                                                                        
Search Name: {0}", request.searchName);
                                    sendEmail(request.searchUser + "@msu.edu", "Text Assembler: Search Complete", body);
                                    DBManager.Instance.updateSearch(request);
                                }
                            }
                            catch (Exception e)
                            {
                                if (e.Message != "STOP PROCESSING") throw e; // we want to ignore the exception that was just thrown to stop the processing of that request

				// if search is complete, check if there are 0 results and if so update it in the DB                          
                                else if (e.Message == "STOP PROCESSING" && request.searchNumberResults == 0 && request.searchPercentComplete == 1)
                                {
                                    request.searchResultLocation = "";
                                    request.searchNumberResults = 0;
                                    request.searchPercentComplete = 1m;
                                    request.searchStatus = AppLookups.getLookupByDescription("Complete").AppLookupCd;
                                    request.emailed = true;                                       

                                    // send email that the search completed with 0 results
                                    string body = string.Format(@"Your queued search completed with 0 results. Please log on to https://lexnex.lib.msu.edu to refine your search and queue it again.                                                                    

For assistance with refining your search contact Hui Hua Chua (http://staff.lib.msu.edu/chua/)                                                                    

Search Name: {0}", request.searchName);
                                    sendEmail(request.searchUser + "@msu.edu", "Text Assembler: Search Complete", body);
                                    DBManager.Instance.updateSearch(request);
                                }
                            }
                        }
                    }
                }

              
            }
            catch (Exception ex)
            {
                results = new Tuple<int, string>(errorCode, string.Format("An error occurred while {0}. Error: {1}. Stack Trace: {2}", errorLocation, ex.Message, ex.StackTrace));
            }
            finally
            {
                // Set the run status to not running
                setAppRunStatus(false);
            }

            return results;
        }

        #endregion

        #region Private Functions

         /// <summary>
         /// Recursivly breaks down the date range of the search to avoid the SEARCH_TOO_GENERAL error
         /// </summary>
        private void processSearch()
        {
            string errMsg = "";

            // check that there are searches remaining
            if (DBManager.Instance.getRemainingSearches().Item1 < 1 || !verifyProcessingWindow())
            {
                throw new Exception("STOP PROCESSING");
            }

            if (traceEnabled)
            {
                DBManager.Instance.logError(string.Format("Starting processing of search: '{0}'.", request.searchFullName), UI_TRACE_CODE, "SYSTEM");
            }

            // finish the in progress search before continuing. No errors are expected since the rest of the search processed sucessfully
            // only potential error should be operation timeout
            if (request.searchStartIndex != 1 && request.searchStartIndex != 0)
            {
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Search: '{0}': resuming in progress search. Start index={1}", request.searchFullName, request.searchStartIndex), UI_TRACE_CODE, "SYSTEM");
                }

                errMsg = processRange(); // no changes need to be made to currDate since this search is in progress
                if (errMsg != "") // handle errors that could occur mid-process
                {
                    if (errMsg.Contains("timed out"))
                    {
                        Logger.Instance.logMessage(string.Format("The search '{0}' timed out, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                        DBManager.Instance.logError(string.Format("The search '{0}' timed out, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");

                        // skip the rest of the processing for this search by throwing an exception
                        throw new Exception("STOP PROCESSING");
                    }
                    else if (errMsg.Contains("EXPIRED_ANSWERSET"))
                    {
                        Logger.Instance.logMessage(string.Format("The search '{0}' had an expired answerset, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                        DBManager.Instance.logError(string.Format("The search '{0}' had an expired answerset, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");

                        request.searchLNID = null;
                        DBManager.Instance.updateSearch(request);

                        // skip the rest of the processing for this search by throwing an exception
                        throw new Exception("STOP PROCESSING");
                    }
                    else if (errMsg.Contains("EXPIRED_SECURITY_TOKEN"))
                    {
                        // only log the expiration if it was really past the expiration time provided by the security token
                        if (DateTime.Now >= expirationTime)
                        {
                            Logger.Instance.logMessage(string.Format("The search '{0}' had an expired security token, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                            DBManager.Instance.logError(string.Format("The search '{0}', had an expired secirity token, will stop and attempt next processing cycle. Error: {1}. Security Token: {2}. Exp Date: {3}", request.searchFullName, errMsg, securityToken, expirationTime.ToString("F")), WS_ERROR_CODE, "SYSTEM");
                        }
                        DBManager.Instance.updateSearch(request);

                        // skip the rest of the processing for this search by throwing an exception
                        throw new Exception("STOP PROCESSING");
                    }
                    else 
                    {
                        int max_retry = 3;
                        try
                        {
                            max_retry = Convert.ToInt32(AppParams.getParameterByName("MAX_RETRY"));
                        }
                        catch { } // ignore errrors and use default value
                        if (request.retryCount > max_retry)
                        {
                            // log that the search request was invalid and update the search record
                            request.searchStatus = AppLookups.getLookupByDescription("Invalid").AppLookupCd;
                            request.searchPercentComplete = 0;
                            request.errorMsg = errMsg;
                            DBManager.Instance.updateSearch(request);

                            Logger.Instance.logMessage(string.Format("Error processing '{0}', invalid search. Error: {1}", request.searchFullName, errMsg));
                            DBManager.Instance.logError(string.Format("Error processing '{0}', invalid search. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");
                        }
                        else
                        {
                            request.retryCount++;
                            DBManager.Instance.updateSearch(request);
                            Logger.Instance.logMessage(string.Format("Error processing '{0}', invalid search. Under retry limit so will queue again. Error: {1}", request.searchFullName, errMsg));
                            DBManager.Instance.logError(string.Format("Error processing '{0}', invalid search. Under retry limit so will queue again. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");
                        }
                        // skip the rest of the processing for this search by throwing an exception
                        throw new Exception("STOP PROCESSING");
                    }
                }

                // check that there are searches remaining
                else if (DBManager.Instance.getRemainingSearches().Item1 < 1 || !verifyProcessingWindow())
                {
                    throw new Exception("STOP PROCESSING");
                }
                else if (request.retryCount > 0)// latest search was sucessful so reset the retry count
                {
                    request.retryCount = 0;
                    DBManager.Instance.updateSearch(request);
                }

                // set up the date range for the next range run now that the in progress one is complete
                request.currStartDate = ((DateTime)request.currEndDate).AddDays(1);
                if (request.searchEndDate == null)
                {
                    request.currEndDate = DateTime.Today;
                }
                else
                {
                    request.currEndDate = request.searchEndDate;
                }
                request.searchStartIndex = 1;
                request.searchLNID = "";

                // check if the search was complete
                if (request.searchStatus == AppLookups.getLookupByDescription("Complete").AppLookupCd)
                {
                    throw new Exception("STOP PROCESSING");
                }
            }

            if (traceEnabled)
            {
                DBManager.Instance.logError(string.Format("Search: '{0}': starting search for new range.", request.searchFullName), UI_TRACE_CODE, "SYSTEM");
            }

            errMsg = processRange(); // process the range, could throw errors

            if (errMsg != "") // handle errors that could occur mid-process
            {
                if (errMsg.Contains("timed out"))
                {
                    Logger.Instance.logMessage(string.Format("The search '{0}' timed out, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                    DBManager.Instance.logError(string.Format("The search '{0}' timed out, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");

                    // skip the rest of the processing for this search by throwing an exception
                    throw new Exception("STOP PROCESSING");
                }
                else if (errMsg.Contains("EXPIRED_ANSWERSET"))
                {
                    Logger.Instance.logMessage(string.Format("The search '{0}' had an expired answerset, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                    DBManager.Instance.logError(string.Format("The search '{0}' had an expired answerset, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");

                    request.searchLNID = null;
                    DBManager.Instance.updateSearch(request);

                    // skip the rest of the processing for this search by throwing an exception
                    throw new Exception("STOP PROCESSING");
                }
                else if (errMsg.Contains("EXPIRED_SECURITY_TOKEN"))
                {
                    // only log the expiration if it was really past the expiration time provided by the security token
                    if (DateTime.Now >= expirationTime)
                    {
                        Logger.Instance.logMessage(string.Format("The search '{0}' had an expired security token, will stop and attempt next processing cycle. Error: {1}", request.searchFullName, errMsg));
                        DBManager.Instance.logError(string.Format("The search '{0}', had an expired secirity token, will stop and attempt next processing cycle. Error: {1}. Security Token: {2}. Exp Date: {3}", request.searchFullName, errMsg, securityToken, expirationTime.ToString("F")), WS_ERROR_CODE, "SYSTEM");
                    }
                    DBManager.Instance.updateSearch(request);

                    // skip the rest of the processing for this search by throwing an exception
                    throw new Exception("STOP PROCESSING");
                }
                else if (!errMsg.Contains("SEARCH_TOO_GENERAL"))
                {
                    // check if over the retry limit before logging it as invalid search
                    int max_retry = 3;
                    try
                    {
                        max_retry = Convert.ToInt32(AppParams.getParameterByName("MAX_RETRY"));
                    }
                    catch { } // ignore errrors and use default value
                    if (request.retryCount > max_retry)
                    {
                        // log that the search request was invalid and update the search record
                        request.searchStatus = AppLookups.getLookupByDescription("Invalid").AppLookupCd;
                        request.searchPercentComplete = 0;
                        request.errorMsg = errMsg;
                        request.emailed = true;                     

                        // send email that the search is invalid
                        string body = string.Format(@"Your queued search has failed due to being invalid. Please log on to https://lexnex.lib.msu.edu to refine your search and queue it again.
	                                
For assistance with refining your search contact Hui Hua Chua (http://staff.lib.msu.edu/chua/)
For technical assistance with the system contact Megan Schanz (schanzme@lib.msu.edu)

Search Name: {0}", request.searchName);
                        sendEmail(request.searchUser + "@msu.edu", "Text Assembler: Search Failed", body);
                        DBManager.Instance.updateSearch(request);

                        Logger.Instance.logMessage(string.Format("Error processing '{0}', invalid search. Error: {1}", request.searchFullName, errMsg));
                        DBManager.Instance.logError(string.Format("Error processing '{0}', invalid search. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");
                    }
                    else
                    {
                        request.retryCount++;
                        DBManager.Instance.updateSearch(request);
                        Logger.Instance.logMessage(string.Format("Error processing '{0}', invalid search. Under retry limit so will queue again. Error: {1}", request.searchFullName, errMsg));
                        DBManager.Instance.logError(string.Format("Error processing '{0}', invalid search. Under retry limit so will queue again. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");
                    }
                    // skip the rest of the processing for this search by throwing an exception
                    throw new Exception("STOP PROCESSING");
                }
                else if (errMsg.Contains("SEARCH_TOO_GENERAL") && ((DateTime)request.currEndDate).Subtract(((DateTime)request.currStartDate)).Days <= 1)
                {
                    // log exception because this search can not be split anymore and can not be processed
                    request.searchStatus = AppLookups.getLookupByDescription("Invalid").AppLookupCd;
                    request.searchPercentComplete = 0;
                    request.errorMsg = "Search too general.";
                   request.emailed = true;                

                    // send email that the search is invalid
                    string body = string.Format(@"
	                                Your queued search has failed due to being too general. Please log on to https://lexnex.lib.msu.edu to refine your search and queue it again.
	                                
For assistance with refining your search contact Hui Hua Chua (http://staff.lib.msu.edu/chua/)
                                    
Search Name: {0}", request.searchName);
                    sendEmail(request.searchUser + "@msu.edu", "Text Assembler: Search Failed", body);

                    DBManager.Instance.updateSearch(request);

                    Logger.Instance.logMessage(string.Format("Error processing '{0}', search was too general to process. Error: {1}", request.searchFullName, errMsg));
                    DBManager.Instance.logError(string.Format("Error processing '{0}', search was too general to process. Error: {1}", request.searchFullName, errMsg), WS_ERROR_CODE, "SYSTEM");

                    // skip the rest of the processing for this search by throwing an exception
                    throw new Exception("STOP PROCESSING");
                }
                else  // search is too general but can be split more
                {
                    DateTime middle = ((DateTime)request.currStartDate).AddDays(Convert.ToDouble(Math.Floor(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(((DateTime)request.currStartDate)).Days)) / Convert.ToDecimal(2)));
                    request.currEndDate = middle;
                    request.searchStartIndex = 1;
                    request.searchLNID = "";
                    DBManager.Instance.updateSearch(request);
                    processSearch(); // process with the currStart to middle

                    // check that there are searches remaining
                    if (DBManager.Instance.getRemainingSearches().Item1 < 1 || !verifyProcessingWindow())
                    {
                        throw new Exception("STOP PROCESSING");
                    }
                    request.currStartDate = middle.AddDays(1);
                    if (request.searchEndDate == null)
                    {
                        request.currEndDate = DateTime.Today;
                    }
                    else
                    {
                        request.currEndDate = request.searchEndDate;
                    }
                    request.searchStartIndex = 1;
                    request.searchLNID = "";
                    DBManager.Instance.updateSearch(request);
                    processSearch(); // process with middle to endDate

                }
            }
            else if  (DBManager.Instance.getRemainingSearches().Item1 < 1 || !verifyProcessingWindow())
            {
                throw new Exception("STOP PROCESSING");
            }
            else if (request.retryCount > 0) // latest search was sucessful so reset the retry count
            {
                request.retryCount = 0;
                DBManager.Instance.updateSearch(request);
            }

            return;
        }

        /// <summary>
        /// Processes the search for the specific date range in currStartDate and currEndDate and calls the web service and saves those results
        /// </summary>
        private string processRange()
        {
            // Get the number of searches we can perform per request and documents we can get at a time
            srchPerReq = Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_SRCH").AppParamValue);
            docPerReq = Convert.ToInt32(AppParams.getParameterByName("DOC_PR_SRCH").AppParamValue);
            if (request.searchStartIndex < 1) request.searchStartIndex = 1;
            string saveLocation = Path.Combine(Path.Combine(ConfigurationManager.AppSettings["saveLocation"], request.searchUser), request.searchName.Replace(' ', '_').Replace("/","_").Replace("\\","_"));

            // Only do the below actions if the search is actually complete
            if (request.searchPercentComplete >= 1 || (request.currEndDate <= request.currStartDate && request.searchPercentComplete != 0))
            {
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Search: '{0}': marking as complete.", request.searchFullName), UI_TRACE_CODE, "SYSTEM");
                }

                errorCode = UI_ERROR_CODE;

                // Add logo image to the folder
                errorLocation = string.Format("adding logo images to the documents folder (search: {0})", request.searchFullName);
                addLogoImageToFolder(saveLocation);

                // Reverse the order of the results
                /// THIS WAS CAUSING PROBLEMS ON THE SERVER SO I AM COMMENTING IT OUT UNTIL RESOLVED
                //errorLocation = string.Format("reversing the document results in the folders (search: {0})", request.searchFullName);
                //reverseDocNameOrder(saveLocation, request.searchFullName);

                // Determine the zip file name that will be created
                errorLocation = string.Format("Updating the target zip file for the search (search: {0})", request.searchFullName);
                String zipPath = Path.Combine(Path.Combine(ConfigurationManager.AppSettings["saveLocation"], request.searchUser), string.Format("{0}.zip", request.searchName.Replace(' ', '_')));
                request.searchResultLocation = zipPath;
                request.searchPercentComplete = 1m;
                DBManager.Instance.updateSearch(request);

                // Zip up all of the results for the search, only retaining the zip file (not the individual html files)              
                errorLocation = string.Format("zipping the documents on the server (search: {0})", request.searchFullName);
                zipPath = zipDocuments(saveLocation, request.searchName.Replace(' ', '_'), Path.Combine(ConfigurationManager.AppSettings["saveLocation"], request.searchUser));

                // Update the DB with the final status of the search with the location of the zip file and 100% complete status
                errorCode = DB_ERROR_CODE;
                errorLocation = string.Format("updating the search status in the database (search: {0})", request.searchFullName);
                request.searchResultLocation = zipPath;
                request.searchPercentComplete = 1m;
                request.searchNumberResults = Directory.GetFiles(Path.Combine(saveLocation, "txt"), "*", SearchOption.TopDirectoryOnly).Length;
                request.searchStatus = AppLookups.getLookupByDescription("Complete").AppLookupCd;
                request.searchQueuePosition = null;
                DBManager.Instance.updateSearch(request);

                return "";
            }

            // this search is complete for the curr range
            if (request.searchStartIndex > request.numResultsInRange && request.numResultsInRange != 0) 
            {
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Search: '{0}': complete for the current range. Start index={1}. Results in range={2}", request.searchFullName, request.searchStartIndex, request.numResultsInRange), UI_TRACE_CODE, "SYSTEM");
                }

                if (request.searchEndDate == null || request.searchStartDate == null)
                {
                    DateTime begDt = new DateTime(1800, 1, 1);
                    try
                    {
                        string dtParam = AppParams.getParameterByName("BEGIN_DT").AppParamValue;
                        string[] dtArray = dtParam.Split('/');
                        begDt = new DateTime(Convert.ToInt32(dtArray[2]), Convert.ToInt32(dtArray[1]), Convert.ToInt32(dtArray[0]));
                    }
                    catch { }
                    request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(begDt).Days + 1) / Convert.ToDecimal((DateTime.Today).Subtract(begDt).Days + 1), 2);
                }
                else
                {
                    request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1) / Convert.ToDecimal(((DateTime)request.searchEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1), 2);
                }
                if (request.searchPercentComplete > 1m) request.searchPercentComplete = 1m; // this scenario happens when there are less than 100 results or when the number of results changes during the search
                request.searchNumberResults += request.numResultsInRange;
                request.searchStartIndex = 1;
                DBManager.Instance.updateSearch(request);
            }

            int fileNum = request.searchNumberResults + request.searchStartIndex;
            endIndex = 0;
            if (endIndex == 0 || endIndex > (request.searchStartIndex + srchPerReq - 1)) endIndex = request.searchStartIndex + srchPerReq - 1;
            List<string> searchResult = null;


            if (traceEnabled)
            {
                DBManager.Instance.logError(string.Format("Search: '{0}': checking if search is complete. Start index={1}. End index={2}. Percent Complete={3}. Current Start Date={4}. Current End Date={5}.",
                    request.searchFullName, request.searchStartIndex, endIndex, request.searchPercentComplete, ((DateTime)request.currStartDate).ToString("MM/dd/yyyy"), 
                    ((DateTime)request.currEndDate).ToString("MM/dd/yyyy")), UI_TRACE_CODE, "SYSTEM");
            }

            // if the search is in progress for the current range
            if (request.searchStartIndex <= endIndex && request.currEndDate >= request.currStartDate && request.searchPercentComplete != 1)
            {
                // Perform the first search with a range of 1 - max results per request
                errorCode = WS_ERROR_CODE;
                errorLocation = string.Format("retrieving documents from the web service (search: {0})", request.searchFullName);
                if (traceEnabled)
                {
                    DBManager.Instance.logError(errorLocation, UI_TRACE_CODE, "SYSTEM");
                }
                try
                {
                    searchResult = getDocuments(request.searchStartIndex, endIndex, docPerReq);
                }
                // catch any exception caused by the web service code and throw back to the driver function
                catch (Exception e)
                {
                    return e.Message;
                }
                //// Handle the scenario when there are no results returned
                if (request.numResultsInRange == 0 || searchResult.Count == 0)
                {
                    if (request.searchEndDate == null || request.searchStartDate == null)
                    {
                        DateTime begDt = new DateTime(1800, 1, 1);
                        try
                        {
                            string dtParam = AppParams.getParameterByName("BEGIN_DT").AppParamValue;
                            string[] dtArray = dtParam.Split('/');
                            begDt = new DateTime(Convert.ToInt32(dtArray[2]), Convert.ToInt32(dtArray[1]), Convert.ToInt32(dtArray[0]));
                        }
                        catch { }
                        request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(begDt).Days + 1) / Convert.ToDecimal((DateTime.Today).Subtract(begDt).Days + 1), 2);
                    }
                    else
                    {
                        request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1) / Convert.ToDecimal(((DateTime)request.searchEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1), 2);
                    }
                    request.searchStartIndex = 1;
                    if (request.searchPercentComplete > 1m) request.searchPercentComplete = 1m; // this scenario happens when there are less than 100 results or when the number of results changes during the search
                    DBManager.Instance.updateSearch(request);
                }
                else
                {

                    //// Save the document text to html files saved to the server
                    //// File location: <root save location>\<requesting user>\<search name>
                    errorCode = UI_ERROR_CODE;
                    errorLocation = string.Format("saving documents to the server (search: {0})", request.searchFullName);
                    if (traceEnabled)
                    {
                        DBManager.Instance.logError(errorLocation, UI_TRACE_CODE, "SYSTEM");
                    }
                    fileNum = saveDocuments(searchResult, saveLocation, fileNum);

                    //// Update the DB with the % complete each search request
                    errorCode = DB_ERROR_CODE;
                    errorLocation = string.Format("updating the search status in the database (search: {0})", request.searchFullName);
                    request.searchStatus = AppLookups.getLookupByDescription("Processing").AppLookupCd;
                    request.searchStartIndex = endIndex + 1;
                    DBManager.Instance.updateSearch(request);

                    //// Update the DB incrementing the # of searches performed
                    errorCode = DB_ERROR_CODE;
                    errorLocation = string.Format("incrementing the number of searches used in the databse (search: {0})", request.searchFullName);
                    DBManager.Instance.incrementSearchesPerHour();


                    // Verify there are remaining searches available this hour
                    // Perform the rest of the searches in groups the max results per request
                    // The final index will be determined based on the total # of documents
                    while (DBManager.Instance.getRemainingSearches().Item1 > 0 && verifyProcessingWindow())
                    {
                        request.searchStartIndex = endIndex + 1;
                        endIndex += srchPerReq;
                        if (request.numResultsInRange != 0 && request.searchStartIndex > request.numResultsInRange) // this search is complete for the curr range
                        {
                            if (request.searchEndDate == null || request.searchStartDate == null)
                            {
                                DateTime begDt = new DateTime(1800, 1, 1);
                                try
                                {
                                    string dtParam = AppParams.getParameterByName("BEGIN_DT").AppParamValue;
                                    string[] dtArray = dtParam.Split('/');
                                    begDt = new DateTime(Convert.ToInt32(dtArray[2]), Convert.ToInt32(dtArray[1]), Convert.ToInt32(dtArray[0]));
                                }
                                catch { }
                                request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(begDt).Days + 1) / Convert.ToDecimal((DateTime.Today).Subtract(begDt).Days + 1), 2);
                            }
                            else
                            {
                                request.searchPercentComplete = Math.Round(Convert.ToDecimal(((DateTime)request.currEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1) / Convert.ToDecimal(((DateTime)request.searchEndDate).Subtract(((DateTime)request.searchStartDate)).Days + 1), 2);
                            }
                            if (request.searchPercentComplete > 1m) request.searchPercentComplete = 1m; // this scenario happens when there are less than 100 results or when the number of results changes during the search
                            request.searchNumberResults += request.numResultsInRange;
                            request.searchStartIndex = 1;
                            DBManager.Instance.updateSearch(request);
                            break;
                        }
                        if (endIndex >= request.numResultsInRange) endIndex = request.numResultsInRange;

                        //// Retrieve documents for the current group from the web service
                        errorCode = WS_ERROR_CODE;
                        errorLocation = string.Format("retrieving documents from the web service (search: {0})", request.searchFullName);
                        if (traceEnabled)
                        {
                            DBManager.Instance.logError(errorLocation, UI_TRACE_CODE, "SYSTEM");
                        }
                        try
                        {
                            searchResult = getDocuments(request.searchStartIndex, endIndex, docPerReq);
                        }
                        // catch any exception caused by the web service code and throw to the driver function
                        catch (Exception e)
                        {
                            return e.Message;
                        }

                        //// Save the document text to html files saved to the server
                        errorCode = UI_ERROR_CODE;
                        errorLocation = string.Format("saving documents to the server (search: {0})", request.searchFullName);
                        if (traceEnabled)
                        {
                            DBManager.Instance.logError(errorLocation, UI_TRACE_CODE, "SYSTEM");
                        }
                        fileNum = saveDocuments(searchResult, saveLocation, fileNum);

                        //// Update the DB with the % complete each search request
                        errorCode = DB_ERROR_CODE;
                        errorLocation = string.Format("updating the search status in the database (search: {0})", request.searchFullName);
                        request.searchStartIndex = endIndex + 1;
                        DBManager.Instance.updateSearch(request);

                        //// Update the DB incrementing the # of searches performed
                        errorCode = DB_ERROR_CODE;
                        errorLocation = string.Format("incrementing the number of searches used in the databse (search: {0})", request.searchFullName);
                        DBManager.Instance.incrementSearchesPerHour();
                    }
                }
            }
           
            return "";
        }

        /// <summary>
        /// Gets documents for the provided range based on the given search request.
        /// It will retrieve the results in groups of the number of documents allowed
        /// to be retrieved per request. It will retieve all of the documents in the 
        /// range of start to end index provided doing as many groups of document requests
        /// as needed.
        /// </summary>
        /// <param name="startIndex">Start index for the search</param>
        /// <param name="endIndex">End index for the search</param>
        /// <param name="docsPerSrch">Number of allowed documents to retrieve per request</param>
        /// <returns>List of strings: the html for the documents retrieved</returns>
        private  List<string> getDocuments(int startIndex, int endIndex, int docsPerSrch)
        {
            List<string> results = new List<string>();

            // Verify our web service connection is still active, and if not 
            // re-authenticate using the same credentials to get a new session
            if (!isActiveSession())
            {
                authenticateWebService();
            }

            // make sure we don't go over the search cap
            if (endIndex - startIndex >= Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_SRCH").AppParamValue))
            {
                endIndex = Convert.ToInt32(AppParams.getParameterByName("RSLT_PR_SRCH").AppParamValue);
            }

            // Parse the search method
            SearchMethod method;
            if (!Enum.TryParse<SearchMethod>(request.searchMethod, out method))
            {
                throw new Exception("Invalid Search: provided search method was invalid.");
            }

            // determine if the search for this range is already in progress, if so use the existing searchID, otherwise get a new one
            if (string.IsNullOrEmpty(request.searchLNID) || startIndex == 1) { 
                //Create the request and response objects
                Search searchReq = new Search();
                SearchResponse searchResp = new SearchResponse();

                //Populate the request with information
                searchReq.binarySecurityToken = securityToken;
                searchReq.locale = Locale.enUS;
                searchReq.projectId = AppParams.getParameterByName("PRJ_ID").AppParamValue;
                searchReq.query = request.searchQuery;
                searchReq.useCSP = false;

                // Set the search options
                SearchOptions so = new SearchOptions();
                so.searchMethod = method; //SearchMethod.MatchOnPhrase;
                so.sortOrder = SortOrder.Date;
                searchReq.searchOptions = so;

                // Set the date restriction if provided  
                DateRestriction dr = new DateRestriction();
                dr.endDateSpecified = true;
                dr.endDate = (DateTime)request.currEndDate;
                dr.startDateSpecified = true;
                dr.startDate = (DateTime)request.currStartDate;
                searchReq.searchOptions.dateRestriction = dr;


                // Set the source to search
                // Always use the smallest collection of data possible. For example, NEWS; 14DAYS (CSI:  272977) uses far fewer system resources than NEWS;CURNWS (CSI:  140560) or NEWS;ALLNWS (CSI:  8399). 
                SourceInformationChoice sic = new SourceInformationChoice();

                // Clean the search source input
                List<string> srcSources = new List<string>();
                foreach (var source in request.searchSource.Replace(" ", "").Split(','))
                {
                    if (!source.Equals(string.Empty))
                    {
                        srcSources.Add(source);
                    }
                }

                /// We have no secured sources available to us so we do not need to loop through all sources to verify that
                //Tuple<string, string> secureSourceDetails = getSourceDetails(srcSources.ToArray());
                //sic.securedSourceId = secureSourceDetails.Item2;

                //if (srcSources.ToArray().Count() > 1 && sic.securedSourceId != null)
                //{
                //    throw new Exception(string.Format("Invalid Search: One or more of the sources (including {0}) you selected was secured. Please select only a single secured source or multiple un-secured sources for searching.", secureSourceDetails.Item1));
                //}
                //if (sic.securedSourceId == null)
                //{
                //    sic.sourceIdList = srcSources.ToArray();
                //}

                sic.sourceIdList = srcSources.ToArray();
                searchReq.sourceInformation = sic;

                // Set the retrieval options
                RetrievalOptions ro = new RetrievalOptions();
                ro.documentMarkup = DocumentMarkup.Display;
                ro.documentMarkupSpecified = true;
                ro.documentView = DocumentView.FullTextWithTerms;
                ro.documentViewSpecified = true;
                searchReq.retrievalOptions = ro;

                SearchSoapBinding searchBind = null;

                try
                {
                    //Create the binding  
                    searchBind = new SearchSoapBinding();

                    searchBind.Url = fixEndPointURL(searchBind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                    // Make the Web Service call
                    if (traceEnabled)
                    {
                        DBManager.Instance.logError(string.Format("Calling: `Search`. Name: {0}. Start Date: {1}. End Date: {2}.", request.searchFullName, ((DateTime)request.currStartDate).ToString("MM/dd/yyyy"), ((DateTime)request.currEndDate).ToString("MM/dd/yyyy")), WS_TRACE_CODE, "SYSTEM");
                    }
                    searchResp = searchBind.Search(searchReq);

                    request.numResultsInRange = Convert.ToInt32(searchResp.documentsFound);

                    // update the request with the searchID
                    request.searchLNID = searchResp.searchId;
                    DBManager.Instance.updateSearch(request);

                    // Don't continue processing if no results found
                    if (request.numResultsInRange == 0)
                    {
                        return results;
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
                    nsmgr.AddNamespace("p", "http://services.v1.wsapi.lexisnexis.com");
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
                    searchReq = null;
                    searchBind = null;
                }
            }

            RetrievalSoapBinding retrieveBind; 

            try {
                //************************ GetDocumentsByRange ************************

                            
                List<string> documents = new List<string>();

                // Create the request and response objects
                GetDocumentsByRange rangeReq = new GetDocumentsByRange();
                RetrievalResponse getByDocRangeResp = null;

                // Populate the request with information
                rangeReq.binarySecurityToken = securityToken;
                rangeReq.searchId = request.searchLNID;

                // Add retrieval options
                RetrievalOptions byDocRangeRo = new RetrievalOptions();
                byDocRangeRo.documentMarkup = DocumentMarkup.Display;
                byDocRangeRo.documentMarkupSpecified = true;
                byDocRangeRo.documentView = DocumentView.FullTextWithTerms;
                byDocRangeRo.documentViewSpecified = true;
                Range range = new Range();
                range.begin = startIndex.ToString();
                range.end = endIndex.ToString();
                byDocRangeRo.documentRange = range;
                rangeReq.retrievalOptions = byDocRangeRo;

                //Create the binding
                retrieveBind = new RetrievalSoapBinding();

                retrieveBind.Url = fixEndPointURL(retrieveBind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                //Make the Web Service call
                if (traceEnabled)
                {
                    DBManager.Instance.logError(string.Format("Calling: `GetDocumentsByRange`. Name: {0}. Start Date: {1}. End Date: {2}. SearchID: {3}. Start Index: {4}. End Index: {5}.", 
                        request.searchFullName, ((DateTime)request.currStartDate).ToString("MM/dd/yyyy"), ((DateTime)request.currEndDate).ToString("MM/dd/yyyy"),
                        request.searchLNID, startIndex.ToString(), endIndex.ToString()), WS_TRACE_CODE, "SYSTEM");
                }
                getByDocRangeResp = retrieveBind.GetDocumentsByRange(rangeReq);

                //Parse the response object
                if (getByDocRangeResp.documentContainerList != null)
                {
                    for (int i = 0; i < getByDocRangeResp.documentContainerList.Length; i++)
                    {
                        // Add Lexis Nexis MSU Libraries images to the file
                        string doc = Encoding.Default.GetString(getByDocRangeResp.documentContainerList[i].document).Replace("<html>",
                            @"<html>
                        <span style=""padding:31px"">
                            <a href=""http://www.lexisnexis.com.proxy1.cl.msu.edu""><img src=""ID_Web_horizontal_lrg_white.gif"" alt=""LexisNexis(R)"" style=""width:200px;height:61px""></a>
                        </span>
                        </span>");
                        documents.Add(doc);
                    }
                }
                
                results = documents;
            }
            catch (WebException we)
            {
                // Possibly an HTTP Error?
                throw we;

            }
            catch (SoapException se)
            {

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(se.Detail.OwnerDocument.NameTable);
                nsmgr.AddNamespace("p", "http://services.v1.wsapi.lexisnexis.com");
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
            }
            return results;
        }


        #endregion

        #region Helper Functions

        /// <summary>
        /// Authenticates against the Lexis Nexis web service, any errors that occur
        /// will be thrown to the caller function.
        /// </summary>
        private void authenticateWebService()
        {
            if (!isActiveSession())
            {
                //************************ Authenticate ************************

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
                    bind = null;

                }

                return;
            }
        }

         /// <summary>
         /// Sends an email to the user with the specified message, will copy the root@host
         /// </summary>
         /// <param name="to">Who to email</param>
         /// <param name="subject">Subject line</param>
         /// <param name="body">Message body</param>
        private void sendEmail(string to, string subject, string body)
        {
            try
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(to);
                /// Optionally can add an email address to also receive all emails sent from the application, like a sys admin
                //message.Bcc.Add("[EMAIL]");
                message.Subject = subject;
                message.From = new System.Net.Mail.MailAddress(string.Format("root@{0}.lib.msu.edu", System.Net.Dns.GetHostName()));
                message.Body = body;
                //message.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
                smtp.Send(message);
            }
            catch (Exception e)
            {
                // log failure but do not stop processing since this is a non-critical feature
                Logger.Instance.logMessage(string.Format("Failed to send email to {0} with subject: {1}. Error: {2}", request.searchUser, subject, e.Message));
                DBManager.Instance.logError(string.Format("Failed to send email to {0} with subject: {1}. Error: {2}. Stack Trace: {3}", request.searchUser, subject, e.Message, e.StackTrace), UI_ERROR_CODE, "SYSTEM");
                return;
            }
        }

        /// <summary>
        /// Determines if the application is running within the allowed time window.
        /// </summary>
        /// <returns>True/False depending on if the current time is within the allowed window</returns>
        private bool verifyProcessingWindow()
        {
            string startTime, endTime;
            try
            {
                // Get the start and end time from the parameter table based on if it is a weekday or weekend
                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                {
                    startTime = AppParams.getParameterByName("WEEKEND_START").AppParamValue;
                    endTime = AppParams.getParameterByName("WEEKEND_END").AppParamValue;
                }
                else
                {
                    startTime = AppParams.getParameterByName("WEEKDAY_START").AppParamValue;
                    endTime = AppParams.getParameterByName("WEEKDAY_END").AppParamValue;
                }
                TimeSpan startSpan = TimeSpan.Parse(startTime);
                TimeSpan endSpan = TimeSpan.Parse(endTime);

                if (IsTimeOfDayBetween(DateTime.Now, startSpan, endSpan))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FormatException fe)
            {
                throw fe;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Determines if a given time is between two time spans
        /// </summary>
        /// <param name="time">Time to check</param>
        /// <param name="startTime">Start time span</param>
        /// <param name="endTime">End time span</param>
        /// <returns>True/False if inbetween timespan</returns>
        private bool IsTimeOfDayBetween(DateTime time,
                                     TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
            {
                return true;
            }
            else if (endTime < startTime)
            {
                return time.TimeOfDay <= endTime ||
                    time.TimeOfDay >= startTime;
            }
            else
            {
                return time.TimeOfDay >= startTime &&
                    time.TimeOfDay <= endTime;
            }

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
         /// Saves the list of html strings to their own html file and text file in the designated 
         /// save location. The file name convention will be:
         /// <saveLocation>/html/Doc_[doc #].html
         /// <saveLocation>/txt/Doc_[doc #].txt
         /// </summary>
         /// <param name="files">List of html strings to write to files</param>
         /// <param name="saveLocation">Location to save the files</param>
         /// <param name="fileNum">Starting index for the file number</param>
         /// <returns>End index for the file number</returns>
         private int saveDocuments(List<string> files, string saveLocation, int fileNum)
        {
            try
            {
                string htmlFolder = Path.Combine(saveLocation,"html");
                string txtFolder = Path.Combine(saveLocation,"txt");

                // Create the directory if it doesn't already exist
                Directory.CreateDirectory(saveLocation);
                Directory.CreateDirectory(htmlFolder);
                Directory.CreateDirectory(txtFolder);

                // Write each file string to an html and txt file
                foreach(string file in files)
                {
                    // write the html file
                    using (FileStream fs = new FileStream(Path.Combine(htmlFolder,string.Format("Doc_{0}.html",fileNum)), FileMode.Create))
                    {
                        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                        {
                            w.WriteLine(file);
                        }
                    }
 
                    // write the txt file
                    using (FileStream fs = new FileStream(Path.Combine(txtFolder, string.Format("Doc_{0}.txt", fileNum)), FileMode.Create))
                    {
                        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                        {
                            w.WriteLine(htmlToTxt(file));
                        }
                    }

                    fileNum++;
                }
            }
             catch (Exception e)
            {
                throw e;
            }
            return fileNum;
        }

         /// <summary>
         /// Converts the given html text to plain text format
         /// </summary>
         /// <param name="html">html to convert</param>
         /// <returns>Plain text string without html formatting</returns>
         private string htmlToTxt(string html)
        {
            string result;

            // decode special characters
            result = WebUtility.HtmlDecode(html);

            // Remove HTML Development formatting
            // Replace line breaks with space
            // because browsers inserts space
            result = result.Replace("\r", " ");
            // Replace line breaks with space
            // because browsers inserts space
            result = result.Replace("\n", " ");
            // Remove step-formatting
            result = result.Replace("\t", string.Empty);
            // Remove repeating spaces because browsers ignore them
            result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                @"( )+", " ");

            // Remove the header (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*head([^>])*>", "<head>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"(<( )*(/)( )*head( )*>)", "</head>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(<head>).*(</head>)", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all scripts (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*script([^>])*>", "<script>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"(<( )*(/)( )*script( )*>)", "</script>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //result = System.Text.RegularExpressions.Regex.Replace(result,
            //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
            //         string.Empty,
            //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"(<script>).*(</script>)", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // remove all styles (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*style([^>])*>", "<style>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"(<( )*(/)( )*style( )*>)", "</style>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(<style>).*(</style>)", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert tabs in spaces of <td> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*td([^>])*>", "\t",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line breaks in places of <BR> and <LI> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*br( )*>", "\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*li( )*>", "\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // insert line paragraphs (double line breaks) in place
            // if <P>, <DIV> and <TR> tags
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*div([^>])*>", "\r\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*tr([^>])*>", "\r\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<( )*p([^>])*>", "\r\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove remaining tags like <a>, links, images,
            // comments etc - anything that's enclosed inside < >
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"<[^>]*>", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // replace special characters:
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @" ", " ",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&bull;", " * ",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&lsaquo;", "<",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&rsaquo;", ">",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&trade;", "(tm)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&frasl;", "/",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&lt;", "<",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&gt;", ">",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&copy;", "(c)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&reg;", "(r)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove all others. More can be added, see
            // http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    @"&(.{2,6});", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // for testing
            //System.Text.RegularExpressions.Regex.Replace(result,
            //       this.txtRegex.Text,string.Empty,
            //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // make line breaking consistent
            result = result.Replace("\n", "\r");

            // Remove extra line breaks and tabs:
            // replace over 2 breaks with 2 and over 4 tabs with 4.
            // Prepare first to remove any whitespaces in between
            // the escaped characters and remove redundant tabs in between line breaks
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\r)( )+(\r)", "\r\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\t)( )+(\t)", "\t\t",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\t)( )+(\r)", "\t\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\r)( )+(\t)", "\r\t",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove redundant tabs
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\r)(\t)+(\r)", "\r\r",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Remove multiple tabs following a line break with just one tab
            result = System.Text.RegularExpressions.Regex.Replace(result,
                    "(\r)(\t)+", "\r\t",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Initial replacement target string for line breaks
            string breaks = "\r\r\r";
            // Initial replacement target string for tabs
            string tabs = "\t\t\t\t\t";
            for (int index = 0; index < result.Length; index++)
            {
                result = result.Replace(breaks, "\r\r");
                result = result.Replace(tabs, "\t\t\t\t");
                breaks = breaks + "\r";
                tabs = tabs + "\t";
            }

            return result;
         }
        
         /// <summary>
         /// Zips together all the files in the provided directory giving it the name
         /// of: [zipNamePrefix]_MMDDYYYY_HHMMSS.zip
         /// </summary>
         /// <param name="directory">Directory to find all the files to zip</param>
         /// <param name="zipNamePrefix">Prefix to use in the zip file name</param>
         /// <param name="zipSaveLocation">Location to save the final zip file</param>
         /// <returns>Returns the full path to the zip file that is created</returns>
        private string zipDocuments(string directory, string zipNamePrefix, string zipSaveLocation)
         {
            string fullPath = Path.Combine(zipSaveLocation, string.Format("{0}.zip", zipNamePrefix));

            using (ZipFile zip = new ZipFile())
            {
                 zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                 zip.AddDirectory(directory, zipNamePrefix);
                 zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G"); 
                 zip.Save(fullPath);
            }

             try
             {
                 Syscall.chmod(Path.GetDirectoryName(fullPath), FilePermissions.S_IROTH); // change the permissions of the user's folder
             }
             catch (Exception e)
             {
                 // log the error but dont fail the process over permissions
                 DBManager.Instance.logError(string.Format("Failed to add read permissions to the user's folder. Error: {0}", e.Message), UI_ERROR_CODE, "SYSTEM");
             }

             try
             {
                 Syscall.chmod(fullPath, FilePermissions.S_IROTH); // change the permissions of the zip file
             }
             catch (Exception e)
             {
                // log the error but dont fail the process over permissions
                 DBManager.Instance.logError(string.Format("Failed to add read permissions to the zip. Error: {0}", e.Message), UI_ERROR_CODE, "SYSTEM");
             }
             return fullPath;
         }
        
         
       
         /// <summary>
         /// Copies the Lexis Nexis logo to the provided base directory
         /// </summary>
         /// <param name="directory">Base directory to save the logo</param>
        private void addLogoImageToFolder(string directory)
         {
             directory = Path.Combine(directory, "html"); //ensure the image are saved to the html sub-directory
             try
             {
                 File.Copy(Path.Combine(ConfigurationManager.AppSettings["logoLocation"], "ID_Web_horizontal_lrg_white.gif"), Path.Combine(directory, "ID_Web_horizontal_lrg_white.gif"));
             }
             catch
             {
                 // ignore errors here since it's just the image file
                 // most likely caused because the file already exists in the folder
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
         /// Determines if the application is currently running or not
         /// </summary>
         /// <returns>True/False depending on if it is running</returns>
         private bool isAppRunning()
        {
             // gets the application's run stat and the stat code for 'running'
            int statCd = DBManager.Instance.getRunStatus();
            int val = AppLookups.getLookupByDescription("Running").AppLookupCd;

             // if the run stat is 'running' then return true, else false
            if (val == statCd) return true;
            else return false;
        }

         /// <summary>
         /// Sets the application run status wit the provided value
         /// </summary>
         /// <param name="runStatus">True/False if the application is currently running or not</param>
         private void setAppRunStatus(bool runStatus)
         {
             int statCode;
             if (runStatus) statCode = AppLookups.getLookupByDescription("Running").AppLookupCd;
             else statCode = AppLookups.getLookupByDescription("Not Running").AppLookupCd;

             DBManager.Instance.setRunStatus(statCode);
         }

         /// <summary>
         /// Gets the secured source ID from the web service for a given source ID
         /// </summary>
         /// <param name="sourceID">Source ID</param>
         /// <returns>Secured Source ID</returns>
         private Tuple<string, string> getSourceDetails(string[] sourceID)
         {
             // Verify our web service connection is still active, and if not 
             // re-authenticate using the same credentials to get a new session
             if (!isActiveSession())
             {
                 authenticateWebService();
             }
             foreach (string source in sourceID)
             {
                 //	Create the request and response objects
                 GetSourceDetails1 getSourceDetailsReq = new GetSourceDetails1();
                 GetSourceDetailsResponse1 getSourceDetailsResp = null;

                 // Populate the request with information
                 getSourceDetailsReq.binarySecurityToken = securityToken;
                 //getSourceDetailsReq.setIncludeSourceElement(Boolean.TRUE);
                 getSourceDetailsReq.sourceId = source;

                 SourceSoapBinding sourceBind = null;


                 try
                 {
                     // Change the endpoint according to your WSK environment
                     sourceBind = new SourceSoapBinding();
                     sourceBind.Url = fixEndPointURL(sourceBind.Url, ConfigurationManager.AppSettings["wskEndPoint"]);

                     // Make the Web Service call
                     getSourceDetailsResp = sourceBind.GetSourceDetails(getSourceDetailsReq);

                     if (getSourceDetailsResp.securedSource != null)
                     {
                         return new Tuple<string, string>(source, getSourceDetailsResp.securedSource.securedSourceId);
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
             return new Tuple<string, string>(null, null);
         }

        #endregion
    }
}
