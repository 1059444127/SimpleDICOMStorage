Requires .NET 4.0

****
NOTE for previous users:  uninstall the previous version by running command prompt
as admin then executing 

	sc stop SimpleDICOMStorageService
	sc delete SimpleDICOMStorageService

to delete the service.

Make sure you update the config file .. there are new required schema elements.
****

Run setup.exe as administrator.  The service is installed but not started.

The app.Config file contains a key

   <add key="ConfigurationFile" value="SampleConfiguration.xml"/>

that makes the service look for the config file in the same directory as the binary. 

If you change this key move the config file to the new location.

By default the service runs as LOCAL SERVICE.  You can change this in the service admin.

You **MUST** set permissions on the configuration file directory to allow LOCAL SERVICE to read the file.
