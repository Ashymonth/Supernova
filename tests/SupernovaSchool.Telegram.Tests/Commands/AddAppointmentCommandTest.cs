// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Moq;
// using SupernovaSchool.Abstractions;
// using SupernovaSchool.Models;
// using SupernovaSchool.Telegram.Tests.Helpers;
// using SupernovaSchool.Telegram.Workflows.CreateTeacher;
// using WTelegram;
//
// namespace SupernovaSchool.Telegram.Tests.Commands;
//
//
// public class AddAppointmentCommandTest : BaseCommandTest
// {
//     private const string UnExistedStudentId = "123";
//
//     private readonly Mock<IStudentService> _mock = new();
//     private readonly WebApplicationFactory<Program> _factory;
//     private Client _tgClient = null!;
//
//     public AddAppointmentCommandTest()
//     {
//         _factory = new WebApplicationFactory<Program>()
//             .WithWebHostBuilder(builder =>
//             {
//                 builder.ConfigureAppConfiguration((context, configurationBuilder) =>
//                 {
//                     configurationBuilder.AddJsonFile("appsettings.json")
//                         .AddUserSecrets<BaseCommandTest>();
//
//                     builder.ConfigureServices(collection =>
//                     {
//                         _mock.Setup(studentService => studentService.GetStudentAsync(UnExistedStudentId))
//                             .ReturnsAsync((Student?)null);
//
//                         var service = collection.First(descriptor =>
//                             descriptor.ServiceType == typeof(IStudentService));
//
//                         collection.Remove(service);
//
//                         collection.AddSingleton<IStudentService>(_ => _mock.Object);
//                     });
//                 });
//             });
//     }
//
//     [Fact]
//     public async Task CreateAppointmentAsync_WhenStudentNotRegistered_ShouldReturnErrorMessage()
//     {
//         var expectedMessagesInOrder = new Queue<string>([
//             CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
//         ]);
//
//         var webClient = _factory.CreateClient();
//
//         _tgClient = await WTelegramClientFactory.CreateClient(Config);
//
//         using var locker = new AutoResetEvent(false);
//         // ReSharper disable once AccessToDisposedClosure
//         _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);
//
//         await SendUpdate(webClient, Telegram.Commands.CreateTeacherCommand);
//
//         locker.WaitOne();
//         
//         Assert.True(expectedMessagesInOrder.Count == 0);
//     }
// }