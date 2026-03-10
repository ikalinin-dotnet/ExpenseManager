using ExpenseManager.Domain.ValueObjects;
using FluentAssertions;

namespace ExpenseManager.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreateMoney_WithDefaults()
    {
        var money = new Money(100m);

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrency_ToUppercase()
    {
        var money = new Money(50m, "eur");
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNegative()
    {
        var act = () => new Money(-1m);
        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenCurrencyIsEmpty(string? currency)
    {
        var act = () => new Money(10m, currency!);
        act.Should().Throw<ArgumentException>().WithMessage("*Currency*");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameAmountAndCurrency()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "USD");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentAmounts()
    {
        var a = new Money(100m, "USD");
        var b = new Money(200m, "USD");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentCurrencies()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "EUR");

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var money = new Money(99.90m, "GBP");
        money.ToString().Should().Be("99.90 GBP");
    }
}
