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
    /// Represents an application parameter object
    /// </summary>
    public class AppParam
    {
        public string AppParamName;
        public string AppParamValue;
        public string AppParamDescription;

        /// <summary>
        /// Constructor for the object
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="description">Parameter description</param>
        public AppParam(string name = "", string value = "", string description = "")
        {
            AppParamName = name;
            AppParamValue = value;
            AppParamDescription = description;
        }
    }

    /// <summary>
    /// Allows easy access to the application parameters
    /// </summary>
    public static class AppParams
    {
        private static List<AppParam> parameters;

        /// <summary>
        /// Returns the parameter object based on the provided name
        /// </summary>
        /// <param name="name">Name of the parameter that should be returned</param>
        /// <returns>Parameter object</returns>
        public static AppParam getParameterByName(string name)
        {
            if (parameters == null) parameters = DBManager.Instance.getParameters();
            return parameters.FirstOrDefault(o => o.AppParamName == name);
        }

    }
}