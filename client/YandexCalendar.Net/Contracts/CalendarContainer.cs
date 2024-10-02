using System.Xml.Serialization;

namespace YandexCalendar.Net.Contracts;

[XmlRoot(ElementName = "multistatus", Namespace = "DAV:")]
public class CalendarContainer
{
    [XmlElement(ElementName = "response")] public List<Response> Responses { get; set; } = null!;
}

public class Response
{
    [XmlElement(ElementName = "href", Namespace = "DAV:")]
    public string Href { get; set; } = null!;

    [XmlElement(ElementName = "propstat", Namespace = "DAV:")]
    public List<Propstat> Propstats { get; set; } = [];
}

public class Propstat
{
    [XmlElement(ElementName = "prop", Namespace = "DAV:")]
    public Prop Prop { get; set; } = null!;

    [XmlElement(ElementName = "status", Namespace = "DAV:")]
    public string Status { get; set; } = null!;
}

public class Prop
{
    [XmlElement(ElementName = "displayname", Namespace = "DAV:")]
    public string DisplayName { get; set; } = null!;

    [XmlElement(ElementName = "calendar-description", Namespace = "urn:ietf:params:xml:ns:caldav")]
    public string CalendarDescription { get; set; } = null!;

    [XmlElement(ElementName = "supported-calendar-component-set", Namespace = "urn:ietf:params:xml:ns:caldav")]
    public SupportedCalendarComponentSet SupportedCalendarComponentSet { get; set; } = null!;
}

public class SupportedCalendarComponentSet
{
    [XmlElement(ElementName = "comp", Namespace = "urn:ietf:params:xml:ns:caldav")]
    public List<Comp> Components { get; set; } = null!;
}

public class Comp
{
    [XmlAttribute(AttributeName = "name")] public string Name { get; set; } = null!;
}