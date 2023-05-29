using Microsoft.AspNetCore;
using System;
using Xunit;
using web_server;
using Microsoft.AspNetCore.Hosting;
using web_server.Services.Interfaces;
using System.Threading.Tasks;

namespace web_server.Tests
{
    public class GetLiteUserByIdUnitTest
    {
        private DependencyResolverHelper _serviceProvider;

        public GetLiteUserByIdUnitTest()
        {

            var webHost = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .Build();
            _serviceProvider = new DependencyResolverHelper(webHost);
        }



        [Fact]
        public async Task SuccessedGetLiteUserByIdNumber2()
        {
            // Arrage
            var service = _serviceProvider.GetService<IAuthService>();
            var expected = @"{""success"":true,""result"":{""Wish"":null,""StartWaitPayment"":""0001-01-01T00:00:00"",""WasFirstPayment"":true,""FirstLogin"":true,""LessonsCount"":1,""SkippedInThisMonth"":0,""Money"":[],""Credit"":[],""UsedTrial"":false,""UserId"":2,""Password"":""1234567"",""FirstName"":""Сергей"",""PhotoUrl"":""/content/avatars/students/Сергей Воронов.jpg"",""MiddleName"":""Андреевич"",""LastName"":""Воронов"",""NotificationTokens"":[],""Notifications"":[],""BirthDate"":""1999-05-12T00:00:00"",""Email"":""sergey_voronov@mail.ru"",""Phone"":""+79188793839"",""Role"":""Student"",""ActiveToken"":null,""BalanceHistory"":[],""Balance"":0.0,""Schedules"":[],""Chat"":null,""Courses"":null,""About"":null,""UserDates"":[]}}";

            // Act
            var result = await service.GetLiteUserById("2");

            // Assert
            Assert.Equal(expected, result);
        }


        [Fact]
        public async Task ErrorGetLiteUserByIdNumber2BecauseDataBaseError()
        {
            // Arrage
            var service = _serviceProvider.GetService<IAuthService>();
            var expected = @"{""success"":false,""result"":""Возникла непредвиденная ошибка с источником данных.""}";

            // Act
            var result = await service.GetLiteUserById("99999999999999");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ErrorGetLiteUserByIdNumber2BecauseDontHaveThisId()
        {
            // Arrage
            var service = _serviceProvider.GetService<IAuthService>();
            var expected = @"{""success"":false,""result"":""Пользователь с таким ID не найден.""}";

            // Act
            var result = await service.GetLiteUserById("9999999");

            // Assert
            Assert.Equal(expected, result);
        }
     
        [Fact]
        public async Task ErrorGetLiteUserByIdNumber2BecauseEmptyId()
        {
            // Arrage
            var service = _serviceProvider.GetService<IAuthService>();
            var expected = @"{""success"":false,""result"":""Некорректный ID пользователя.""}";

            // Act
            var result = await service.GetLiteUserById("");

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
