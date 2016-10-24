# FHIR-Server
This project was created as part of the work done for the thesis "Development of a Telehealth Framework for an Android Platform", by Nicholas Ho.

This is the server component of the project, containing an implementation of a server offering a FHIR REST API.

To run:
1) Clone/download a copy of of this repository
2) Open the .sln file using Visual Studio 2015
3) Build the project.
	a) If there are any errors during the building process, close VS2015, navigate to the directory containing the source files and delete the "packages" folder (if present)
4) Start the server by clicking on the Start button in VS, using any attached browser.
5) The server is ready for taking requests when the default ASP.NET homepage shows up.
6) An SQL Server Express database will be generated for you, along with 3 sample seeded data for each resource.
6) Use Postman (https://chrome.google.com/webstore/detail/postman/fhbjgbiflinjbdggehcddcbncdddomop?hl=en) to send requests to the server.
7) Unit tests for the server are located in the contents of this project as well.