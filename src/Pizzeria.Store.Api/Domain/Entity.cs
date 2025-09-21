namespace Pizzeria.Store.Api.Domain;

public abstract class Entity
{
    protected Entity()
        : this(Guid.NewGuid())
    {
    }

    protected Entity(Guid id)
        : base()
    {
        this.Id = id;

        // Note audit fields should be populated by EF save changes interceptor
        this.CreatedBy = "System";
        this.ModifiedBy = "System";
        this.CreatedAtUtc = DateTime.UtcNow;
        this.ModifiedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public string CreatedBy { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string ModifiedBy { get; private set; }

    public DateTime ModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (this.GetType() != other.GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (this.GetType().ToString() + this.Id.ToString())
            .GetHashCode();
    }

    public void Delete()
    {
        if (this.IsDeleted)
        {
            return;
        }

        this.IsDeleted = true;
    }

    public void Undelete()
    {
        if (!this.IsDeleted)
        {
            return;
        }

        this.IsDeleted = false;
    }

    public void ApplyCreationTrackingData(string? createdBy)
    {
        this.CreatedBy = createdBy ?? "System";
        this.CreatedAtUtc = DateTime.UtcNow;
        this.ApplyModificationTrackingData(createdBy);
    }

    public void ApplyModificationTrackingData(string? modifiedBy)
    {
        this.ModifiedBy = modifiedBy ?? "System";
        this.ModifiedAtUtc = DateTime.UtcNow;
    }
}
