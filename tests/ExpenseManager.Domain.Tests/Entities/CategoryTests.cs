using ExpenseManager.Domain.Entities;
using FluentAssertions;

namespace ExpenseManager.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldCreateCategory()
    {
        var category = new Category(Guid.NewGuid(), "Travel", "Business travel");

        category.Name.Should().Be("Travel");
        category.Description.Should().Be("Business travel");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenNameIsEmpty(string? name)
    {
        var act = () => new Category(Guid.NewGuid(), name!, null);
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void Update_ShouldModifyNameAndDescription()
    {
        var category = new Category(Guid.NewGuid(), "Old", "Old desc");

        category.Update("New", "New desc");

        category.Name.Should().Be("New");
        category.Description.Should().Be("New desc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Update_ShouldThrow_WhenNameIsEmpty(string? name)
    {
        var category = new Category(Guid.NewGuid(), "Valid", null);

        var act = () => category.Update(name!, null);

        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }
}
