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
    /// Represents an Application Lookup object
    /// </summary>
    public class AppLookup
    {
        public int AppLookupCd;
        public string AppLookupCategory;
        public string AppLookupDescription;

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="code">Lookup code value</param>
        /// <param name="category">Lookup category name</param>
        /// <param name="description">Lookup descrtipion</param>
        public AppLookup(int code = 0, string category = "", string description = "")
        {
            AppLookupCd = code;
            AppLookupCategory = category;
            AppLookupDescription = description;
        }
    }

    /// <summary>
    /// Class to search the application lookup values either by the category or description
    /// </summary>
    public static class AppLookups
    {
        private static List<AppLookup> lookups;

        /// <summary>
        /// Returns the AppLookup object for the given description value
        /// </summary>
        /// <param name="desc">Description of the the lookup value to return</param>
        /// <returns>AppLookup object for the given description</returns>
        public static AppLookup getLookupByDescription(string desc)
        {
            if (lookups == null) lookups = DBManager.Instance.getLookups();
            return lookups.FirstOrDefault(o => o.AppLookupDescription == desc);
        }

        /// <summary>
        /// Returns all of the AppLookup objects for the provided category
        /// </summary>
        /// <param name="category">Category to filter for</param>
        /// <returns>List of the AppLookup objects for the given category</returns>
        public static List<AppLookup> getLookupsByCategory(string category)
        {
            if (lookups == null) lookups = DBManager.Instance.getLookups();
            return lookups.Where(o => o.AppLookupCategory == category).ToList();
        }
    }
}
