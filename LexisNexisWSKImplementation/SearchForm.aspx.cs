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
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using LexisNexisWSKImplementation.LNWebReference;
using System.Net;


namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// Main screen of the application, allows users to view their search queue status and perform searches (by adding to the queue or ad-hoc)
    /// </summary>
    public partial class SearchForm : System.Web.UI.Page
    {
        #region Fields / Properties

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

        private List<UserSearch> data { get; set; }
        private List<Source> sources { get; set; }

        private User currentUser 
        { 
            get { return (User)Session["userObject"]; }
            set 
            {
                Session["userObject"] = value; 
            }
        }
        private int adhocSearches
        {
            get { return (int)this.Session["adhocSearches"]; }
            set
            {
                this.Session["adhocSearches"] = value;
            }
        }

        private bool isAdhoc
        {
            get { return (bool)this.Session["isAdhoc"]; }
            set
            {
                this.Session["isAdhoc"] = value;
            }
        }

        private int adhocRetries
        {
            get { return (int)this.Session["adhocRetries"]; }
            set
            {
                this.Session["adhocRetries"] = value;
            }
        }
        private string searchString
        {
            get { return (string)this.Session["searchString"]; }
            set
            {
                this.Session["searchString"] = value;
            }
        }
        private string searchSource
        {
            get { return (string)this.Session["searchSource"]; }
            set
            {
                this.Session["searchSource"] = value;
            }
        }
        private DateTime? searchFrom
        {
            get { return (DateTime?)this.Session["searchFrom"]; }
            set
            {
                this.Session["searchFrom"] = value;
            }
        }
        private DateTime? searchTo
        {
            get { return (DateTime?)this.Session["searchTo"]; }
            set
            {
                this.Session["searchTo"] = value;
            }
        }
 

        #endregion

        #region Form Load

        /// <summary>
        /// On load event for the page. Will initialize objects on the form.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {            
            // verify the user has already logged in
            if (currentUser == null)
            {
                Session["userObject"] = null;
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                return;
            }

            error_label.InnerHtml = "";
            string location = "initializing the web service.";
            int errorCode = WS_ERROR_CODE;
            try
            {
                // Perform the below actions only when the page is loaded for the first time
                if (!IsPostBack)
                {
                    adhocRetries = 0; 
                    adhocSearches = 0;
                    TabName.Value = "#search";

                    location = "retrieving the available sources from the database.";
                    errorCode = DB_ERROR_CODE;

                    // Add in the Commonly Used Resource folder
                    this.sources = new List<Source>();
                    List<AppLookup> lkups = AppLookups.getLookupsByCategory("COMMON_SRC");
                    foreach(AppLookup lkup in lkups)
                    {
                        this.sources.Add(new Source(lkup.AppLookupDescription, lkup.AppLookupCd.ToString(), "Commonly Used Resources"));
                    }
                    this.sources.AddRange(DBManager.Instance.getSearchSources());

                    // Check Box Drop Down Lists
                    location = "populating the source dropdowns.";
                    errorCode = UI_ERROR_CODE;
                    lstFolders.DataTextField = "sourceFolderDisplay";
                    lstFolders.DataValueField = "sourceFolder";
                    List<Source> srcFolders = new List<Source>();
                    var grp = this.sources.GroupBy(i => i.sourceFolder);
                    foreach (var src in grp)
                    {
                        srcFolders.Add(new Source("", "", src.Key, src.Count()));
                    }

                    lstFolders.DataSource = srcFolders;
                    lstFolders.DataBind();
                    lstFolders.SelectedIndex = 0;

                    // filter the sources for only the selected folders
                    List<Source> commonSources = new List<Source>();
                    commonSources.AddRange(this.sources.OfType<Source>().Where(s => s.sourceFolder == "Commonly Used Resources"));
                    lstSources.DataTextField = "sourceName";
                    lstSources.DataValueField = "sourceID";
                    lstSources.DataSource = commonSources;
                    lstSources.DataBind();


                    // Put current date in the date range fields
                    txtFrom.Text = DateTime.Today.ToString("MM/dd/yyyy");
                    txtTo.Text = DateTime.Today.ToString("MM/dd/yyyy");

                    // open connection with the application's backend DB
                    location = "testing the application's database connection.";
                    errorCode = DB_ERROR_CODE;
                    DBManager.Instance.testConnection(); // any errors from the test connect will be thrown here

                    // Populate the search method drop down
                    location = "retrieving the available search methods";
                    errorCode = UI_ERROR_CODE;
                    List<SearchMethod> methods = Enum.GetValues(typeof(SearchMethod)).Cast<SearchMethod>().ToList();
                    methods.RemoveAll(x => x.ToString().Contains("Continue"));
                    methods.RemoveAll(x => x.ToString().Contains("Decompounding"));
                    lstMethods.DataSource = methods;
                    lstMethods.DataBind();

                    location = "retrieving the logged in user ID.";
                    errorCode = UI_ERROR_CODE;
                    user_label.InnerHtml = @"<p style=""padding-right:1cm""><b class=""lnLabel"">User ID: </b>" + currentUser.userID + "</p>";

                    searchString = "";
                    searchSource = "";

     
                }
                // Perform the below actions any time the page is reloaded
                location = "retrieving the user's saved search data from the database.";
                errorCode = DB_ERROR_CODE;
                this.data = DBManager.Instance.getUserSearchGrid(currentUser.userID);
                errorCode = UI_ERROR_CODE;
                myListView.DataSource = this.data;
                myListView.DataBind();

                // set the run window
                location = "retrieving the next processing window from the database.";
                runWindow.InnerHtml = string.Format("Today's processing window for the search queue is scheduled at : {0}", DBManager.Instance.getNextSearchWindow().ToString("MM/dd/yy hh:mm:ss tt"));

                TabName.Value = Request.Form[TabName.UniqueID];                            
            }
            catch (Exception ex)
            {
                error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred while {0}</p> <p class = 'errLbl'>{1}</p>", location, ex.Message);
                DBManager.Instance.logError(string.Format("An error occurred while {0}. Error: {1}", location, ex.Message), errorCode, currentUser.userID);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for the search button, will process the search criteria that was populated.
        /// </summary>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            performAdhocSearch();
        }

        /// <summary>
        /// Event handler for the save search button on the form. Attempts to save the users search parameters in the 
        /// database so it can be run at a later time.
        /// </summary>
        protected void btnQueueSearch_Click(object sender, EventArgs e)
        {
            performQueueSearch();
        }

        /// <summary>
        /// Event handler for when the user clicks on the logout button, will return them to the sign on screen
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
       {
           error_label.InnerHtml = "";
           int errorCode = UI_ERROR_CODE;

           try
           {
               currentUser = null;
               Session["userObject"] = null;
               Session.Clear();
               Response.Redirect("https://oauth.itservices.msu.edu/oauth/logout", false);
           }
           catch (Exception ex)
           {
               error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred attempting to logout.</p> <p class = 'errLbl'>{0}</p>", ex.Message);
               DBManager.Instance.logError(string.Format("An error occurred while attempting to logout. Error: {0}", ex.Message), errorCode, currentUser.userID);
           }
       }

        /// <summary>
        /// Event handler for when the user clicks on the download the effective search tips button
        /// </summary>
        protected void lnkDownloadTips_Click(object sender, EventArgs e)
        {
            string FilePath = Server.MapPath("Content/LexisNexis WSK Implementation Tips for Effective Searches.pdf");
            WebClient User = new WebClient();
            Byte[] FileBuffer = User.DownloadData(FilePath);
            if (FileBuffer != null)
            {
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-length", FileBuffer.Length.ToString());
                Response.BinaryWrite(FileBuffer);
            }
        }

        /// <summary>
        /// Event handler for when the refresh button is clicked on the user's search queue tab.
        /// Will refresh the grid containing the latest status of their searches.
        /// </summary>
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            error_label.InnerHtml = "";
            userTab_results.InnerHtml = "";
            int errorCode = DB_ERROR_CODE;

            // verify the user has already logged in
            if (currentUser == null)
            {
                Session["userObject"] = null;
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                return;
            }

            try
            {
                this.data = DBManager.Instance.getUserSearchGrid(currentUser.userID);
                errorCode = UI_ERROR_CODE;
                myListView.DataSource = this.data;
                myListView.DataBind();

                // set the run window
                runWindow.InnerHtml = string.Format("Today's processing window for the search queue is scheduled at : {0}", DBManager.Instance.getNextSearchWindow().ToString("MM/dd/yy hh:mm:ss tt"));

                TabName.Value = Request.Form[TabName.UniqueID];
            }
            catch (Exception ex)
            {
                error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred attempting to refresh the search status grid.</p> <p class = 'errLbl'>{0}</p>", ex.Message);
                DBManager.Instance.logError(string.Format("An error occurred while attempting to refresh the search status grid. Error: {0}", ex.Message), errorCode, currentUser.userID);
            }
        }
           
        /// <summary>
        /// Event handler for when one of the action buttons is pressed on the user's search grid
        /// The value of e.CommandArgument is the APPL_USR_SRCH_STAT_REC_ID for the selected record
        /// </summary>
        protected void lkbCommandAction_Command(object sender, CommandEventArgs e)
        {
            error_label.InnerHtml = "";
            userTab_results.InnerHtml = "";
            int errorCode = UI_ERROR_CODE;

            // verify the user has already logged in
            if (currentUser == null)
            {
                Session["userObject"] = null;
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                return;
            }
            try
            {
                //userTab_results.InnerHtml = "Processing request for search: " + e.CommandArgument.ToString();
                UserSearch rec = this.data.FirstOrDefault(o => o.searchID == e.CommandArgument.ToString());

                // Perform the actions for the 'Cancel' command
                if (rec.searchAction.Equals("Cancel"))
                {
                    // update the database to remove the search
                    errorCode = DB_ERROR_CODE;
                    DBManager.Instance.deleteSearch(rec.searchFullName);

                    // update the grid so it reflects it being removed
                    errorCode = UI_ERROR_CODE;
                    this.data = DBManager.Instance.getUserSearchGrid(currentUser.userID);
                    myListView.DataSource = this.data;
                    myListView.DataBind();
                    userTab_results.InnerHtml = "The search record was sucessfully removed from your queue.";

                }
                // Perform the actions for the 'Download' command
                else if (rec.searchAction.Equals("Download"))
                {
                    Response.Redirect(string.Format("DownloadFile.ashx?fileName={0}", rec.searchResultLocation), false);
                }

                // set the run window
                runWindow.InnerHtml = string.Format("Today's processing window for the search queue is scheduled at : {0}", DBManager.Instance.getNextSearchWindow().ToString("MM/dd/yy hh:mm:ss tt"));

                TabName.Value = Request.Form[TabName.UniqueID];
            }
            catch (Exception ex)
            {
                error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred attempting to process a command on a search.</p> <p class = 'errLbl'>{0}</p>", ex.Message);
                DBManager.Instance.logError(string.Format("An error occurred while attempting to process a command on a seach. Error: {0}", ex.Message), errorCode, currentUser.userID);
            }
        }

        /// <summary>
        /// Event handler for when the selected index of the source folders is changed.
        /// It will populate the second drop down with the sources in the selected folders
        /// </summary>
        protected void lstFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            // get all selected items
            List<string> selectedText = new List<string>();
            List<string> selectedValue = new List<string>();

            foreach (ListItem item in (sender as ListControl).Items)
            {
                if (item.Selected)
                {
                    selectedText.Add(item.Text);
                    selectedValue.Add(item.Value);
                }
            }
            if (selectedValue.Count() > 0)
            {
                lstSources.DataTextField = "sourceName";
                lstSources.DataValueField = "sourceID";

                // Add in the Commonly Used Resource folder
                this.sources = new List<Source>();
                List<AppLookup> lkups = AppLookups.getLookupsByCategory("COMMON_SRC");
                foreach (AppLookup lkup in lkups)
                {
                    this.sources.Add(new Source(lkup.AppLookupDescription, lkup.AppLookupCd.ToString(), "Commonly Used Resources"));
                }
                this.sources.AddRange(DBManager.Instance.getSearchSources());


                List<Source> filterSources = new List<Source>();
                List<Source> finalList = new List<Source>();

                // filter the sources for only the selected folders
                foreach (string folder in selectedValue)
                {
                    filterSources.AddRange(this.sources.OfType<Source>().Where(s => s.sourceFolder == folder));
                }

                // remove duplicate sources from the list (since sources can appear in multiple folders)
                var DistinctItems = filterSources.GroupBy(x => x.sourceID).Select(y => y.First());
                foreach(Source item in DistinctItems)
                {
                    finalList.Add(item);
                }

                filterSources = finalList;
                lstSources.DataSource = filterSources;
                lstSources.DataBind();
            }
            else
            {
                lstSources.DataTextField = "sourceName";
                lstSources.DataValueField = "sourceID";
                lstSources.DataSource = null;
                lstSources.DataBind();
            }
        }

        /// <summary>
        /// Event handler for when the button to apply the source folder filter is updated
        /// Calls the lstFolders_SelectedIndexChanged method
        /// </summary>
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            lstFolders_SelectedIndexChanged(lstFolders, e);
        }

        /// <summary>
        /// Event handler for when the button to add the selected sources is clicked
        /// will append all the selected source IDs to the selected sources text box
        /// </summary>
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in lstSources.Items)
            {
                if (item.Selected)
                {
                    txtSources.Text += item.Value + ",";
                }
            }
            
        }

        /// <summary>
        /// Confirms that the user wants to continue with their search even after the warning
        /// </summary>
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            // continue adhoc search
            if (isAdhoc)
            {
                performAdhocSearch();
            }
            else // continue to queue the search
            {
                performQueueSearch();
            }

            divWarning.Visible = false;
        }

        /// <summary>
        /// Cancels the search so the user can refine their criteria
        /// </summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            divWarning.Visible = false;
        }


        #endregion

        #region Helper Functions
        
        /// <summary>
        /// Performs the adhoc search and first verifies the user inputs
        /// </summary>
        private void performAdhocSearch()
        {
            error_label.InnerHtml = "";
            result_text.InnerHtml = "";
            DateTime? toDate = null;
            DateTime? fromDate = null;
            int errorCode = UI_ERROR_CODE;

            // verify the user has already logged in
            if (currentUser == null)
            {
                Session["userObject"] = null;
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                return;
            }

            // Check if there are available searches left to use this hour and not over the ad hoc limit
            if (DBManager.Instance.getRemainingSearches().Item1 <= 0 || adhocSearches >= Convert.ToInt32(AppParams.getParameterByName("ADHOC_SRCH_PER_SESSION").AppParamValue))
            {
                error_label.InnerHtml = "<p class = 'errLbl'>There are either no remaining searches this hour or you are at the limit for on demand searches. Please queue remaining search requests to be run at a later time.</p>";
                return;
            }

            // Verify user input, if either field is empty show an error to the user
            if (txtSearch.Text.ToString().Trim().Equals(string.Empty))
            {
                error_label.InnerHtml = "<p class = 'errLbl'>Please enter search criteria.</p>";
                return;
            }

            // Verify the source input
            if (txtSources.Text.ToString().Trim().Length > 0)
            {
                // make sure that the provided source is an integer
                try
                {
                    searchSource = txtSources.Text.ToString().Trim();
                    foreach (string source in searchSource.Replace(" ", "").Split(','))
                    {
                        if (!source.Equals(string.Empty))
                        {
                            int val = Convert.ToInt32(source);
                        }
                    }
                }
                catch (FormatException)
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>One or more of the provided title IDs were not a valid integer.</p>";
                    return;
                }
            }
            else
            {
                error_label.InnerHtml = "<p class = 'errLbl'>Please select at least one title to search from.</p>";
                return;
            }

            // Verify the date fields if populated
            try
            {
                if (txtFrom.Text.ToString().Equals(string.Empty) || txtTo.Text.ToString().Equals(string.Empty))
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not valid. Both of the date fields should be propulated.</p>";
                    return;
                }
                fromDate = Convert.ToDateTime(txtFrom.Text.ToString());
                toDate = Convert.ToDateTime(txtTo.Text.ToString());
                // also make sure the to date is greater than the from date
                if (toDate < fromDate)
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not valid. The 'To' date should be later than the 'From' date.</p>";
                    return;
                }

                // Verify the from date is greater than or equal to 1/1/1984
                if (fromDate != null)
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

                    if (fromDate < begDt)
                    {
                        error_label.InnerHtml = string.Format("<p class = 'errLbl'>The 'From' date provided is earlier than LexisNexis has results for. Please pick a date on or after {0}.</p>", begDt.ToString(@"MM\/dd\/yyyy"));
                        return;
                    }
                }

                // Verify the date range is within 10 yrs
                if (fromDate != null && toDate != null)
                {
                    DateTime TenYears = ((DateTime)fromDate).AddYears(10);
                    if (toDate > TenYears)
                    {
                        error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was more than 10 years in span, please break searches into 10 year blocks.</p>";
                        return;
                    }
                }
            }
            catch (FormatException)
            {
                error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not properly formated as a date. Try: 5/14/2015</p>";
                return;
            }

            try
            {
                // Retrieve the selected search method
                SearchMethod method;
                if (!Enum.TryParse<SearchMethod>(lstMethods.SelectedValue.ToString(), out method))
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>Please select a valid search method.</p>";
                    return;
                }

                // Clean the search source input
                List<string> srcSources = new List<string>();
                foreach (var source in searchSource.Replace(" ", "").Split(','))
                {
                    if (!source.Equals(string.Empty))
                    {
                        srcSources.Add(source);
                    }
                }

                if (srcSources.Count >= 1000 && divWarning.Visible == false)
                {
                    isAdhoc = true;
                    divWarning.Visible = true;
                    return;
                }

                searchString = txtSearch.Text.ToString();
                searchFrom = fromDate;
                searchTo = toDate;
                errorCode = WS_ERROR_CODE;
                int totalResults = 0;
                bool limitedRange = false;
                Tuple<List<SearchResult>, string> results = WebService.Instance.adhocSearch(searchString, srcSources.ToArray(), searchFrom, searchTo, method, out totalResults, ref limitedRange);
                // Increment the number of searches performed this hour
                errorCode = DB_ERROR_CODE;
                DBManager.Instance.incrementSearchesPerHour();
                errorCode = UI_ERROR_CODE;
                adhocSearches++;               
                result_text.InnerHtml = buildSearchResultsDisplay(results, totalResults, limitedRange);
                adhocRetries = 0;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("SEARCH_TOO_GENERAL"))
                {
                    error_label.InnerHtml = @"<p class = 'errLbl'>Error: The search criteria was too general.<br/>
                        You have received this response because your search matches more than 10,000 results. In order to retrieve more than 10,000 results per day you 
                        will need to break your request into multiple queries wherein each query matches 10,000 or less results per day, queuing each query for download 
                        along the way.  For example if you searched for ‘Obama’ over the course of a year, across all English language newspapers, you would likely 
                        return more than 10,000 results. In order to capture all matching content you would need to create multiple jobs scoped to smaller sources. 
                        For advice on capturing datasets larger than 10,000 items contact 
                        <a href=""http://staff.lib.msu.edu/chua/"" style=""color:#0000FF;border-bottom: 1px solid #0000FF;"">Hui Hua Chua</a>. </p>";
                }
                else if (ex.Message.Contains("EXPIRED_SECURITY_TOKEN") && adhocRetries == 0)
                {
                    WebService.Instance.authenticate();
                    adhocRetries++;
                    performAdhocSearch();
                }
                else if (adhocRetries == 0)
                {
                    adhocRetries++;
                    performAdhocSearch();
                }
                else
                {
                    error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred attempting to search.</p> <p class = 'errLbl'>{0}</p>", ex.Message);
                }
                DBManager.Instance.logError(string.Format("An error occurred while attempting to search. Error: {0}", ex.Message), errorCode, currentUser.userID);
            }
        }

        /// <summary>
        /// Verifies the users input then saves the search to the queue
        /// </summary>
        private void performQueueSearch()
        {
            error_label.InnerHtml = "";
            result_text.InnerHtml = "";
            DateTime? toDate = null;
            DateTime? fromDate = null;

            // verify the user has already logged in
            if (currentUser == null)
            {
                Session["userObject"] = null;
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                return;
            }

            int errorCode = UI_ERROR_CODE;
            // Verify user input, if either field is empty show an error to the user
            if (txtSearch.Text.ToString().Trim().Equals(string.Empty))
            {
                error_label.InnerHtml = "<p class = 'errLbl'>Please enter search criteria.</p>";
                return;
            }
            if (txtSearchName.Text.ToString().Trim().Equals(string.Empty))
            {
                error_label.InnerHtml = "<p class = 'errLbl'>Please enter a corupus name.</p>";
                return;
            }

            // Verify the source input
            if (txtSources.Text.ToString().Trim().Length > 0)
            {
                // make sure that the provided source is an integer
                try
                {
                    searchSource = txtSources.Text.ToString().Trim();
                    foreach (string source in searchSource.Replace(" ", "").Split(','))
                    {
                        if (!source.Equals(string.Empty))
                        {
                            int val = Convert.ToInt32(source);
                        }
                    }
                }
                catch (FormatException)
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>One or more of the provided title IDs were not a valid integer.</p>";
                    return;
                }
            }
            else
            {
                error_label.InnerHtml = "<p class = 'errLbl'>Please select at least one title to search from.</p>";
                return;
            }

            // Clean the search source input
            List<string> srcSources = new List<string>();
            foreach (var source in searchSource.Replace(" ", "").Split(','))
            {
                if (!source.Equals(string.Empty))
                {
                    srcSources.Add(source);
                }
            }
            if (srcSources.Count >= 1000 && divWarning.Visible == false)
            {
                isAdhoc = false;
                divWarning.Visible = true;
                return;
            }

            // Verify the date fields if populated
            try
            {
                if (txtFrom.Text.ToString().Equals(string.Empty) || txtTo.Text.ToString().Equals(string.Empty))
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not valid. Both of the date fields should be propulated.</p>";
                    return;
                }
                fromDate = Convert.ToDateTime(txtFrom.Text.ToString());
                toDate = Convert.ToDateTime(txtTo.Text.ToString());
                // also make sure the to date is greater than the from date
                if (toDate < fromDate)
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not valid. The 'To' date should be later than the 'From' date.</p>";
                    return;
                }

                // Verify the from date is greater than or equal to 1/1/1984
                if (fromDate != null)
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

                    if (fromDate < begDt)
                    {
                        error_label.InnerHtml = string.Format("<p class = 'errLbl'>The 'From' date provided is earlier than LexisNexis has results for. Please pick a date on or after {0}.</p>", begDt.ToString(@"MM\/dd\/yyyy"));
                        return;
                    }
                }

                // Verify the date range is within 10 yrs
                if (fromDate != null && toDate != null)
                {
                    DateTime TenYears = ((DateTime)fromDate).AddYears(10);
                    if (toDate > TenYears)
                    {
                        error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was more than 10 years in span, please break searches into 10 year blocks.</p>";
                        return;
                    }
                }
            }
            catch (FormatException)
            {
                error_label.InnerHtml = "<p class = 'errLbl'>The date range provided was not properly formated as a date. Try: 5/14/2015</p>";
                return;
            }      

            try
            {
                // Retrieve the selected search method
                SearchMethod method;
                if (!Enum.TryParse<SearchMethod>(lstMethods.SelectedValue.ToString(), out method))
                {
                    error_label.InnerHtml = "<p class = 'errLbl'>Please select a valid search method.</p>";
                    return;
                }

                //clear results of last search
                result_text.InnerHtml = "";

                // Generate a Unique ID for the search name
                string id = generateUID();

                // Save the search to the database
                errorCode = DB_ERROR_CODE;
                DBManager.Instance.saveSearch(id + "_" + txtSearchName.Text.ToString(), txtSearch.Text.ToString(), searchSource.Replace(" ", ""), toDate, fromDate, method.ToString(), currentUser.userID);

                // clear form and add label for sucess, also refresh the user search grid so it will have the latest search
                try
                {
                    this.data = DBManager.Instance.getUserSearchGrid(currentUser.userID);
                    myListView.DataSource = this.data;
                    myListView.DataBind();
                }
                catch
                {
                    // ignore this error
                }

                errorCode = UI_ERROR_CODE;
                error_label.InnerHtml = "<p class = 'errLbl'>The search was sucessfully saved!</p>";
                txtSearchName.Text = "";
                txtSearch.Text = "";
            }
            catch (Exception ex)
            {
                error_label.InnerHtml = string.Format("<p class = 'errLbl'>An error occurred attempting to save the search.</p> <p class = 'errLbl'>{0}</p>", ex.Message);
                DBManager.Instance.logError(string.Format("An error occurred while attempting to save the user's search. Error: {0}", ex.Message), errorCode, currentUser.userID);
            }
        }

        /// <summary>
        /// Builds a display table based on the search result objects
        /// </summary>
        /// <param name="results">List of search result objects to convert to an html string for display</param>
        /// <returns>string containing the html tables with the results</returns>
        private string buildSearchResultsDisplay(Tuple<List<SearchResult>,string> results, int totalResults, bool limitedRange)
        {
            string display = "";

            // Check if there are any results
            if (results.Item1 == null || results.Item1.Count == 0) return "<p class = 'errLbl'>The search returned 0 results.</p>";

            display = string.Format("<p class='narrowP'>{0}The search returned a total of <strong>{1}</strong> results{2}, displaying only the first <strong>{3}</strong> results.", 
                limitedRange ? "WARNING: The date range provided returned too many results for the preview search so a more limited date range was used.</br>" : "", totalResults, results.Item2, results.Item1.Count) +
                    "<br/>Due to limitations on the number of allowed searches that can be performed on an on demand basis, please use the ‘Queue Corpus for Download’ feature to retrieve the full set of results.</p>";


            int i = 0;
            foreach (SearchResult result in results.Item1)
            {
                display += string.Format(@" 
                <table>
                      <tr><td><b>Document:</b> {7}</td><td><b>Headline:</b> {0}</td></tr>
                      <tr><td><b>Publisher:</b> {1}</td><td><b>Publish Date:</b> {2}</td></tr>
                      <tr><td><b>Length:</b> {3}</td><td><b>Link:</b> {4}</td></tr>
                      <tr><td><a href=""#{6}"" style=""text-decoration:underline;color:blue;"" data-toggle=""collapse"" data-target=""#{6}"">View Full Text</a></td><tr>
              </table>
              <div id=""{6}"" class=""collapse"">
                {5}
              </div>
              <hr/>", result.resultHeadline, result.resultPublisher, result.resultPublishDate, result.resultLength, result.resultLink, result.resultHTML, "result" + i++, i);
            }
            return display;
        }

        /// <summary>
        /// Generates a unique 16 byte string to use as an ID
        /// </summary>
        /// <returns>16-byte unique ID</returns>
        private string generateUID()
        {
            const string availableChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            using (var generator = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                generator.GetBytes(bytes);
                var chars = bytes
                    .Select(b => availableChars[b % availableChars.Length]);
                var token = new string(chars.ToArray());
                return token;
            }
  
        }
   
       #endregion

        

       
    }
}