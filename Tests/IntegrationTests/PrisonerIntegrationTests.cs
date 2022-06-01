using Contracts;
using Entities;
using RabbitMqClients;

namespace TestingProject.IntegrationTests;

public class PrisonerIntegrationTests
{
    private IPrisonerService _prisonerService = null!;
    private ISectorService _sectorService = null!;
    [OneTimeSetUp]
    public void Setup()
    {
        _prisonerService = new PrisonerClient();
        _sectorService = new SectorClient();
    }

    private async Task<Prisoner> NewPrisoner()
    {
        return new Prisoner
        {
            FirstName = "Firstname",
            LastName = "Lastname",
            Sector = (await _sectorService.GetSectorsAsync()).FirstOrDefault(),
            Ssn = 12324354,
            CrimeCommitted = "crime committed",
            EntryDate = DateTime.Now,
            ReleaseDate = DateTime.Now,
            Notes = new List<Note>()
        };
    }

    [Test]
    [Category("PrisonerClient-tests")]
    public async Task CreatePrisoner_test()
    {
        Prisoner p = await NewPrisoner();
        Assert.DoesNotThrow(() => _prisonerService.CreatePrisonerAsync(p));
        p = await _prisonerService.GetPrisonerBySsnAsync(p.Ssn.ToString());
        await _prisonerService.RemovePrisonerAsync(p.Id);
    }
    
    [Test]
    [Category("PrisonerClient-tests")]
    public async Task GetPrisonerBySsn_test()
    {
        var prisoners = await _prisonerService.GetPrisonersAsync(1, 1);
        if (prisoners==null || prisoners.Count == 0)
        {
            Assert.Pass();
        }
        else
        {
            Prisoner p = await _prisonerService.GetPrisonerBySsnAsync(prisoners.First().Ssn.ToString());
            Assert.That(p.Ssn, Is.EqualTo(prisoners.First().Ssn));
        }
    }
    
    [Test]
    [Category("PrisonerClient-tests")]
    [TestCase(1,5)]
    [TestCase(1,10)]
    [TestCase(2,5)]
    [TestCase(2,10)]
    public async Task GetPrisoners_test(int pageNumber, int pageSize)
    {
        var prisoners = await _prisonerService.GetPrisonersAsync(pageNumber, pageSize);
        if (prisoners==null || prisoners.Count == 0)
        {
            Assert.Pass();
        }
        else
        {
            Assert.That(prisoners.Count <= pageSize);
        }
    }
    
    [Test]
    [Category("PrisonerClient-tests")]
    public async Task UpdatePrisoner_test()
    {
        var prisoners = await _prisonerService.GetPrisonersAsync(1, 1);
        bool wasChanged = false;
        if (prisoners==null || prisoners.Count == 0)
        {
            await _prisonerService.CreatePrisonerAsync(await NewPrisoner());
            wasChanged = true;
        }
        
        Prisoner p = ((await _prisonerService.GetPrisonersAsync(1,1))!).First();
        
        //update
        p.FirstName = "updated";
        await _prisonerService.UpdatePrisonerAsync(p);
        Prisoner updatedFetchedPrisoner = await _prisonerService.GetPrisonerByIdAsync(p.Id);
        
        Assert.That(p.Id==updatedFetchedPrisoner.Id && p.FirstName.Equals(updatedFetchedPrisoner.FirstName));

        //rollback changes
        if (wasChanged)
        {
            await _prisonerService.RemovePrisonerAsync(p.Id);
        }
    }
}