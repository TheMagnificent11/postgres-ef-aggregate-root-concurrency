namespace Pizzeria.Store.Domain;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot()
        : base(Guid.NewGuid())
    {
    }

    /// <param name="id">Entity ID</param>
    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    // Note: Version property removed as per requirements
}