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

namespace LexisNexisWSKImplementationQueueProcessor
{
    /// <summary>
    /// Represents a user's search request
    /// </summary>
    class SearchRequest: ISearchRequest
    {

        public int searchRecID { get; set; }
        public string searchID { get; set; }
        public string searchName { get; set; }
        public int searchStatus { get; set; }
        public string searchResultLocation { get; set; }
        public string searchQuery { get; set; }
        public string searchSource { get; set; }
        public string searchSecureSource { get; set; }
        public DateTime? searchStartDate { get; set; }
        public DateTime? searchEndDate { get; set; }
        public decimal searchPercentComplete { get; set; }
        public string searchUser { get; set; }
        public int searchStartIndex { get; set; }
        public int searchNumberResults { get; set; }
        public string searchMethod { get; set; }
        public DateTime? currStartDate { get; set; }
        public DateTime? currEndDate { get; set; }
        public string errorMsg { get; set; }
        public int retryCount { get; set; }
        public string searchLNID { get; set; } // search ID field from LN search response (used to call getdocumentsbyrange)
        public int numResultsInRange { get; set; }
        public long? fileSize { get; set; }
        public DateTime? fileSizeCheckDate { get; set; }
        public bool readyToDownload { get; set; }
        public int? searchQueuePosition { get; set; }
        public bool emailed { get; set; }


        public string searchFullName
        {
            get { return searchID + "_" + searchName; }
            set
            {
                string[] split = value.Split(new char[] { '_' }, 2);
                this.searchName = split[1].Trim();
                this.searchID = split[0].Trim();
            }
        }

        /// <summary>
        /// Constructor for the search request class
        /// </summary>
        /// <param name="id">ID of the request in the database</param>
        /// <param name="name">User provided name of the request</param>
        /// <param name="status">Current status of the request</param>
        /// <param name="location">Location of the search results zip file</param>
        /// <param name="query">Search query</param>
        /// <param name="source">Search source ID</param>
        /// <param name="startDate">Start date for the search</param>
        /// <param name="endDate">End date for the search</param>
        /// <param name="percent">Percent complete with the search</param>
        /// <param name="user">User ID requesting the search</param>
        /// <param name="startIndex">Index to start the search at</param>
        /// <param name="numResults">Number of results in the search</param>
        /// <param name="method">Search method</param>
        public SearchRequest(int id = 0, string name = "",  int status = 1, string location = "", string query = "", string source = "", DateTime? startDate = null, 
            DateTime? endDate = null, decimal percent = 0.0m, string user = "", int startIndex = 1, int numResults = 0, string method = "", DateTime? currStart = null,
            DateTime? currEnd = null, string errMsg = "", int retryCount = 0, string searchLNID = "", int numResultsInRange = 0, long? fileSize = null, DateTime? fileSizeCheckDate = null,
            bool readyToDownload = false, int? searchQueuePosition = null, bool emailed = false)

        {
            this.searchRecID = id;
            this.searchName = name;
            this.searchStatus = status;
            this.searchResultLocation = location;
            this.searchQuery = query;
            this.searchSource = source;
            this.searchStartDate = startDate;
            this.searchEndDate = endDate;  
            this.searchPercentComplete = percent;
            this.searchUser = user;
            this.searchStartIndex = startIndex;
            this.searchNumberResults = numResults;
            this.searchMethod = method;
            this.currStartDate = currStart;
            this.currEndDate = currEnd;
            this.errorMsg = errMsg;
            this.retryCount = retryCount;
            this.searchLNID = searchLNID;
            this.numResultsInRange = numResultsInRange;
            this.searchQueuePosition = searchQueuePosition;
            this.emailed = emailed;

            this.fileSize = fileSize;
            this.fileSizeCheckDate = fileSizeCheckDate;
            this.readyToDownload = readyToDownload;


            // split out the id from the name
            string[] split = name.Split(new char[] { '_' }, 2);
            this.searchName = split[1].Trim();
            this.searchID = split[0].Trim();
        }
    }
}
