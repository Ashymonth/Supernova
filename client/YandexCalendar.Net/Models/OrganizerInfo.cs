namespace YandexCalendar.Net.Models;

public record OrganizerInfo
{
    public OrganizerInfo(string organizerId, string organizerName)
    {
        ArgumentNullException.ThrowIfNull(organizerId);
        ArgumentNullException.ThrowIfNull(organizerName);
        
        OrganizerId = organizerId;
        OrganizerName = organizerName;
    }

    public string OrganizerId { get; set; }

    public string OrganizerName { get; set; }

}