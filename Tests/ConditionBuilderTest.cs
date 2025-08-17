
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model;

namespace Tests;

public class ConditionBuilderTests
{
    IConditionBuilder builder;
    [SetUp]
    public void Setup()
    {
        builder = new ConditionBuilder();
    }

    [Test]
    public void BoolLiteralTrue()
    {
        var exp = builder.ParseCondition("True");
    }
    [Test]
    public void And()
    {
        var exp = builder.ParseCondition("True && True");
    }
    [Test]
    public void MatchAnd()
    {
        var exp = builder.ParseCondition("x ~= '((x1)|(x2))' && true");
    }
    [Test]
    public void EqualsAnd()
    {
        var exp = builder.ParseCondition("x == '((x1)|(x2))' && true");
    }
    [Test]
    public void AndMatch()
    {
        var exp = builder.ParseCondition("true && x ~= '((x1)|(x2))'");
    }
    [Test]
    public void MatchOr()
    {
        var exp = builder.ParseCondition("x ~= '((x1)|(x2))' || true");
    }
    [Test]
    public void InLineArray()
    {
        var exp = builder.ParseCondition("@[3,5][@_()]");
        Assert.That(5, Is.EqualTo(exp.Evaluate(new JsonHtValue(1)).AsLong));
    }
}