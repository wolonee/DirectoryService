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
        
        var department_l1 = await CreateChildDepartment(mainParentDepartment, "left1");
        var department_r1 = await CreateChildDepartment(mainParentDepartment, "right1"); // переносим этого
        var department_r11 = await CreateChildDepartment(department_r1, "right11");
        var department_r12 = await CreateChildDepartment(department_r1, "right12");

        var result = await ExecuteHandler(async handler =>
        {
            var command = new UpdateParentCommand(department_r1.Id, new UpdateParentRequest(department_l1.Id));

            return await handler.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            var goalDepartment = await dbContext.Departments
                .Include(d => d.ChildrenDepartments)
                .FirstAsync(d => d.Id == department_l1.Id, cancellationToken);
            
            var parentDepartment = await dbContext.Departments
                .Include(d => d.ChildrenDepartments)
                .FirstAsync(d => d.Id == mainParentDepartment.Id, cancellationToken);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(department_r1, goalDepartment.ChildrenDepartments);
            Assert.Contains(department_r11, goalDepartment.ChildrenDepartments);
            Assert.Contains(department_r12, goalDepartment.ChildrenDepartments);
            Assert.DoesNotContain(department_r1, parentDepartment.ChildrenDepartments);
        });

    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateParentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<UpdateParentHandler>();
        return await action(handler);
    }
}