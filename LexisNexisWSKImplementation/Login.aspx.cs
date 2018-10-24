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
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;


namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// Frist screen the user is presented with in the application, allows them to log into the system
    /// </summary>
    public partial class _Default : Page
    {
        #region Fields
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



        #endregion

        #region Constructor
        /// <summary>
        /// On load event for the page. checks to see if the user authenticated with OAuth2
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                    if (Request.ServerVariables["REMOTE_USER"] != null)
                    {
                        if (!Request.ServerVariables["REMOTE_USER"].Equals(string.Empty))
                        {

                            Session["userObject"] = new User(Request.ServerVariables["REMOTE_USER"]);
                            Response.Redirect("~/SearchForm.aspx", false);
                        }
                    }

                
            }
            catch (Exception ex)
            {
                result_text.InnerHtml = string.Format("<p class = 'errLbl'>Error occurred processing authentication: {0}</p><p class = 'errLbl'>{1}</p>", ex.Message, ex.StackTrace);
            }



        }
        #endregion 

        /// <summary>
        /// Downloads the technical overview document
        /// </summary>
        protected void lnkDownloadTech_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("DownloadFile.ashx?fileName={0}", Server.MapPath("Content/LexisNexis WSK Implementation Technical Overview.pdf")), false);
        }

    }
}
