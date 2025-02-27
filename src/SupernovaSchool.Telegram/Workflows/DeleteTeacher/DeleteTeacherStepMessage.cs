using SupernovaSchool.Models;

namespace SupernovaSchool.Telegram.Workflows.DeleteTeacher;

public static class DeleteTeacherStepMessage
{
    private const string SelectTeacherToDeleteTemplate = "Выберите номер учителя для удаления: \n{0}";

    public const string TeacherDeletedMessage = "Учитель удален";
    
    public static string SelectTeacherToDeleteMessage(List<Teacher> teachers)
    {
        var message = string.Join("\n", teachers.Select((teacher, index) => $"{index}. {teacher.Name}"));

        return string.Format(SelectTeacherToDeleteTemplate, message);
    }
}