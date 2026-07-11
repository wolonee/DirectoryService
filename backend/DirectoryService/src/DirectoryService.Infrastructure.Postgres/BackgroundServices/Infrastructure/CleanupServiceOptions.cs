namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupServiceOptions
{
    public bool Enabled { get; set; }
    
    public int IntervalHours { get; set; }
    
    public int RetentionDays { get; set; }
    
    public int BatchSize { get; set; }
}