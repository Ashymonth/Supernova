using Ardalis.Specification;
using SupernovaSchool.Models;

namespace SupernovaSchool.Specifications;

public sealed class StudentByIdSpecification : Specification<Student>
{
    public StudentByIdSpecification(string id)
    {
        Query.Where(student => student.Id == id);
    }
    
}