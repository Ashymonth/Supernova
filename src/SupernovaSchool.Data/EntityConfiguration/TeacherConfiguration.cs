using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupernovaSchool.Models;

namespace SupernovaSchool.Data.EntityConfiguration;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ComplexProperty(credential => credential.YandexCalendarPassword, propertyBuilder =>
        {
            propertyBuilder.Property("Value").HasColumnName(nameof(Teacher.YandexCalendarPassword));
        });
    }
}