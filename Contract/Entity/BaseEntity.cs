namespace MITCRMS.Contract.Entity
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateCreated { get; set; }= DateTime.UtcNow;

    }
}
