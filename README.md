# WebFormMVC

ASP.NET WebForm in VB.NET solution, mixed with a light ASP.NET MVC in C# with Core 2.0 Web API. 
Onion inspired architecture. Loose coupling and clean separation of concern.

### UI.Project1
This project is located under the WebFormMVC web form project to allow access to the /MVC/UI.Project1/Views/.
The WebFormMVC project isolates the root folder structure access only to files and folders underneath it.
In the Global.asax code, the Razor engine is set to allow searching for Views *.cshtml files through /MVC/UI.Project1/Views/.

### REST API
The RestApi [appsettings.json](https://github.com/hhalim/WebFormMVC/blob/master/RestApi/appsettings.json) requires the correct url for CORS operation set, which is already set by default to http://localhost, http://localhost:52595. The UI layer uses direct JavaScript calls into the web api, this cross domain call needs the correct url in the settings file. If this is incorrect, the CORS initial OPTION preflight call will fail.
