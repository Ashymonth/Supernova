using Ardalis.Specification;
using SupernovaSchool.Models;

namespace SupernovaSchool.Specifications;

public sealed class TeacherByIdSpecification : Specification<Teacher>, ISingleResultSpecification<Teacher>
{
    public TeacherByIdSpecification(Guid teacherId)
    {
        Query.Where(teacher => teacher.Id == teacherId);
    }
}