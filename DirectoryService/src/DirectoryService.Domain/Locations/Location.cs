namespace DirectoryService.Domain;

public class Location
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; } // Типо: офисное здание PLAZA
    
    public string Contry { get; private set; }
    
    public string City { get; private set; }
    
    public string Address { get; private set; }
}