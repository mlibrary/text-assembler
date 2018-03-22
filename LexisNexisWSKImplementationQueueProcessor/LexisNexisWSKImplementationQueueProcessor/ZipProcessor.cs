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
using System.IO;

namespace LexisNexisWSKImplementationQueueProcessor
{
    class ZipProcessor
    {
        #region Fields

        private int errorCode;
        private string errorLocation;

        private int UI_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("UI Error").AppLookupCd; }
        }
        private int DB_ERROR_CODE
        {
            get { return AppLookups.getLookupByDescription("Database Error").AppLookupCd; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for the class, currently does nothing
        /// </summary>
        public ZipProcessor()
        {

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Processes the deletion of old search records by first calling the stored procedure to 
        /// do the deletion in the database, then removing all the returned files from the file
        /// system as well
        /// </summary>
        /// <returns>Tuple with the error code and message if any occur</returns>
        public Tuple<int, string> processZips()
        {
            Tuple<int, string> results = new Tuple<int, string>(0, "");
            errorLocation = "";
            errorCode = 0;

            try
            {
                // process the deletion in the database
                errorLocation = "retrieving searches to check";
                errorCode = DB_ERROR_CODE;
                List<SearchRequest> searches = DBManager.Instance.getCompletedSearches();


                // loop through each search and check if it is able to be marked as completed
                errorCode = UI_ERROR_CODE;
                foreach (SearchRequest search in searches)
                {
                    // get the filesize of the zip
                    errorLocation = string.Format("checking the zipfile size for search: {0}", search.searchFullName);
                    FileInfo fileData = new FileInfo(search.searchResultLocation);

                    if (!fileData.Exists)
                    {
                        Logger.Instance.logMessage(string.Format("Search result file does not exist for search: '{0}' at: '{1}'", search.searchFullName, search.searchResultLocation));
                        continue;
                    }

                    // get the size of the zip file
                    long curSize = fileData.Length;

                    // compare it with the filesize last checked
                    // if it is the same from over an hour ago then it is ready to be marked ready and remove uncompressed directory
                    errorLocation = string.Format("comparing zipfile size for search: {0}", search.searchFullName);
                    if (curSize == search.fileSize && DateTime.Now >= ((DateTime)search.fileSizeCheckDate).AddHours(1))
                    {
                        search.readyToDownload = true;

                        // send email that the search is complete
                        string body = string.Format(@"Your queued search has successfully completed. Please log on to https://lexnex.lib.msu.edu to download your results.

Search Name: {0}
Number of Results: {1}", search.searchName, search.searchNumberResults);
                        if (search.emailed == false)
                        {
                            sendEmail(search.searchUser + "@msu.edu", "Text Assembler: Search Complete", body);
                            search.emailed = true;
                            DBManager.Instance.updateSearch(search);
                        }

                        errorLocation = string.Format("updating the database after processing zip for search: {0}", search.searchFullName);
                        DBManager.Instance.updateSearch(search);

                        // Delete the un-zipped files since they are no longer needed
                        errorLocation = string.Format("removing the unzipped directory with search results (search: {0})", search.searchFullName);
                        try
                        {
                            // get the directory to delete from the zip file name
                            dir = search.searchResultLocation.Replace(part_to_remove, "");

                            errorLocation = string.Format("removing the unzipped directory with search results (directory: {0})", dir);

                            deleteDirectory(dir);
                        }
                        catch (Exception e)
                        {
                            // we will log the error but not cause the process to fail here since this is a trivial issue that can be cleaned up independently
                            Logger.Instance.logMessage(string.Format("Error {0}. Error: {1}.", errorLocation, e.Message));
                            DBManager.Instance.logError(string.Format("Error {0}. Error: {1}. Stack Trace {2}", errorLocation, e.Message, e.StackTrace), errorCode, "SYSTEM");
                        }
                    }
                    // else update the database with the latest check info
                    else if (curSize != search.fileSize)
                    {
                        search.fileSize = curSize;
                        search.fileSizeCheckDate = DateTime.Now;
                    }

                    errorLocation = string.Format("updating the database after processing zip for search: {0}", search.searchFullName);
                    DBManager.Instance.updateSearch(search);
                }
            }
            catch (Exception ex)
            {
                results = new Tuple<int, string>(errorCode, string.Format("An error occurred while {0}. Error: {1}", errorLocation, ex.Message));
            }
            finally
            {

            }

            return results;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Recursively deletes everything in the provided directory and finally, the 
        /// folder itself.
        /// </summary>
        /// <param name="directory">Directory to delete allong with any files in it</param>
        private void deleteDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;
                File.SetAttributes(file, FileAttributes.Normal);
                if (!File.Exists(file)) continue;
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                deleteDirectory(dir);
            }

            try
            {
                Directory.Delete(directory, true);
            }
            catch (DirectoryNotFoundException)
            {
                // do nothing, the directory has already been deleted
            }
            catch (IOException)
            {
                Directory.Delete(directory, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(directory, true);
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
                message.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
                smtp.Send(message);
            }
            catch (Exception e)
            {
                // log failure but do not stop processing since this is a non-critical feature
                Logger.Instance.logMessage(string.Format("Failed to send email to {0} with subject: {1}. Error: {2}", to, subject, e.Message));
                DBManager.Instance.logError(string.Format("Failed to send email to {0} with subject: {1}. Error: {2}. Stack Trace: {3}", to, subject, e.Message, e.StackTrace), UI_ERROR_CODE, "SYSTEM");
                return;
            }
        }


        #endregion
    }
}
