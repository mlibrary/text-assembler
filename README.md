Lexis Nexis WSK Implementation Project Documentation
====================================
Table of contents
------------------------------------
* [References](#references)
* [System Requirements](#minimum-system-requirements)
* [Environment](#environment)
* [Setup Instructions](#setup-instructions)
* [Systems Administration](#systems-administration)
* [Troubleshooting](#troubleshooting)




References
------------------------------------

- Lexis Nexis developer reference site: https://www.lexisnexis.com/webserviceskit/


Minimum System Requirements
------------------------------------

- Ubuntu 14.04.02 LTS
- MariaDB 5.5 (Should work on MySQL, but haven't tested it)
- Apache 2.4.7
- Mono 5.0
- Git 1.9.1 (If using Git for a code repository)

Special Ubuntu Packages:

- libapache2-mod-mono
- mono-apache-server4


Environment
------------------------------------
This was developed in a Windows environment using Visual Studio 2013 Community Edition and stored the code base in a Git repository. The server is a Ubuntu machine that uses Mono and Apache to serve the application.  

Setup Instructions
---------------------------------

**1. Install Apache**

```
sudo apt-get install apache2
```

**2.  Create the folder for the new site**

```
sudo mkdir -p /var/www/lexnex
sudo chown $USER:$USER /var/www/lexnex
sudo chmod -R 755 /var/www
```

**3. Create the new config for the site, modifying the fields as necessary**

```
<VirtualHost *:80>
        ServerName [SERVER_NAME]
        Redirect "/" "https://[SERVER_NAME]/"
</VirtualHost>

<VirtualHost *:443>
        ServerAdmin [SERVER_ADMIN_EMAIL]
        ServerName [SERVER_NAME]
        ServerAlias www.[SERVER_NAME]
        DocumentRoot /var/www/lexnex
        
        SSLEngine on
        SSLCertificateFile [PATH_TO_.crt]
        SSLCertificateKeyFile [PATH_TO_.key]
        SSLCertificateChainFile [PATH_TO_interm.cer]
        SSLProtocol all -TLSv1 -SSLv2 -SSLv3
        SSLCipherSuite ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-SHA384:ECDHE-RSA-AES256-SHA384:ECDHE-ECDSA-AES128-SHA256:ECDHE-RSA-AES128-SHA256
        SSLHonorCipherOrder     on
        SSLCompression          off

        MonoServerPath [SERVER_NAME] "/usr/bin/mod-mono-server4"
        MonoPath [SERVER_NAME] "/usr/lib/mono/4.5/usr/lib"
        MonoSetEnv [SERVER_NAME] MONO_IOMAP=all
        MonoApplications [SERVER_NAME] "/:/var/www/lexnex"

        <Directory /var/www/lexnex>
                Options -Indexes +FollowSymLinks +MultiViews
                AllowOverride None
                Require all granted
                MonoSetServerAlias [SERVER_NAME]
                SetHandler mono
                DirectoryIndex Login.aspx
        </Directory>

        ErrorLog ${APACHE_LOG_DIR}/ln_error.log
        CustomLog ${APACHE_LOG_DIR}/ln_access.log combined

</VirtualHost>

```

Update the apache2.conf file to comment out the /var/www/ directory section.

```
#<Directory /var/www/>
#       Options Indexes FollowSymLinks
#       AllowOverride None
#       Require all granted
#</Directory>
```

**4. Enable the new site**

```
sudo a2ensite lexnex.conf
```

**5. Setup SSL**
 - Create a new SSL certificate
 
 Production:
 ```
 openssl req -out CSR.csr -new -newkey rsa:2048 -nodes -keyout lexnex.key
 ```
 Development:
 ```
 openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout lexnex.key -out lexnex.crt
 ```

 - Enable SSL on Apache
 
 ```
 sudo a2enmod ssl
 ```
 
**6. Restart Apache**

```
sudo service apache2 restart
```

**7. Create the mono registry directory**

```
sudo mkdir /var/www/.mono
sudo chown root:root /var/www/.mono
sudo chmod 777 /var/www/.mono -R
```

**8. Install Mono**
http://www.mono-project.com/download/#download-lin-ubuntu 
```
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/ubuntu trusty main" | sudo tee /etc/apt/sources.list.d/mono-official.list
sudo apt-get update
sudo apt-get install mono-complete
sudo apt-get install mono-xsp4
sudo apt-get install libapache2-mod-mono mono-apache-server4
sudo a2enmod mod_mono
```

**9. Setup the Lexis Nexis site** 

- Copy the contents of the LexisNexis folder into `/var/www/lexnex` folder
- Make a copy of the `LexisNexisWSKImplementation\Web.config.exmaple` and call it `Web.config` and update the the connection string information and web service connection information

```
 <connectionStrings>
   <add
         name="DBConnString"
         connectionString="Server=[DB_HOST]; Database=lexis_nexis; UID=[DB_USER]; Password=[DB_PASS];"
         providerName="System.Data.SqlClient"/>
  <add
         name="DBConnStringFailOver"
        connectionString="Server=[DB_HOST]; Database=lexis_nexis; UID=[DB_USER]; Password=[DB_PASS];"
         providerName="System.Data.SqlClient"/>
    </connectionStrings>
 <appSettings>
    <add key="wskEndPoint" value="[LN_ENDPOINT]"/>
    <add key="wskID" value="[LN_USER]" />
    <add key="wskPassword" value="[LN_PASS]" />
	
	<add key="MsuClientId" value="[MSU_CLIENT_ID]" />
    <add key="MsuClientSecret" value="[MSU_CLIENT_SECRET]" />
    <add key="MsuRedirectUri" value="https://[SERVER_NAME]/Login.aspx" />
...
 </appSettings>
```

- When run locally, it will bypass OAuth authentication (setup in `Login.aspx.cs`), but you will need to configure or remove the OAuth authentication if it is deployed on a server. The relavent files are `OAuthHelpers/MsuOAuth2Client.cs` and `App_Start/AuthConfig.cs`

- Rebuild the project once the changes are made

```
sudo xbuild LexisNexisWSKImplementation.cproj
```

- Remove the extra dlls from the bin folder leaving only: 

 ```
DotNetOpenAuth.AspNet.dll
DotNetOpenAuth.AspNet.xml
DotNetOpenAuth.Core.dll
DotNetOpenAuth.Core.xml
DotNetOpenAuth.OAuth.Consumer.dll
DotNetOpenAuth.OAuth.Consumer.xml
DotNetOpenAuth.OAuth.dll
DotNetOpenAuth.OAuth.xml
GroupDropDownList.dll
GroupDropDownList.pdb
LexisNexisWSKImplementation.dll
LexisNexisWSKImplementation.dll.config
LexisNexisWSKImplementation.pdb
log4net.dll
log4net.xml
Microsoft.AspNet.Membership.OpenAuth.dll
Microsoft.AspNet.Membership.OpenAuth.xml
MySql.Data.dll
Newtonsoft.Json.dll
Newtonsoft.Json.xml
System.Web.Providers.dll
System.Web.Providers.xml
 ```

**10. Restart Apache**

```
sudo service apache2 stop
sudo service apache2 start
```

**11. Deploy the Lexis Nexis queue processor**

- Copy the LexisNexisQueueProcessor folder to the desired location
-  Make a copy of the `LexisNexisWSKImplementationQueueProcessor\app.config.example` and call it `app.config` and update the folder paths, the database connection information, and web service connection information

```
 <connectionStrings>
   <add
         name="DBConnString"
         connectionString="Server=[DB_HOST]; Database=lexis_nexis; UID=[DB_USER]; Password=[DB_PASS];"
         providerName="System.Data.SqlClient"/>
  <add
         name="DBConnStringFailOver"
        connectionString="Server=[DB_HOST]; Database=lexis_nexis; UID=[DB_USER]; Password=[DB_PASS];"
         providerName="System.Data.SqlClient"/>
  </connectionStrings>
 <appSettings>
    <add key="logoLocation" value="[LOGO_DIR]" /> <!-- LexisNexis that is added to the result files -->
    <add key="saveLocation" value="[SEARCH_RESULTS_DIR]" /> <!-- Location where the search results should be stored -->
    <add key="logFilename" value="[LOG_FULLPATH]" /> <!-- Full name and path to the log file -->
	<add key="wskEndPoint" value="[LN_ENDPOINT]"/>
    <add key="wskID" value="[LN_USER]" />
    <add key="wskPassword" value="[LN_PASS]" />
...
 </appSettings>
```

- The queue processor is configured to send emails when a search completes (either sucessfully or in error). The email body text will need to be updated for your situation as well as the reciptiant's email since it currently assumes it is always [username]@msu.edu  

- Rebuild the project once the changes are made

```
sudo xbuild LexisNexisWSKImplementationQueueProcessor.cproj
```

- Remove the extra dll files

```
sudo rm bin\Debug\System.*  
sudo rm bin\Debug\Microsoft.*  
sudo rm bin\Debug\Mono.Posix.ddl  
```

- Add a new Cron entry so that the processor runs every hour, the sources will update daily, and the search deletion processing will be run daily

```
sudo vim /etc/crontab
```

```
@hourly mono <path to the .exe> --processQueue
@daily mono <path to the .exe> --processDeletion
@daily mono <path to the .exe> --updateSources
```

**12. Database setup**

- Run the DB script on the target database 

- Make any desired modifications to the APPL_PARAM table that contains query restrictions to be used for the WSK

Systems Administration
------------------------------------

All errors that occur are stored in the APPL_LOG table of the database (lexis_nexis).

If a user is having issues downloading their completed search, this most likely is because they permissions on the user's folder within the mounted share are wrong. You can create a cron job that runs regularly to correct any that get created with the wrong permissions (the .NET code should do this when it creates them, but I've noticed some instances where it hasn't).  
```
0 6 * * * root chmod 755 [BASE_PATH_TO_SEARCH_RESULTS]/*
```

Most other errors can be corrected by starting and stopping Apache, but note that Mono related issues only get corrected when Apache is stopped then started (not just restarted).  

To run the Mono clean-up steps, run the following commands:  
```
cd /var/www/lexnex
./clean_bin
```

For additional troubleshooting tips, see the section below.  


Troubleshooting
------------------------------------

- To enable debug information to be displayed on the web page, modify the Web.config file and add the below section: 

```
 <system.web>
    <customErrors mode="Off" />
</system.web>
```

- The most common errors occur because the bin folder has extra dll files in it. To fix, run 
the clean_bin script with the following command:

```
sudo /var/www/lexnex/clean_bin
```

which will run commands to remove these files from the `/var/www/lexnex.lib.msu.edu/` directory: 
 
```
bin/System.*
bin/Microsoft.*
bin/DotNet*
bin/Entity*
bin/Asp*
```

Then copy only the required files back from the repository's bin directory (stored in `/var/www/lexnex.lib.msu.edu/bin_bkup/`):  
```
DotNetOpenAuth.AspNet.dll  
DotNetOpenAuth.AspNet.xml  
DotNetOpenAuth.Core.dll  
DotNetOpenAuth.Core.xml  
DotNetOpenAuth.OAuth.Consumer.dll  
DotNetOpenAuth.OAuth.Consumer.xml  
DotNetOpenAuth.OAuth.dll  
DotNetOpenAuth.OAuth.xml  
GroupDropDownList.dll  
GroupDropDownList.pdb  
LexisNexisWSKImplementation.dll  
LexisNexisWSKImplementation.dll.config  
LexisNexisWSKImplementation.pdb  
log4net.dll  
log4net.xml  
Microsoft.AspNet.Membership.OpenAuth.dll  
Microsoft.AspNet.Membership.OpenAuth.xml  
MySql.Data.dll  
Newtonsoft.Json.dll  
Newtonsoft.Json.xml  
System.Web.Providers.dll  
System.Web.Providers.xml  
```


- It is important to note that some errors require Apache to be completely stopped and restarted instead of just performing 
a restart.

- An error with 'Invalid IL Code' usually means that some of the dll files were not removed from the bin folder when they should have been

- Note that commented out code is still processed by Mono, so if testing something by commenting it out, be sure to remove the code from the file instead of commenting it out before compiling.

- When Apache configtest fails on a Mono related error, check if there was an update performed on the server affecting Mono. If doing apt-get update fails because of a checksum error run the following to clear the cache and re-try the update:

```
sudo rm /var/lib/apt/lists/*
```

- Assuming the site still does not work because of configtest failure, ensure that mod-mono is enabled (in /etc/apache2/mods-enabled) and that the .so file exists that it references. If it still does not work, make sure all mono packages are marked with i and not c in aptitude search mono.

- Sometime restarting Apache does not stop all of the Mono processes (you'll see these in `htop` still). In this case the server 
should just be rebooted. You'll notice this happening on system login when there system load is reporting higher than 1.  

- After Mono is updated and Apache starts failing on the configtest, try to totally uninstall mono and re-install it making sure you are grabbing the 
latest version of the packge from http://www.mono-project.com/download/#download-lin-ubuntu. Steps to totally uninstall:  
```
sudo apt-get remove mono-complete mono-xsp4 libapache2-mod-mono mono-apache-server4
sudo apt-get autoremove
sudo rm /etc/apt/sources_list.d/mono*
```
Then follow the commands for your version at the url to add back the correct package location. then re-run the install steps:  
```
sudo apt-get update
sudo apt-get install mono-complete mono-xsp4 libapache2-mod-mono mono-apache-server4
sudo a2enmod mod_mono
sudo service apache2 restart
```

- Note on cron job timings: the processor application that processes the search queue runs every hour, but stops if it fails the check
to the database to see if it is within the processing window (currently nights and weekends). So normally I try not to reboot the
server right on the hour so it doesn't mess up any runs, or also not at night since that is when the processing is occuring.

- Note on errors reported to the APPL_LOG table in the database: it is normal to see a few errors daily when the processor has been
processing a query because the Lexis Nexis web service regularly will return an INTERNAL_SERVER_ERROR code to us when re-trying with
the same query just seconds later will work. I log all the errors but just increment the retry_count field in the main search status
table to allow queries to be retried a certain number of times before marking them as invalid.

- Also, there is a nightly job that will re-pull the latest sources that users can query from, this will occassionally fail too when
Lexis Nexis is having too high a load. Since the sources change irregularly it's not too concerning if it fails once and a while
since we'll still have the sources from the previous day.
