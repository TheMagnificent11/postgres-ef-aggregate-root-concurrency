namespace Pizzeria.Store.Api.Domain;

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

    public uint Version { get; protected set; }
}
