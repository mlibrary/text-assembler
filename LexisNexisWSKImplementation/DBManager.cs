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

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Configuration;

namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// Manages all interaction with the application's database
    /// </summary>
    public class DBManager
    {
        #region Fields
        private static DBManager instance;
        #endregion

        #region Constructor
        /// <summary>
        /// Privater constructor for the class, currently does nothing
        /// </summary>
        private DBManager()
        {
        }

        /// <summary>
        /// Public constructor for the class using the Singleton design pattern to enforce that only 1 DB manager
        /// instance exists for the session.
        /// </summary>
        public static DBManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBManager();
                }
                return instance;
            }

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tests the connection to the application's database
        /// </summary>
        /// <returns>True/False depending on it is able to establish a connection</returns>
        public bool testConnection()
        {
            MySqlConnection con = getConnection();

            if (con.ConnectionString.Equals(string.Empty))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Tests the connection to the application's database
        /// </summary>
        /// <param name="conn">Connection to test</param>
        /// <returns>True/False depending on it is able to establish a connection</returns>
        public bool testConnection(MySqlConnection con)
        {
            try
            {
                con.Open();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e; // this error can't be logged to the database since we can't even connect, so just throw it
            }
        }

        #region Getters

        /// <summary>
        /// Gets a database connection object first trying the primary connection found in the 
        /// config, but otherwise using the Failover connection
        /// </summary>
        /// <returns>Connection Object</returns>
        public MySqlConnection getConnection()
        {
            // Try primary database first
            MySqlConnection con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["DBConnString"].ConnectionString);
            try
            {
                if(testConnection(con))
                {
                    return con;
                }
            }
            catch { }

            // Try Failover database
            con = new MySqlConnection(WebConfigurationManager.ConnectionStrings["DBConnStringFailOver"].ConnectionString);
            try
            {
                if (testConnection(con))
                {
                    return con;
                }
            }
            catch { }

            // Neither database connection worked so return an empty connection string
            return new MySqlConnection("");
            
        }

        /// <summary>
        /// Gets the user's search queue grid from the database
        /// </summary>
        /// <param name="userID">ID of the user to retrieve the data for</param>
        /// <returns>List of UserSearch result objects</returns>
        public List<UserSearch> getUserSearchGrid(string userID)
        {
            List<UserSearch> results = new List<UserSearch>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_usr_srch", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@USR_NME", userID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // name, date, status, result location, action
                                results.Add(new UserSearch(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                                    reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetString(5), reader.GetString(6), 
                                    reader.IsDBNull(7) ? "" : reader.GetString(7), reader.IsDBNull(8) ? "" : reader.GetString(8), reader.IsDBNull(9) ? "": reader.GetString(9)));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return results;
        }

        /// <summary>
        /// Gets all of the available search sources from the database
        /// </summary>
        /// <returns>List of source objects</returns>
        public List<Source> getSearchSources()
        {
            List<Source> results = new List<Source>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_srch_src", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // Id, name, folder
                                results.Add(new Source(reader.GetString(1), reader.GetString(0), reader.GetString(2)));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return results;
        }

        /// <summary>
        /// Gets the remaining number of available searches for the hour as well as the number of searches
        /// already been performed.
        /// </summary>
        /// <returns>Tuple with the remaining searches available and the number already performed</returns>
        public Tuple<int, int> getRemainingSearches()
        {
            Tuple<int, int> results = null;
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_appl_srch_stat", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlParameter outParmRemaining = new MySqlParameter("@REMAINING", MySqlDbType.Int32);
                    outParmRemaining.Direction = System.Data.ParameterDirection.Output;
                    MySqlParameter outParmCurrent = new MySqlParameter("@CURRENT", MySqlDbType.Int32);
                    outParmCurrent.Direction = System.Data.ParameterDirection.Output;

                    cmd.Parameters.Add(outParmRemaining);
                    cmd.Parameters.Add(outParmCurrent);

                    cmd.ExecuteNonQuery();

                    results = new Tuple<int, int>(outParmRemaining.Value as int? ?? default(int), outParmCurrent.Value as int? ?? default(int));

                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return results;
        }

        /// <summary>
        /// Retrieves the next available processing window for the search queue
        /// </summary>
        /// <returns>Timestamp of the next run window</returns>
        public DateTime getNextSearchWindow()
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_nex_run_window", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlParameter outParm = new MySqlParameter("@RUN_WINDOW", MySqlDbType.DateTime);
                    outParm.Direction = System.Data.ParameterDirection.Output;

                    cmd.Parameters.Add(outParm);

                    cmd.ExecuteNonQuery();

                    return outParm.Value as DateTime? ?? default(DateTime);

                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
        }

        /// <summary>
        /// Gets all of the lookup values from the database
        /// </summary>
        /// <returns>List of lookup objects</returns>
        public List<AppLookup> getLookups()
        {
            List<AppLookup> results = new List<AppLookup>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_appl_lkup", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // Id, name, folder
                                results.Add(new AppLookup(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return results;
        }

        /// <summary>
        /// Gets all of the application parameters from the database
        /// </summary>
        /// <returns>List of application parameter objects</returns>
        public List<AppParam> getParameters()
        {
            List<AppParam> results = new List<AppParam>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_appl_param", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // name, value, description
                                results.Add(new AppParam(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return results;
        }

        #endregion

        #region Setters
        /// <summary>
        /// Adds a new log record to the APPL_LOG table in the database based 
        /// on the given inputs.
        /// </summary>
        /// <param name="errorMsg">Error message to be logged</param>
        /// <param name="logCd">Error code associated with the error</param>
        /// <param name="userID">User ID that the error occurred for</param>
        public void logError(string errorMsg, int logCd, string userID)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_add_log", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LOG_CD", logCd);
                    cmd.Parameters.AddWithValue("@LOG_MSG", errorMsg);
                    cmd.Parameters.AddWithValue("@LOG_USR", userID);

                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // currently do nothing with the errors and do not throw them up since it would probably result in an unhandled exception
                // this this could be called mostly in a catch block.
            }
        }
        
        /// <summary>
        /// Save the search parameters to the database to be included in part of the queue
        /// </summary>
        /// <param name="searchName">Name of the search, will be displayed on the UI grid instead of the search query</param>
        /// <param name="searchQuery">Query text for the search</param>
        /// <param name="sourceID">Source ID to perform the search against</param>
        /// <param name="toDate">The 'To' parameter in the date range filter</param>
        /// <param name="fromDate">The 'From' parameter in the date range filter</param>
        /// <param name="userID">User ID that requested the search</param>
        public void saveSearch(string searchName, string searchQuery, string sourceID, DateTime? toDate, DateTime? fromDate, string method, string userID)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_add_usr_srch", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SRCH_NME", searchName);
                    cmd.Parameters.AddWithValue("@SRCH_USR", userID);
                    cmd.Parameters.AddWithValue("@SRCH_QRY", searchQuery);
                    cmd.Parameters.AddWithValue("@SRCH_SRC", sourceID);
                    cmd.Parameters.AddWithValue("@SRCH_TO_DT", toDate);
                    cmd.Parameters.AddWithValue("@SRCH_FROM_DT", fromDate);
                    cmd.Parameters.AddWithValue("@SRCH_MTHD", method);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error saving the user's search to the database. Error: {0}", ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
        }
        
        /// <summary>
        /// Removes the specified user search from the queue by terminating the record in the database table
        /// </summary>
        /// <param name="searchName"Name of the search to remove from the queue</param>
        public void deleteSearch(string searchName)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_del_usr_srch", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SRCH_NME", searchName);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error saving the user's search to the database. Error: {0}", ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
        }
       
        /// <summary>
        /// Calls the stored procedure that incrememnts the number of searches performed this hour by 1
        /// </summary>
        public void incrementSearchesPerHour()
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_incmnt_appl_srch_stat", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error saving the user's search to the database. Error: {0}", ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
        }

        
        #endregion
        #endregion
    }
}