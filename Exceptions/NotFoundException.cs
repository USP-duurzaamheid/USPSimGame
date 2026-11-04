namespace USPSimGame.Exceptions;

public class NotFoundException : USPSimGameException
{
    public string EntityName { get; }
    public object Id { get; }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID {id} was not found.")
    {
        EntityName = entityName;
        Id = id;
    }
}
