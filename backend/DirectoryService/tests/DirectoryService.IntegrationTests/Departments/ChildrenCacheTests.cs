using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Departments;

public class ChildrenCacheTests : DirectoryBaseTests
{
    public ChildrenCacheTests(DirectoryTestWebFactory factory)
        : base(factory) { }

    // Повторный GET обслуживается из кэша: изменение БД в обход инвалидации не видно.
    [Fact]
    public async Task GetChildren_SecondCall_ServesStaleFromCacheWhenDbChangedDirectly()
    {
        Department parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, "alpha");

        // Первый GET наполняет кэш: один ребёнок.
        PaginationResponse<GetDepartmentChildrenByParentDto> first = await GetChildren(parent.Id);
        Assert.Equal(1, first.TotalCount);

        // Добавляем второго ребёнка НАПРЯМУЮ в БД (без API → без инвалидации кэша).
        await CreateChildDepartment(parent, "beta");

        // Второй GET приходит из кэша и второго ребёнка не видит.
        PaginationResponse<GetDepartmentChildrenByParentDto> second = await GetChildren(parent.Id);
        Assert.Equal(1, second.TotalCount);
    }

    // После создания ребёнка через API тег сброшен → следующий GET свежий.
    [Fact]
    public async Task GetChildren_AfterCreateViaApi_ReturnsFreshDataBecauseTagInvalidated()
    {
        Department parent = await CreateParentDepartment();
        await CreateChildDepartment(parent, "alpha");

        // Первый GET наполняет кэш: один ребёнок.
        PaginationResponse<GetDepartmentChildrenByParentDto> first = await GetChildren(parent.Id);
        Assert.Equal(1, first.TotalCount);

        // Создаём второго ребёнка ЧЕРЕЗ API — CreateDepartmentHandler гасит тег родителя.
        Guid locationId = await CreateLocation("gammaStreet", "Moscow", "Russia", "gammaOffice");
        var request = new CreateDepartmentRequest("Child2", "child2_id", parent.Id, [locationId]);
        HttpResponseMessage response = await AppHttpClient.PostAsJsonAsync("/departments", request);
        var created = await response.HandleResponseAsync<Guid>();
        Assert.True(created.IsSuccess);

        // Тег сброшен → второй GET видит обоих детей.
        PaginationResponse<GetDepartmentChildrenByParentDto> second = await GetChildren(parent.Id);
        Assert.Equal(2, second.TotalCount);
    }

    private async Task<PaginationResponse<GetDepartmentChildrenByParentDto>> GetChildren(Guid parentId)
    {
        HttpResponseMessage response =
            await AppHttpClient.GetAsync($"/departments/{parentId}/children?Pagination.Page=1&Pagination.PageSize=20");

        Result<PaginationResponse<GetDepartmentChildrenByParentDto>, Errors> result =
            await response.HandleResponseAsync<PaginationResponse<GetDepartmentChildrenByParentDto>>();

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
