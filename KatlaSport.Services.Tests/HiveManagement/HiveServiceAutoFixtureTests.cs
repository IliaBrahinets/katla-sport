using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using KatlaSport.DataAccess.ProductStoreHive;
using KatlaSport.Services.HiveManagement;
using Moq;
using Xunit;

namespace KatlaSport.Services.Tests.HiveManagement
{
    public class HiveServiceAutoFixtureTests
    {
        static HiveServiceAutoFixtureTests()
        {
            MapperInitializer.Initialize();
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHives_EmptySet_EmptyListReturned(
                                                              [Frozen] Mock<IProductStoreHiveContext> context,
                                                              HiveService service)
        {
            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new StoreHive[0]);

            var result = await service.GetHivesAsync();

            Assert.Empty(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHives_SetWithTenElements_ListWithTenElementsReturned(
                                                                                  [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                  HiveService service,
                                                                                  IFixture fixture)
        {
            fixture.Customize<StoreHive>(c => c.Without(x => x.Sections));
            var input = fixture.CreateMany<StoreHive>(10).ToArray();

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(input);
            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            var result = await service.GetHivesAsync();

            Assert.True(input.Length == result.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHives_HiveAndChainedSection_HiveListItemWithProperlyCountedSections(
                                                                                                 [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                                 HiveService service)
        {
            var hive = new StoreHive
            {
                Id = 1
            };

            var hiveSection = new StoreHiveSection
            {
                Id = 1,
                StoreHive = hive,
                StoreHiveId = 1
            };

            hive.Sections = new[] { hiveSection };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { hive });
            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { hiveSection });

            var result = await service.GetHivesAsync();

            Assert.True(result[0].HiveSectionCount == 1);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_RequestWithUnExistedId_ExceptionThrown(
                                                                         [Frozen] Mock<IProductStoreHiveContext> context,
                                                                         HiveService service)
        {
            var hive = new StoreHive
            {
                Id = 1
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { hive });

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.GetHiveAsync(2));
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHive_RequestWithExistedId_HiveReturned(
                                                                    [Frozen] Mock<IProductStoreHiveContext> context,
                                                                    HiveService service)
        {
            var hive = new StoreHive
            {
                Id = 1
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { hive });

            var resultHive = await service.GetHiveAsync(1);

            Assert.True(hive.Id == resultHive.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHive_CreateHiveWithExistedCode_ExceptionThrown(
                                                                               [Frozen] Mock<IProductStoreHiveContext> context,
                                                                               HiveService service)
        {
            var hive = new StoreHive
            {
                Id = 1,
                Code = "Code"
            };

            var newHive = new UpdateHiveRequest
            {
                Code = hive.Code
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { hive });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.CreateHiveAsync(newHive));
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHive_TryCreateHive_ElementWasAddedToContext(
                                                                         [Frozen] Mock<IProductStoreHiveContext> context,
                                                                         HiveService service)
        {
            var hive = new UpdateHiveRequest
            {
                Code = "Code",
                Name = "Name",
                Address = "Address"
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new StoreHive[0]);

            await service.CreateHiveAsync(hive);

            Assert.True(context.Object.Hives.Count() == 1);
            Assert.Contains(context.Object.Hives, h => h.Code == hive.Code &&
                                                       h.Name == hive.Name &&
                                                       h.Address == hive.Address &&
                                                       h.IsDeleted == false);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHive_TryCreateHive_HiveEntityReturned(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveService service)
        {
            var hive = new UpdateHiveRequest
            {
                Code = "Code",
                Name = "Name",
                Address = "Address"
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new StoreHive[0]);

            var createdHive = await service.CreateHiveAsync(hive);

            Assert.True(
                        createdHive.Name == hive.Name &&
                        createdHive.Code == hive.Code &&
                        createdHive.Address == hive.Address &&
                        createdHive.IsDeleted == false);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_UpdateWithUnExistedId_ExceptionThrown(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveService service)
        {
            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new StoreHive[0]);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.UpdateHiveAsync(1, new UpdateHiveRequest()));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_UpdateHiveToExistedCode_ExceptionThrown(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveService service)
        {
            var existedhive = new StoreHive
            {
                Id = 1,
                Code = "Code"
            };

            var updatedExistedHive = new StoreHive
            {
                Id = 2,
                Code = "Code1"
            };

            var newHive = new UpdateHiveRequest
            {
                Code = "Code"
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { existedhive, updatedExistedHive });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.UpdateHiveAsync(2, newHive));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_TryUpdateHive_HiveUpdated(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveService service)
        {
            var existedhive = new StoreHive
            {
                Id = 1,
                Code = "Code"
            };

            var newHive = new UpdateHiveRequest
            {
                Code = "newCode"
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { existedhive });

            await service.UpdateHiveAsync(existedhive.Id, newHive);

            var updatedHive = context.Object.Hives.First(h => h.Id == existedhive.Id);

            Assert.True(updatedHive.Code == newHive.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHive_TryUpdateHive_UpdatedHiveReturned(
                                                                  [Frozen] Mock<IProductStoreHiveContext> context,
                                                                  HiveService service)
        {
            var existedhive = new StoreHive
            {
                Id = 1,
                Code = "Code"
            };

            var newHive = new UpdateHiveRequest
            {
                Code = "newCode"
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { existedhive });

            var updatedHive = await service.UpdateHiveAsync(existedhive.Id, newHive);

            Assert.True(updatedHive.Code == newHive.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_DeletingHiveWithUnexistedId_ExceptionThrown(
                                                                                     [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                     HiveService service)
        {
            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new StoreHive[0]);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.DeleteHiveAsync(1));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_DeletingHiveWithUndeletedStatus_ExceptionThrown(
                                                                                     [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                     HiveService service)
        {
            var hive = new StoreHive
            {
                Id = 1,
                IsDeleted = false
            };

            context.Setup(c => c.Hives).ReturnsAsyncEntitySet(new[] { hive });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.DeleteHiveAsync(hive.Id));
        }
    }
}
