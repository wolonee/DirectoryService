using System.Net.Http.Json;
using DirectoryService.Application.Departments.UpdateParent;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class UpdateParentTests : DirectoryBaseTests
{
    public UpdateParentTests(DirectoryTestWebFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Update_parent_with_childrens_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb"); // переносим этого
        var department_r11 = await CreateChildDepartment(department_r1, "ccc");
        var department_r12 = await CreateChildDepartment(department_r1, "ddd");
        
        var request = new UpdateParentRequest(department_l1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        
        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfparentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.Contains(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r11.Id, childrenListOfDepartment_r1);
            Assert.Contains(department_r12.Id, childrenListOfDepartment_r1);
            Assert.DoesNotContain(department_r1.Id, childrenListOfparentDepartment);
        });
    }
    
    [Fact]
    public async Task Update_parent_without_childrens_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb");
        
        var request = new UpdateParentRequest(department_l1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.Contains(department_r1.Id, childrenListOfDepartment_l1);
            Assert.DoesNotContain(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Update_parent_with_childrens_to_root_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(root, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb");
        var department_r111 = await CreateChildDepartment(department_r11, "ccc"); 
        var department_r112 = await CreateChildDepartment(department_r11, "ddd");
        
        var request = new UpdateParentRequest(root.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r11.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);            
            
            var childrenListOfDepartment_r11 = await dbContext.Departments
                .Where(d => d.Id == department_r11.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfparentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.Contains(department_r11.Id, childrenListOfparentDepartment);
            Assert.DoesNotContain(department_r11.Id, childrenListOfDepartment_r1);
            Assert.Contains(department_r111.Id, childrenListOfDepartment_r11);
            Assert.Contains(department_r112.Id, childrenListOfDepartment_r11);
        });
    }
    
    [Fact]
    public async Task Update_parent_without_childrens_to_root_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(root, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb"); 
        
        var request = new UpdateParentRequest(root.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r11.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);            
            
            var childrenListOfparentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.Contains(department_r11.Id, childrenListOfparentDepartment);
            Assert.DoesNotContain(department_r11.Id, childrenListOfDepartment_r1);
        });
    }
    
    [Fact]
    public async Task Invalid_parent_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment(); // active: false
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb");
        
        var request = new UpdateParentRequest(Guid.NewGuid());

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Parent_not_active_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa", active: false);
        var department_r1 = await CreateChildDepartment(root, "bbb");
        
        var request = new UpdateParentRequest(department_l1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        
        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Child_not_active_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb", active: false); // переносим этого

        var request = new UpdateParentRequest(department_l1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        
        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }

    [Fact]
    public async Task Invalid_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb");
        
        var request = new UpdateParentRequest(department_l1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{Guid.NewGuid()}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Parent_equal_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(root, "aaa");
        var department_r1 = await CreateChildDepartment(root, "bbb"); // переносим этого
        
        var request = new UpdateParentRequest(department_r1.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Update_parent_equal_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var root = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(root, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb");
        
        var request = new UpdateParentRequest(department_r11.Id);

        var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{department_r1.Id}/parent", request, cancellationToken);

        var result = await response.HandleResponseAsync(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Contains(result.Error, e => e.Type == ErrorType.FAILURE);

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == root.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.Contains(department_r11.Id, childrenListOfDepartment_r1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Concurrent_update_parent_for_same_department_should_apply_only_one_operation()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var rootDepartment = await CreateParentDepartment();
        
        var alternativeParentDepartment = await CreateChildDepartment(rootDepartment, "alternative_parent");
        
        var departmentToMove = await CreateChildDepartment(rootDepartment, "department_to_move");
        var child1 = await CreateChildDepartment(departmentToMove, "child_1");
        var child2 = await CreateChildDepartment(departmentToMove, "child_2");
        
        var task1 = Task.Run(async () =>
        {
            var request = new UpdateParentRequest(alternativeParentDepartment.Id);
            var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentToMove.Id}/parent", request, cancellationToken);
            return await response.HandleResponseAsync(cancellationToken: cancellationToken);
        });
    
        var task2 = Task.Run(async () =>
        {
            var request = new UpdateParentRequest(null);
            var response = await AppHttpClient.PutAsJsonAsync($"/api/departments/{departmentToMove.Id}/parent", request, cancellationToken);
            return await response.HandleResponseAsync(cancellationToken: cancellationToken);
        });
    
        var results = await Task.WhenAll(task1, task2);
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var successCount = results.Count(r => r.IsSuccess);
            var failureCount = results.Count(r => r.IsFailure);

            Assert.True(successCount == 1 && failureCount == 1);
            
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id);
            
            bool isInRoot = movedDepartment.ParentId == null;
            bool isInAlternativeParent = movedDepartment.ParentId == alternativeParentDepartment.Id;

            Assert.True(isInRoot || isInAlternativeParent);
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdateParentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateParentHandler>();
        return await action(handler);
    }
}