# UserCRUD
Aplicaçao com API em .Net Core 3 com CRUD de usuários utilizando gestão de usuários via Identity

Para alterar o local da base de dados, basta inserir a connection string no arquivo AppSettings.json na seçao "ConnectionStrings" e alterar no Startup de "UseInMemoryDatabase" para o conector desejado (ccaso SQL Server, apenas comentar linha 41 e descomentar linha 40) 
