# 061_PermisionBasedAuthorization_DevCreed

- DevCreed: Permission Based Authorization In .Net 5 (Core) 
	- https://www.youtube.com/watch?v=slTben1Djz0&list=PL62tSREI9C-fGaDsCUvu5OaPWrv-mMzy-
	
		- V002 Crear aplicación ASP.NET Web Application(.NET Framework)
			- Carpeta relacionada 
				061_PermisionBasedAuthorization_DevCreed\NET50
				
			- ASP.NET Core Web App (Model-View_Controller)
			- Nombre:
				- Project: PBaseWebADotNet5.Web
				- Solution: PBaseWebADotNet5
			- Tipo:
				- .NET 5.0
				- Authentication Type: Individual Accounts
				- Configure for HTPPS
				- Enable Razor runtime compilation
				
		- Agregar conectionString
			```cs
				{
				  "ConnectionStrings": {
					"DefaultConnection": "Server=localhost;Database=db_DebCreed;User ID=sa;Password=123456;MultipleActiveResultSets=true",
				  },
				  ....
				}
			```