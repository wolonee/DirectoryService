using DirectoryService.Application.Departments.UpdateParent;
using DirectoryService.Contracts.Departments;
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

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого
        var department_r11 = await CreateChildDepartment(department_r1, "ccc");
        var department_r12 = await CreateChildDepartment(department_r1, "ddd");

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

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
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);
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

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(department_r1.Id, childrenListOfDepartment_l1);
            Assert.DoesNotContain(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Update_parent_with_childrens_to_root_should_succeed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb"); // переносим этого
        var department_r111 = await CreateChildDepartment(department_r11, "ccc"); 
        var department_r112 = await CreateChildDepartment(department_r11, "ddd");

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r11.Id, new UpdateParentRequest(mainParentDepartment.Id));

            return await handler.Handle(command, cancellationToken);
        });

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
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);
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

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb"); 

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r11.Id, new UpdateParentRequest(mainParentDepartment.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);            
            
            var childrenListOfparentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(department_r11.Id, childrenListOfparentDepartment);
            Assert.DoesNotContain(department_r11.Id, childrenListOfDepartment_r1);
        });
    }
    
    [Fact]
    public async Task Invalid_parent_active_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment(active: false);
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(Guid.NewGuid()));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Parent_not_active_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa", active: false);
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Child_not_active_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb", active: false); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }

    [Fact]
    public async Task Invalid_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(Guid.NewGuid(), new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Parent_equal_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "bbb"); // переносим этого

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_r1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_l1 = await dbContext.Departments
                .Where(d => d.Id == department_l1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
            Assert.DoesNotContain(department_r1.Id, childrenListOfDepartment_l1);
            Assert.Contains(department_r1.Id, childrenListOfParentDepartment);
        });
    }
    
    [Fact]
    public async Task Update_parent_equal_child_should_failed()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var mainParentDepartment = await CreateParentDepartment();
        
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "aaa");
        var department_r11 = await CreateChildDepartment(department_r1, "bbb");

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_r11.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var childrenListOfDepartment_r1 = await dbContext.Departments
                .Where(d => d.Id == department_r1.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            var childrenListOfParentDepartment = await dbContext.Departments
                .Where(d => d.Id == mainParentDepartment.Id)
                .Include(d => d.ChildrenDepartments)
                .SelectMany(d => d.ChildrenDepartments.Select(c => c.Id))
                .ToListAsync(cancellationToken);
            
            Assert.True(result.IsFailure);
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
        
        var task1 = ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(departmentToMove.Id, new UpdateParentRequest(alternativeParentDepartment.Id));
            return await handler.Handle(command, cancellationToken);
        });
        
        var task2 = ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(departmentToMove.Id, new UpdateParentRequest(null));
            return await handler.Handle(command, cancellationToken);
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