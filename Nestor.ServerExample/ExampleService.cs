using Nestor.Shared;

namespace Nestor.ServerExample;

public class ExampleService : IExampleService
{
    private readonly ExampleDbContext _dbContext;

    public ExampleService(ExampleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void CreateExample(Example example)
    {
        Example.AddExamples(_dbContext, "Example", example);
        _dbContext.SaveChanges();
    }
}
