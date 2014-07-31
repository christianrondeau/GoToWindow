// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Diagnostics;
using System.Security.Permissions;
using Microsoft.Search.Interop;
[assembly: CLSCompliant(true)]

namespace GoToWindow.Api
{
    /// <summary>
    /// Query Windows Index for .exe and .lnk
    /// </summary>
    public static class WindowsSearch
    {
        // Displays each matching line in each file
        static void DisplayMatchingLines(StringCollection fileList)
        {
            // launch findstr
            ProcessStartInfo si = new ProcessStartInfo();

            // run findstr with number, case insensitive, literal
            si.Arguments = "/N /i /P /L ";

            // add search userQuery as parameters to findstr
            foreach (string term in termList)
            {
                // exact on each phrase
                si.Arguments += "/C:\"" + term + "\" ";
            }

            // loading from each file passed on standardinput
            si.Arguments += " /F:/";
            si.FileName = "findstr.exe";
            si.CreateNoWindow = true;
            si.WindowStyle = ProcessWindowStyle.Hidden;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            si.RedirectStandardInput = true;

            // Execute FINDSTR
            Process p = Process.Start(si);

            StreamWriter writer = p.StandardInput;
            StreamReader readerFindstrResults = p.StandardOutput;

            foreach (string file in fileList)
            {
                writer.WriteLine(file);
            }
            writer.Close();

            String line;
            while ((line = readerFindstrResults.ReadLine()) != null)
            {
                Debug.WriteLine(line);
            }
            readerFindstrResults.Close();
        }

        [STAThread]
        public static List<string> Search(string query)
        {
            List<string> results = new List<string>();

            if (query.Trim().Length == 0)
            {
                return results;
            }

            filePattern = query;
            exts = new string[] { ".lnk", ".exe" };

            // This uses SearchAPI interop assembly
            CSearchManager manager = new CSearchManager();

            // the SystemIndex catalog is the default catalog that windows uses
            CSearchCatalogManager catalogManager = manager.GetCatalog("SystemIndex");

            // get the ISearchQueryHelper which will help us to translate AQS --> SQL necessary to query the indexer
            CSearchQueryHelper queryHelper = catalogManager.GetQueryHelper();

            // set the number of results we want
            if (maxRows > 0)
            {
                queryHelper.QueryMaxResults = maxRows;
            }

            // set the columns we want
            queryHelper.QuerySelectColumns = "System.ItemPathDisplay";

            // default is to scope to anywhere in file system
            queryHelper.QueryWhereRestrictions = "AND scope='file:'";

            // if we have a file pattern 
            if (filePattern.Length > 0)
            {
                // then we add file pattern restriction, mapping cmd line style wildcards to SQL style wildcards
                string pattern = filePattern;
                pattern = pattern.Replace("*", "%");
                pattern = pattern.Replace("?", "_");

                if (pattern.Contains("%") || pattern.Contains("_"))
                {
                    queryHelper.QueryWhereRestrictions += " AND System.FileName LIKE '" + pattern + "' ";
                }
                else
                {
                    // if there are no wildcards we can use a contains which is much faster as it uses the index
                    queryHelper.QueryWhereRestrictions += " AND Contains(System.FileName, '" + pattern + "') ";
                }
            }

            // if we have file extensions
            if (exts != null)
            {
                // then we add a constraint against the System.ItemType column in the form of
                // Contains(System.ItemType, '.txt OR .doc OR .ppt') 
                queryHelper.QueryWhereRestrictions += " AND Contains(System.ItemType,'";
                bool fFirst = true;
                foreach (string ext in exts)
                {
                    if (!fFirst)
                    {
                        queryHelper.QueryWhereRestrictions += " OR ";
                    }
                    queryHelper.QueryWhereRestrictions += "\"" + ext + "\"";
                    fFirst = false;
                }
                queryHelper.QueryWhereRestrictions += "') ";
            }

            // and we always have a sort column and direction, either the default or the one specified in the parameters
            // so append an ORDER BY statement for it
            queryHelper.QuerySorting = sortCol + " " + sortDirection;

            // Generate SQL from our parameters, converting the userQuery from AQS->WHERE clause
            string sqlQuery = queryHelper.GenerateSQLFromUserQuery(userQuery);

            // --- Perform the query ---
            // create an OleDbConnection object which connects to the indexer provider with the windows application
            System.Data.OleDb.OleDbConnection conn = new OleDbConnection(queryHelper.ConnectionString);

            // open it
            conn.Open();

            // now create an OleDB command object with the query we built above and the connection we just opened.
            OleDbCommand command = new OleDbCommand(sqlQuery, conn);

            // execute the command, which returns the results as an OleDbDataReader.
            OleDbDataReader WDSResults = command.ExecuteReader();

            int nResults = 0;
            while (WDSResults.Read())
            {
                nResults++;
                // col 0 is always our path in display format
                string path = WDSResults.GetString(0);

                // output the path
                results.Add(path);
            }
            WDSResults.Close();
            conn.Close();

            return results;
        }

        // default sort column
        static string sortCol = "System.ItemPathDisplay";

        // default sort direction
        static string sortDirection = "ASC";

        // Maximum number of rows to return
        static int maxRows;

        // Pattern for filename
        static string filePattern = "";

        // AQS query constructed from terms
        static string userQuery = " ";

        // List of terms to search for
        static StringCollection termList = new StringCollection();

        // Set of extensions to search for
        static string[] exts;

    }
}
