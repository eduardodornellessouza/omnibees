1.SL ---> OB.REST.Services - OB API RESTful services (WEB API)
2.BL ---> OB.BL.Contracts - project for the Data Transfer Objects (DTOs) exposed by the REST services in OB.REST.Services
          OB.BL.Operations - implementation of the business layer and the OB.REST.Services controller actions.
3.DM ---> Domain Model (OB.Domain)
4.DL ---> DataAccessLayer 
	  OB.DL.Common  - DataAccessLayer infrastructure that supports the Repository and UnitOfWork patterns and cache stuff
	  OB.Log - Logging support
	  OB.DL.Model.* - EntityFramework models per OB module
5.TL ---> Test projects (with same name as the project appended with '.Test' suffix)
	  OB.BL.Operations.Test
          OB.REST.Services.Test
           