namespace SupernovaSchool.Telegram.Steps;

public class MessagePaginator
{
    public int CurrentPage { get; set; }

    public string PaginationMessage { get; set; } = null!;

    public int GetPage()
    {
        switch (PaginationMessage)
        {
            case "Дальше":
                CurrentPage++;
                break;
            case "Назад":
                CurrentPage--;
                break;
        }

        return CurrentPage;
    }
}