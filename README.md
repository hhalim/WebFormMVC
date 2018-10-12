# WebFormMVC

ASP.NET WebForm in VB.NET solution, mixed with a light ASP.NET MVC in C# with Core 2.0 Web API. 
Onion inspired architecture. Loose coupling and clean separation of concern.

### UI.Project1
This project is located under the WebFormMVC web form project to allow access to the /MVC/UI.Project1/Views/.
The WebFormMVC project isolates the root folder structure access only to files and folders underneath it.
In the Global.asax code, the Razor engine is set to allow searching for Views *.cshtml files through /MVC/UI.Project1/Views/.