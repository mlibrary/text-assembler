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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// Http Handler for downloading a file
    /// </summary>
    public class DownloadFile : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return true; } }


        /// <summary>
        /// Processes the http request and downloads the requested file from the server to the client
        /// </summary>
        /// <param name="context">context for the request and response, the request contains the filename to download</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // protects against unauthenticated users downloading files other than the technical documentation
                if (context.Session["userObject"] == null && !context.Request.QueryString["fileName"].ToString().Contains("LexisNexis WSK Implementation Technical Overview.pdf"))
                {
                    context.Session.Clear();
                    context.Response.Redirect("~/Login.aspx", false);
                    return;
                }

                string destPath = context.Request.QueryString["fileName"].ToString();
                // Check to see if file exist
                FileInfo fi = new FileInfo(destPath);
                if (fi.Exists)
                {
                    context.Response.Clear();
                    context.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"",fi.Name));
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.WriteFile(fi.FullName, false);
                }
                else
                {
                    context.Response.Write("Error! File was not found on the server.");
                }
            }
            catch (Exception exception)
            {
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(exception.Message);
            }
            finally
            {
                HttpContext.Current.Response.End();
            }
        }

    }
}