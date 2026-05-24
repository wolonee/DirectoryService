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

    private async Task<T> ExecuteHandler<T>(Func<UpdateParentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateParentHandler>();
        return await action(handler);
    }
}