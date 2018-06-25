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
    public class HiveSectionServiceAutoFixtureTests
    {
        static HiveSectionServiceAutoFixtureTests()
        {
            MapperInitializer.Initialize();
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSections_EmptySet_EmptyListReturned(
                                                              [Frozen] Mock<IProductStoreHiveContext> context,
                                                              HiveSectionService service)
        {
            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            var result = await service.GetHiveSectionsAsync();

            Assert.Empty(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSections_SetWithTenElements_ListWithTenElementsReturned(
                                                                                  [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                  HiveSectionService service,
                                                                                  IFixture fixture)
        {
            fixture.Customize<StoreHiveSection>(c => c.Without(x => x.StoreHive)
                                                      .Without(x => x.Items)
                                                      .Without(x => x.Categories));
            var input = fixture.CreateMany<StoreHiveSection>(10).ToArray();

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(input);

            var result = await service.GetHiveSectionsAsync();

            Assert.True(input.Length == result.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSections_SetWithTenHiveSectionsAndHiveId_ListWithFilteredHiveSectionsReturned(
                                                                                  [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                  HiveSectionService service,
                                                                                  IFixture fixture)
        {
            var firstHive = new StoreHive
            {
                Id = 1
            };
            string firstHiveName = "FirstHive";
            fixture.Customize<StoreHiveSection>(c => c.Without(x => x.StoreHive)
                                                      .Without(x => x.Items)
                                                      .Without(x => x.Categories)
                                                      .With(x => x.Name, firstHiveName)
                                                      .With(x => x.StoreHiveId, firstHive.Id)
                                                      .With(x => x.StoreHive, firstHive));
            var chainedToFirstHive = fixture.CreateMany<StoreHiveSection>(5).ToArray();

            var secondHive = new StoreHive
            {
                Id = 2
            };
            string secondHiveName = "SecondHive";
            fixture.Customize<StoreHiveSection>(c => c.Without(x => x.StoreHive)
                                                      .Without(x => x.Items)
                                                      .Without(x => x.Categories)
                                                      .With(x => x.Name, secondHiveName)
                                                      .With(x => x.StoreHiveId, secondHive.Id)
                                                      .With(x => x.StoreHive, secondHive));
            var chainedToSecondHive = fixture.CreateMany<StoreHiveSection>(5).ToArray();

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(chainedToFirstHive.Concat(chainedToSecondHive).ToArray());

            var result = await service.GetHiveSectionsAsync(firstHive.Id);

            Assert.True(chainedToSecondHive.Length == result.Count);
            Assert.All(result, h => h.Name.Equals(secondHiveName));
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_RequestWithUnExistedId_ExceptionThrown(
                                                                         [Frozen] Mock<IProductStoreHiveContext> context,
                                                                         HiveSectionService service)
        {
            var hiveSection = new StoreHiveSection
            {
                Id = 1
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { hiveSection });

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.GetHiveSectionAsync(2));
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSection_RequestWithExistedId_HiveSectionReturned(
                                                                    [Frozen] Mock<IProductStoreHiveContext> context,
                                                                    HiveSectionService service)
        {
            var hiveSection = new StoreHiveSection
            {
                Id = 1
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { hiveSection });

            var resultHive = await service.GetHiveSectionAsync(1);

            Assert.True(hiveSection.Id == resultHive.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSection_CreateHiveSectionWithExistedCode_ExceptionThrown(
                                                                               [Frozen] Mock<IProductStoreHiveContext> context,
                                                                               HiveSectionService service)
        {
            var hiveSection = new StoreHiveSection
            {
                Id = 1,
                Code = "Code"
            };

            var newHiveSection = new UpdateHiveSectionRequest
            {
                Code = hiveSection.Code
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { hiveSection });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.CreateHiveSectionAsync(newHiveSection));
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSection_TryCreateHiveSection_ElementWasAddedToContext(
                                                                         [Frozen] Mock<IProductStoreHiveContext> context,
                                                                         HiveSectionService service)
        {
            var hiveSection = new UpdateHiveSectionRequest
            {
                Code = "Code",
                Name = "Name"
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            await service.CreateHiveSectionAsync(hiveSection);

            Assert.True(context.Object.Sections.Count() == 1);
            Assert.Contains(context.Object.Sections, h => h.Code == hiveSection.Code &&
                                                          h.Name == hiveSection.Name &&
                                                          h.IsDeleted == false);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSection_TryCreateHiveSection_HiveSectionEntityReturned(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveSectionService service)
        {
            var hiveSection = new UpdateHiveSectionRequest
            {
                Code = "Code",
                Name = "Name"
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            var createdHiveSection = await service.CreateHiveSectionAsync(hiveSection);

            Assert.True(
                        createdHiveSection.Name == hiveSection.Name &&
                        createdHiveSection.Code == hiveSection.Code &&
                        createdHiveSection.IsDeleted == false);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_UpdateWithUnExistedId_ExceptionThrown(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveSectionService service)
        {
            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.UpdateHiveSectionAsync(1, new UpdateHiveSectionRequest()));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_UpdateHiveSectionWithExistedCode_ExceptionThrown(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveSectionService service)
        {
            var existedHiveSection = new StoreHiveSection
            {
                Id = 1,
                Code = "Code"
            };

            var updatedExistedHiveSection = new StoreHiveSection
            {
                Id = 2,
                Code = "Code1"
            };

            var newHiveSection = new UpdateHiveSectionRequest
            {
                Code = "Code"
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { existedHiveSection, updatedExistedHiveSection });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.UpdateHiveSectionAsync(updatedExistedHiveSection.Id, newHiveSection));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_TryUpdateHiveSection_HiveSectionUpdated(
                                                                   [Frozen] Mock<IProductStoreHiveContext> context,
                                                                   HiveSectionService service)
        {
            var existedHiveSection = new StoreHiveSection
            {
                Id = 1,
                Code = "Code"
            };

            var newHiveSection = new UpdateHiveSectionRequest
            {
                Code = "newCode"
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { existedHiveSection });

            await service.UpdateHiveSectionAsync(existedHiveSection.Id, newHiveSection);

            var updatedHiveSection = context.Object.Sections.First(h => h.Id == existedHiveSection.Id);

            Assert.True(updatedHiveSection.Code == newHiveSection.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveSection_TryUpdateHiveSection_UpdatedHiveSectionReturned(
                                                                  [Frozen] Mock<IProductStoreHiveContext> context,
                                                                  HiveSectionService service)
        {
            var existedHiveSection = new StoreHiveSection
            {
                Id = 1,
                Code = "Code"
            };

            var newHiveSection = new UpdateHiveSectionRequest
            {
                Code = "newCode"
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { existedHiveSection });

            var updatedHiveSection = await service.UpdateHiveSectionAsync(existedHiveSection.Id, newHiveSection);

            Assert.True(updatedHiveSection.Code == newHiveSection.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSection_DeletingHiveSectionWithUnexistedId_ExceptionThrown(
                                                                                     [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                     HiveSectionService service)
        {
            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new StoreHiveSection[0]);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(async () => await service.DeleteHiveSectionAsync(1));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHive_DeletingHiveWithUndeletedStatus_ExceptionThrown(
                                                                                     [Frozen] Mock<IProductStoreHiveContext> context,
                                                                                     HiveSectionService service)
        {
            var hiveSection = new StoreHiveSection
            {
                Id = 1,
                IsDeleted = false
            };

            context.Setup(c => c.Sections).ReturnsAsyncEntitySet(new[] { hiveSection });

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(async () => await service.DeleteHiveSectionAsync(hiveSection.Id));
        }
    }
}
