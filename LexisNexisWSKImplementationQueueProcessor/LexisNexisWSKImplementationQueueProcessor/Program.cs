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
    /// Driver for the program, processes the command line agruments and kicks off the 
    /// relavent processes.
    /// </summary>
    class Program
    {
        #region Fields
        public const string HELP_MSG = @"Lexis Nexis Queue Processor
------------------------------------------------------------------------------
Parameters
------------------------------------------------------------------------------
--processQueue      Processes the current search queue using the remaining 
                    alloted number of searches for the current hour.
--updateSources     Retrieves the most current list of available search soures
                    from the web service.
--processDeletion   Processes the deletion of searches, the number of months
                    retained is stored in the database in APPL_PARAM.
--help              Returns this help message.";

        #endregion

        #region Main Function
        /// <summary>
        /// Processes the command line arguments provided and kicks off the relavent process.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Returns 1 for failure and 0 for sucess</returns>
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine(HELP_MSG);
                return 1;
            }
            else
            {
                switch (args[0].ToString())
                {
                    case "--processQueue":
                        Logger.Instance.logMessage("Starting Processing.");
                        QueueProcessor processor = new QueueProcessor();
                        Tuple<int,string> results = processor.processQueue();
                        if (results.Item1 != 0)
                        {
                            DBManager.Instance.logError(results.Item2, results.Item1, "SYSTEM");
                           Logger.Instance.logMessage(string.Format("Error(s) occured while processing the queue. ({0}) {1}", results.Item1, results.Item2)); 
                            System.Console.WriteLine(string.Format("Error(s) occured while processing the queue. ({0}) {1}", results.Item1, results.Item2)); 
                        }
                        else 
                        { 
                            Logger.Instance.logMessage("Processing Complete."); 
                        }
                        return 0;

                    case "--updateSources":
                        Logger.Instance.logMessage("Starting retrieval of sources.");
                        SourceProcessor srcProcessor = new SourceProcessor();
                        Tuple<int, string> srcresults = srcProcessor.updateSources();
                        if (srcresults.Item1 != 0)
                        {
                            DBManager.Instance.logError(srcresults.Item2, srcresults.Item1, "SYSTEM");
                            Logger.Instance.logMessage(string.Format("Error(s) occured while retrieving the sources. ({0}) {1}", srcresults.Item1, srcresults.Item2));
                            System.Console.WriteLine(string.Format("Error(s) occured while retrieving the sources. ({0}) {1}", srcresults.Item1, srcresults.Item2));
                        }
                        else
                        {
                            Logger.Instance.logMessage("Retrieval Complete.");
                        }
                        return 0;
                    case "--processDeletion":
                        Logger.Instance.logMessage("Starting deletion process.");
                        DeletionProcessor delProcessor = new DeletionProcessor();
                        Tuple<int, string> delresults = delProcessor.processDeletion();
                        if (delresults.Item1 != 0)
                        {
                            DBManager.Instance.logError(delresults.Item2, delresults.Item1, "SYSTEM");
                            Logger.Instance.logMessage(string.Format("Error(s) occured while processing the deletion. ({0}) {1}", delresults.Item1, delresults.Item2));
                            System.Console.WriteLine(string.Format("Error(s) occured while processing the deletion. ({0}) {1}", delresults.Item1, delresults.Item2));
                        }
                        else
                        {
                            Logger.Instance.logMessage("Deletion Processing Complete.");
                        }
                        return 0;
                    case "--help":
                        System.Console.WriteLine(HELP_MSG);
                        break;
                    default:
                        System.Console.WriteLine("An invalid parameter was provided.");
                         System.Console.WriteLine(HELP_MSG);
                        break;
                }
                return 0;
            }
        }
        #endregion
    }
}
