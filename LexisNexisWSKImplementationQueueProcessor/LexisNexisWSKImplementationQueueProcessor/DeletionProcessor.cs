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
using System.Linq;
using System.Text;

namespace LexisNexisWSKImplementationQueueProcessor
{
    class DeletionProcessor
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
        public DeletionProcessor()
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
        public Tuple<int,string> processDeletion()
        {
            Tuple<int, string> results = new Tuple<int, string>(0, "");
            errorLocation = "";
            errorCode = 0;

            try
            {
                // process the deletion in the database
                errorLocation = "running the stored procedure to process the deletion";
                errorCode = DB_ERROR_CODE;
                List<string> files = DBManager.Instance.deleteSearches();

                // delete search files at the locations returned from the database
                errorLocation = "deleting the search files from the filesystem";
                errorCode = UI_ERROR_CODE;
                deleteSearchFiles(files);
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
        /// Deletes a list of files from the server
        /// </summary>
        /// <param name="files">List of files that should be deleted, assumes they are the full path</param>
        private void deleteSearchFiles(List<string> files)
         {
            foreach (string f in files)
            {
                // check if the path exists, if so, delete it
                try
                {
                    File.Delete(f);
                }
                catch (Exception e)
                {
                    string message = string.Format("Error deleting {0}. Error: {1}", f, e.Message);
                    DBManager.Instance.logError(message, UI_ERROR_CODE, "SYSTEM");
                    Logger.Instance.logMessage(message);
                    System.Console.WriteLine(message);
                }

                // Remove the location from the database even if an error occured
                DBManager.Instance.removeSearhLocation(f);
            }
        }
       
        #endregion
    }
}
