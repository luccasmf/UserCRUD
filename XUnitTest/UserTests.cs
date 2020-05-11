using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UserCRUD;
using UserCRUD.Configurations;
using UserCRUD.ViewModels;
using Xunit;

namespace XUnitTest
{
    public class UserTests
    {
        private readonly HttpClient _client;
        public UserTests()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>()
                );
            _client = server.CreateClient();
        }


        [Theory]
        [InlineData("UserDelete")]
        public async Task TestDeleteUser(string userName)
        {
            await CreateUser(userName);
            var request = new HttpRequestMessage(new HttpMethod("DELETE"), $"/api/Account/DeleteUser?userName={userName}");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();


            ObjectResult result = JsonConvert.DeserializeObject<ObjectResult>(response.Content.ReadAsStringAsync().Result);

            Assert.True(result.Success);
        }

        [Theory]
        [InlineData("UserLogin")]
        public async Task TestLogin(string userName)
        {
            await CreateUser(userName);

            var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/Account/Login");

            LoginViewModel userLogin = new LoginViewModel
            {
                UserName = userName,
                Password = "123456"
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();


            ObjectResult result = JsonConvert.DeserializeObject<ObjectResult>(response.Content.ReadAsStringAsync().Result);

            response.EnsureSuccessStatusCode();
            Assert.True(result.Success);
        }

        [Theory]
        [InlineData("UserChangePWD")]
        public async Task TestChangePassword(string userName)
        {
            await CreateUser(userName);

            var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/Account/Login");

            LoginViewModel userLogin = new LoginViewModel
            {
                UserName = userName,
                Password = "123456"
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            ObjectDataResult<JWTToken> result = JsonConvert.DeserializeObject<ObjectDataResult<JWTToken>>(response.Content.ReadAsStringAsync().Result);

            var request2 = new HttpRequestMessage(new HttpMethod("POST"), "/api/Account/ChangePassword");
            UpdatePasswordViewModel pwdUpdate = new UpdatePasswordViewModel
            {
                OldPassword = "123456",
                NewPassword = "ABC123"
            };

            request2.Content = new StringContent(JsonConvert.SerializeObject(pwdUpdate), Encoding.UTF8, "application/json");
            request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $"{result.Data.AccessToken}");


            response = await _client.SendAsync(request2);
            response.EnsureSuccessStatusCode();

            ObjectResult changePwdResult = JsonConvert.DeserializeObject<ObjectResult>(response.Content.ReadAsStringAsync().Result);


            Assert.True(changePwdResult.Success);
        }

        [Theory]
        [InlineData("UserGet")]
        private async Task TestGetUser(string userName)
        {
            await CreateUser(userName);

            var request = new HttpRequestMessage(new HttpMethod("GET"), $"/api/Account/GetUser?userName={userName}");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            User result = JsonConvert.DeserializeObject<User>(response.Content.ReadAsStringAsync().Result);

           
            Assert.Equal(userName,result.UserName);
        }

        [Theory]
        [InlineData("POST")]
        public async Task TestCreateEmpyUser(string method)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), "/api/Account/Register");

            RegisterUserViewModel userToAdd = new RegisterUserViewModel();
            
            request.Content = new StringContent(JsonConvert.SerializeObject(userToAdd), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }


        //metodo padrao de criar usuario
        private async Task<ObjectResult> CreateUser(string userName)
        {
            var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/Account/Register");

            RegisterUserViewModel userToAdd = new RegisterUserViewModel
            {
                Email = "teste@teste.com",
                Password = "123456",
                UserName = userName
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(userToAdd), Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();


            ObjectResult result = JsonConvert.DeserializeObject<ObjectResult>(response.Content.ReadAsStringAsync().Result);

            response.EnsureSuccessStatusCode();
            // Assert.True(result.Success);
            return result;
            //Assert.True(await TestDeleteUser("UserTeste"));
        }
    }
}
