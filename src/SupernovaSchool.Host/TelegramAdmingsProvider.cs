using SupernovaSchool.Abstractions;

namespace SupernovaSchool.Host;

public class TelegramAdminsProvider : IAdminsProvider
{
    private readonly HashSet<string> _adminIds;

    public TelegramAdminsProvider(HashSet<string> adminIds)
    {
        _adminIds = adminIds;
    }

    public bool IsAdmin(string userId) => _adminIds.Contains(userId);
}