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

namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// Represents a search request record for the user containing information about the request
    /// </summary>
    public class UserSearch : IUserSearch
    {
        public int searchRecID { get; set; }
        public string searchID { get; set; }
        public string searchName { get; set; }
        public string searchDate { get; set; }
        public string searchStatus { get; set; }
        public string searchResultLocation { get; set; }
        public string searchAction { get; set; }
        public string searchPercent { get; set; }
        public string searchResultCount { get; set; }
        public string searchQueuePosition { get; set; }
        public string searchQuery { get; set; }

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
        /// Constructor for the user search request
        /// </summary>
        /// <param name="id">ID of the search</param>
        /// <param name="name">Name of the search</param>
        /// <param name="date">Date that the search was requested</param>
        /// <param name="status">Current status of the search</param>
        /// <param name="location">Save location of the documents from the search</param>
        /// <param name="action">Allowable action on the search (Download or Delete)</param>
        /// <param name="percent">Percent complete with the search</param>
        /// <param name="count">Count of the number of results returned by the search</param>
        /// <param name="queue">Position in the queue</param>
        /// <param name="query">Query for the search</param>
        public UserSearch(int id = 0, string name = "", string date = null, string status = "", string location = "", string action = "", string percent = "100", string count = "", string queue = "", string query = "")
        {
            this.searchRecID = id;
            this.searchName = name;
            this.searchDate = date;
            this.searchStatus = status;
            this.searchAction = action;
            this.searchResultLocation = location;
            this.searchPercent = percent;
            this.searchResultCount = count;
            this.searchQueuePosition = queue;
            this.searchQuery = query;

            // split out the id from the name
            string[] split = name.Split(new char[] { '_' }, 2);
            this.searchName = split[1].Trim();
            this.searchID = split[0].Trim();
        }
    }
}