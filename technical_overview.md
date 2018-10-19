Technical Documentation
====================================

Table of contents
------------------------------------
* [Purpose](#purpose)
* [Components](#components)
* [Front End Functionality](#font-end-functionality)
* [Queue Processor](#queue-processor)
* [Zip Processor](#zip-processor)
* [Source Processor](#source-processor)
* [More Help](#more-help)

Purpose
------------------------------------
The purpose of this application is to consume the web service kit (WSK) provided by Lexis Nexis to provide MSU users 
with the ability to perform searches for research or data mining while incorporating the throttling controls associated 
with the limited license that MSU Libraries has with Lexis Nexis.

Components
------------------------------------
- C# ASP.NET Web Site:  
Allows users to queue search requests and download completed results. 

- C# Console Application:  
Processes the search queue that is stored in the backend database. 

- MariaDB Backend Database:  
Stores search status and lookup information such as the throttling limitations. 


Front End Functionality
------------------------------------
The web front end allows users to enter search criteria and preview the first 10 results that the web service returns, 
queue searches that they want to retrieve the full result set for, and download completed search request results. 
If a search is too general when attempting to preview the results, it will try to pull the first 10 results from last 
date of the range and the page will specify what range the result preview is from.

The parameters provided to the user are all of the ones made available via the WSK. The source list browser is a 
compiled list of all the sources that are pulled on a regular basis from the WSK (see the 
[Source Processor](#source-processor) for more information on how that works).

The download queue tab allows users to manage their queued search requests. They can view the current status 
of their search, download it if it is complete, or cancel it if it hasn't already started processing.

One note on the percent complete; due to the way the queue processor works it is impossible to get an accurate 
percent complete calculation. So instead the percent complete reflects how many days of the date range have been 
processed. So if it seems stuck at 0% for quite some time, that could be because it is still processing the same 
large range with thousands of results.

Queue Processor
------------------------------------
The queue processor's job is to loop through all the searchs in the queue and attempt to download as many of the 
results as possible within the constraints of the configured limitations defined in the database. The process steps 
are as follows:  

**Step 1:** Determine if the process should be run (a connection with the application’s database can be established, 
is within the valid run window, has remaining searches for the hour, and there are search requests in the queue).

**Step 2:** Loop through each of the search requests in the queue and call the search processor function for it. 
When it gets the active search queue it will rotate the order so that no one search can monopolize the queue. This 
is useful when there is a large search that may take weeks to run, so it will not block other small searches from 
being completed in the mean time. 

**Step 3:** The search processor will attempt the call the web service for the search request for the entire date range 
specified, and if successful it will loop through the results based on the number of documents we can retrieve per 
search and download the results. It will save each document in html and txt format to the server. It will update the 
database after each iteration with the current index it is at. After each iteration it will also check that there are 
still remaining searches left for the current hour before continuing, otherwise it will stop.

**Step 4:** If the web service call for the entire date range fails, it will break the date range in half and try doing 
the left half of the range and then the right half. The function is written recursively so it will continue to split the 
date range until it gets to a range of 1 day before marking it as an invalid search for being too general. Once it finds 
a valid date range where the search is not too general, it will follow the same process described in step 3 with 
iterating through the results based on the current index (but will also store the current start and end date that it is 
working on for that index so it can pick up where it left off if it runs out of searches for the current hour).

*Note:* The reason we store the current index, start and end date is so the processor can pick up a search from the 
middle if it runs out of searches for that current hour and has to stop. For example if we have a search from 
10/1/2015 – 10/20/2015 that was too general and it is currently on the 211th document in the range 10/17/2015 – 
10/20/2015, it won’t have to restart the processing from the beginning because it knows we have already downloaded the 
documents from 10/1/2015 – 10/16/2015 and the first 210 documents in the range of 10/17/2015 – 10/20/2015.

**Step 5:** Once complete with the search, it will update the record for the search request in the database with 
the save location on the server and the percent complete. And it will then kick off the process to zip the results.

**Step 6:** After all search requests have been processed or it ran out of searches for the current hour, the application will update the run status in the database to indicate that it is not running.


Zip Processor
------------------------------------
The zip processor was split out into its own process that runs regularly to check what searches have been 
completed and see if the zip job for them have completed so it can notify the user. The reason this had to be split 
out was because the queue processor application kicks off the zip command and continues execution even if it hasn't 
completed yet. This resulted in users being notified of completed searches when they were in fact still compressing. 
This was particularly an issue with large searches that took long periods of time to complete.

The steps this processor performs are as follows:

**Step 1:** Check if another instance of the zip processor is running. If it is, stop processing; otherwise mark 
it as running and continue.

**Step 2:** Identify all of the completed searches in the database. This will be all the searches 100% complete but 
are not yet marked as downloadable.

**Step 3:** Loop through all the completed searches and for each one: Check to see if a zip file for the search 
already exists. If it does not exist, then it will report an error since the queue processor should have started the 
zip process before marking it as ready in the database. But otherwise it will see if the size has changed at all since 
the last time it was checked. If there has been no change in the size in 
over an hour then we can assume that the zip process is complete. In that case we can mark it as complete in the 
database and notify the user that it is ready to download. It will also remove all the uncompressed files for the 
search since they are no longer required. If the size has changed within the past hour then we just 
update the size and time it was last checked in the database and stop processing that search.


Source Processor
------------------------------------
The source processor's job is to pull the latest list of searchable source IDs from the WSK to make sure our 
system is always up-to-date with additions and deletions from the list. Here are the steps that it performs each time 
it runs: 

**Step 1:** Verify that the process can establish a connection to the application’s database.

**Step 2:** Recursively get all of the sources in a folder starting from the root folder when calling the web service 
function “BrowseSources”.  For each source record it finds in the folder it will add it to a list of source objects that 
contains the source name, number, and folder.

**Step 3:** Once all of the sources have been retrieved, it will soft-delete all of the active records in the search 
source table of the application's database and then bulk insert all of the new source records as the new active set.



More Help
------------------------------------
For additional questions involving how the application works or setting it up, please contact me:

Megan Schanz, Systems Programmer  
schanzme@lib.msu.edu