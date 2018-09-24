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
using System.Configuration;

namespace LexisNexisWSKImplementationQueueProcessor
{
    class DBManager
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
            MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["DBConnString"].ConnectionString);
            try
            {
                if (testConnection(con))
                {
                    return con;
                }
            }
            catch { }

            // Try Failover database
            con = new MySqlConnection(ConfigurationManager.ConnectionStrings["DBConnStringFailOver"].ConnectionString);
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

        /// <summary>
        /// Gets all of the active queued searches from the database
        /// </summary>
        /// <returns>List of search request objects</returns>
        public List<SearchRequest> getSearchQueue()
        {
            List<SearchRequest> results = new List<SearchRequest>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_srch_queue", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // id, name, status, result location, query, source, start date, end date, percent complete
                                results.Add(new SearchRequest(id: reader.GetInt32(0),
                                    name: reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    status: reader.IsDBNull(2) ? 1 : reader.GetInt32(2),
                                    location: reader.IsDBNull(3) ? null : reader.GetString(3),
                                    query: reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    source: reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    startDate: reader.IsDBNull(6) ? (DateTime?)null : (DateTime?)reader.GetDateTime(6),
                                    endDate: reader.IsDBNull(7) ? (DateTime?)null : (DateTime?)reader.GetDateTime(7),
                                    percent: reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                    user: reader.IsDBNull(9) ? "" : reader.GetString(9),
                                    startIndex: reader.IsDBNull(10) ? 1 : reader.GetInt32(10), 
                                    numResults: reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                    method: reader.IsDBNull(12) ? "" : reader.GetString(12),
                                    currStart: reader.IsDBNull(13) ? (DateTime?)null : (DateTime?)reader.GetDateTime(13),
                                    currEnd: reader.IsDBNull(14) ? (DateTime?)null : (DateTime?)reader.GetDateTime(14), 
                                    errMsg: "",
                                    retryCount: reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                                    searchLNID: reader.IsDBNull(16) ? "" : reader.GetString(16),
                                    numResultsInRange: reader.IsDBNull(17) ? 0: reader.GetInt32(17),
                                    searchQueuePosition: reader.IsDBNull(18) ? (int?)null : reader.GetInt32(18)
                                    ));
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
        /// Get all of the searches that have completed but not marked as ready for download
        /// </summary>
        /// <returns></returns>
        public List<SearchRequest> getCompletedSearches()
        {
            List<SearchRequest> results = new List<SearchRequest>();
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_zip_queue", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // id, name, status, result location, query, source, start date, end date, percent complete
                                results.Add(new SearchRequest(id: reader.GetInt32(0),
                                    name: reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    status: reader.IsDBNull(2) ? 1 : reader.GetInt32(2),
                                    location: reader.IsDBNull(3) ? null : reader.GetString(3),
                                    query: reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    source: reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    startDate: reader.IsDBNull(6) ? (DateTime?)null : (DateTime?)reader.GetDateTime(6),
                                    endDate: reader.IsDBNull(7) ? (DateTime?)null : (DateTime?)reader.GetDateTime(7),
                                    percent: reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                    user: reader.IsDBNull(9) ? "" : reader.GetString(9),
                                    startIndex: reader.IsDBNull(10) ? 1 : reader.GetInt32(10),
                                    numResults: reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                    method: reader.IsDBNull(12) ? "" : reader.GetString(12),
                                    currStart: reader.IsDBNull(13) ? (DateTime?)null : (DateTime?)reader.GetDateTime(13),
                                    currEnd: reader.IsDBNull(14) ? (DateTime?)null : (DateTime?)reader.GetDateTime(14),
                                    errMsg: "",
                                    retryCount: reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                                    searchLNID: reader.IsDBNull(16) ? "" : reader.GetString(16),
                                    numResultsInRange: reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                                    fileSize: reader.IsDBNull(18) ? 0 : reader.GetInt64(18),
                                    fileSizeCheckDate: reader.IsDBNull(19) ? (DateTime?)null : (DateTime?)reader.GetDateTime(19),
                                    readyToDownload: reader.IsDBNull(20) ? false : reader.GetBoolean(20)
                                    ));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return results;
        }


        /// <summary>
        /// Retrieves the applications's run status from the database
        /// </summary>
        /// <param name="typeCode">Job code from lookup table</param>
        /// <returns>Boolean with the run status value</returns>
        public bool getRunStatus(int typeCode)
        {
            int results;
            bool result = false;
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_get_run_stat", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@JOB_TYP", typeCode);
                    MySqlParameter outParmStat = new MySqlParameter("@RUN_STAT", MySqlDbType.Int32);
                    outParmStat.Direction = System.Data.ParameterDirection.Output;

                    cmd.Parameters.Add(outParmStat);

                    cmd.ExecuteNonQuery();

                    results = outParmStat.Value as int? ?? default(int);
                    result = results == 1 ? true : false;
                }
            }
            catch (Exception)
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error retrieving the user's search grid from the database. Error: {0}",ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
            return result;
        }


        #endregion
        #region Setters
        /// <summary>
        /// Adds a new log record to the APPL_LOG table in the database based 
        /// on the given inputs.
        /// </summary>
        /// <param name="errorMsg">Error message to be logged</param>
        /// <param name="logCd">Error code associated with the error</param>
        /// <param name="userID">User ID that the error occured for</param>
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
        
        /// <summary>
        /// Updates the database search record with the updated data in the provided object
        /// </summary>
        /// <param name="record">Search record with updated data</param>
        public void updateSearch(SearchRequest record)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_upd_usr_srch", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SRCH_NME", record.searchFullName);
                    cmd.Parameters.AddWithValue("@STAT_CD", record.searchStatus);
                    cmd.Parameters.AddWithValue("@RSLT_LOC", record.searchResultLocation);
                    cmd.Parameters.AddWithValue("@PCT_CMPLT", record.searchPercentComplete);
                    cmd.Parameters.AddWithValue("@ST_INDX", record.searchStartIndex);
                    cmd.Parameters.AddWithValue("@NUM_RSLTS", record.searchNumberResults);
                    cmd.Parameters.AddWithValue("@CURR_ST", record.currStartDate);
                    cmd.Parameters.AddWithValue("@CURR_END", record.currEndDate);
                    cmd.Parameters.AddWithValue("@ERR_MSG", record.errorMsg);
                    cmd.Parameters.AddWithValue("@RETRY_CNT", record.retryCount);
                    cmd.Parameters.AddWithValue("@LN_ID", record.searchLNID);
                    cmd.Parameters.AddWithValue("@RNG_RSLTS", record.numResultsInRange);
                    cmd.Parameters.AddWithValue("@FILE_SIZE", record.fileSize);
                    cmd.Parameters.AddWithValue("@FILE_CHECK", record.fileSizeCheckDate);
                    cmd.Parameters.AddWithValue("@READY_FLAG", record.readyToDownload);
                    cmd.Parameters.AddWithValue("@QUEUE_POS", record.searchQueuePosition);
                    cmd.Parameters.AddWithValue("@EMAILED", record.emailed);

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
        /// Updates the run status in the database
        /// </summary>
        /// <param name="isRunning">If the process is running</param>
        /// <param name="typeCode">Job code from the lookup table</param>
        public void setRunStatus(int typeCode, bool isRunning)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_set_run_stat", con);
                
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@JOB_TYP", typeCode);
                    cmd.Parameters.AddWithValue("@RUNNING", isRunning);

                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // log the error to the database before throwing the exception back to the UI to handle
                //logError(string.Format("Error saving the user's search to the database. Error: {0}", ex.Message), DB_ERROR_CODE, userID);
                throw;
            }
        }

        /// <summary>
        /// Updates the database with the latest list of searchable sources pulled from the web service.
        /// Will first terminate the previous source records and then insert the new ones.
        /// </summary>
        /// <param name="sources">List of source objects to be inserted into APPL_SRCH_SRC</param>
        public void setSearchSources(List<Source> sources)
        {

            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();

                    MySqlCommand cmd = con.CreateCommand();
                    MySqlTransaction trans;
                    trans = con.BeginTransaction();
                    cmd.Connection = con;
                    cmd.Transaction = trans;

                    try
                    {
                        cmd.CommandText = "UPDATE APPL_SRCH_SRC SET APPL_SRCH_SRC_REC_TRMN_DT = NOW() WHERE APPL_SRCH_SRC_REC_TRMN_DT IS NULL";
                        cmd.ExecuteNonQuery();

                        string insert = "INSERT INTO APPL_SRCH_SRC (APPL_SRCH_SRC_ID, APPL_SRCH_SRC_NME, APPL_SRCH_SRC_FLDR, APPL_SRCH_SRC_REC_EFF_DT, APPL_SRCH_SRC_REC_TRMN_DT) VALUES (@id,@name,@folder,NOW(),NULL)";
                        for (int i = 0; i < sources.Count; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", sources[i].sourceID);
                            cmd.Parameters.AddWithValue("@name", sources[i].sourceNameOnly);
                            cmd.Parameters.AddWithValue("@folder", sources[i].sourceFolder);

                            cmd.CommandText = insert;
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            trans.Rollback();
                            throw;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// Calls the stored procedure that processes search deletion and returns the search
        /// files that need to be deleted from the server as well.
        /// </summary>
        /// <returns>List of strings with the paths to files that need to be deleted</returns>
        public List<string> deleteSearches()
        {
            List<string> fileLocations = new List<string>();

            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_del_usr_records", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                fileLocations.Add(reader.GetString(0));
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

            return fileLocations;
        }

        /// <summary>
        /// Removes the search location from the database
        /// </summary>
        /// <param name="path">The path to remove</param>
        public void removeSearhLocation(string path)
        {
            try
            {
                using (MySqlConnection con = getConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("p_del_usr_path", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // throw the exception back to the UI to handle
                throw;
            }
        }
        #endregion
        #endregion
    }
}
