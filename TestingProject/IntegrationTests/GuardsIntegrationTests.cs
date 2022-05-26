using Contracts;
using Entities;
using RabbitMQClients;

namespace TestingProject.IntegrationTests;

public class GuardsIntegrationTests
{
    private IGuardService _guardService = null!;
    [OneTimeSetUp]
    public void Setup()
    {
        _guardService = new GuardClient();
    }

    private Guard NewGuard()
    {
        return new Guard
        {
            FirstName = "g-fname",
            LastName = "g-lname",
            Email = "g@mail.com",
            PhoneNumber = "143565656",
            Password = "password",
            Role = "guard"
        };
    }

    [Test]
    [Category("GuardClient-tests")]
    [Ignore("cannot delete created guard")]
    public  void CreateGuard_test()
    {
        Guard g = NewGuard();
        Assert.DoesNotThrow(() => _guardService.CreateGuardAsync(g));
        
        //TODO - delete created guard.
        //cannot delete created guard -cannot retrieve id of the created guard
        // var guards = await _guardService.GetGuardsAsync(1);
        // await _guardService.RemoveGuardAsync(g.Id);
    }
    
    [Test]
    [Category("GuardClient-tests")]
    public async Task UpdateGuard_test()
    {
        Guard g;
        var guards = await _guardService.GetGuardsAsync(1);

        bool wasModified = false;
        if (guards.Count == 0)
        {
            await _guardService.CreateGuardAsync(NewGuard());
            wasModified = true;
        }
        
        g = (await _guardService.GetGuardsAsync(1)).First();

        g.FirstName = "updated";
        await _guardService.UpdateGuardAsync(g);
        Guard updatedFetchedGuard = await _guardService.GetGuardByIdAsync(g.Id);
        
        Assert.True(g.Id==updatedFetchedGuard.Id && g.FirstName.Equals(updatedFetchedGuard.FirstName));

        //rollback changes
        if (wasModified)
        {
            await _guardService.RemoveGuardAsync(g.Id);
        }
    }
}