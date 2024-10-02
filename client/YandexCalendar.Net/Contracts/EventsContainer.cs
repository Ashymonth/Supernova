using System.Xml.Serialization;

namespace YandexCalendar.Net.Contracts;

[XmlRoot(ElementName = "multistatus", Namespace = "DAV:")]
public class CalendarResponse
{
    [XmlElement(ElementName = "response", Namespace = "DAV:")]
    public List<CalendarItem> CalendarItems { get; set; }
}

public class CalendarItem
{
    [XmlElement(ElementName = "href", Namespace = "DAV:")]
    public string Href { get; set; }

    [XmlElement(ElementName = "propstat", Namespace = "DAV:")]
    public PropertyStatus PropertyStatus { get; set; }
}

public class PropertyStatus
{
    [XmlElement(ElementName = "prop", Namespace = "DAV:")]
    public CalendarProperties CalendarProperties { get; set; }

    [XmlElement(ElementName = "status", Namespace = "DAV:")]
    public string Status { get; set; }
}

public class CalendarProperties
{
    [XmlElement(ElementName = "getetag", Namespace = "DAV:")]
    public string Etag { get; set; }

    [XmlElement(ElementName = "calendar-data", Namespace = "urn:ietf:params:xml:ns:caldav")]
    public string CalendarEventData { get; set; }
}