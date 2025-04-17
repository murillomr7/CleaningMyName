using AutoFixture;

namespace CleaningMyName.UnitTests.Common;

/// <summary>
/// Base class for all test classes providing common functionality
/// </summary>
public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
}

/// <summary>
/// AutoMoq customization for AutoFixture
/// </summary>
public class AutoMoqCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
