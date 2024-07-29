using com.antlersoft.HostedTools.ConditionBuilder.Model;

namespace Tests;

public class Tests
{
    IEnumerable<IHtValue> GenerateHtValue(IEnumerable<string> input) {
        foreach (string s in input) {
            var arr = s.Split();
            int l = arr.Length;
            JsonHtValue val = new JsonHtValue();
            for (int i=0; i<l; i++) {
                char c=(char)((int)('A') + i);
                string key = new string(c, 1);
                val[key]=new JsonHtValue(arr[i]);
            }
            yield return val;
        }
    }
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    class AllFields : IHtExpression {
        public IHtValue Evaluate(IHtValue ht) {
            int i=0;
            var result = new JsonHtValue();
            if (! ht["left"].IsEmpty) {
            foreach (var pair in ht["left"].AsDictionaryElements) {
                char c=(char)((int)('A') + i++);
                string key = new string(c, 1);
                result[key] = pair.Value;
            }
            }
            if (! ht["right"].IsEmpty) {
            foreach (var pair in ht["right"].AsDictionaryElements) {
                char c=(char)((int)('A') + i++);
                string key = new string(c, 1);
                result[key] = pair.Value;
            }
            }
            return result;
        }
    }

    [Test]
    public void GeneratorTest()
    {
        var arg1=new string[] { "1 A", "2 B"};
        int count = 0;
        foreach (var val in GenerateHtValue(arg1))
        {
            count++;
            Assert.AreEqual(2, val.AsDictionaryElements.Count());
            Assert.AreEqual((long)count, val["A"].AsLong);
        }
        Assert.AreEqual(count, 2);
    }

    static ValueComparer _comparer = new ValueComparer();
    static IHtExpression _allFields = new AllFields();
    static IHtExpression _keyExpression = new ConditionBuilder().ParseCondition("A");
    [Test]
    public void AllFieldsTest() {
        var arg1 = new string[] { "left 1", "right 2"};
        JsonHtValue combined = new JsonHtValue();
        foreach (var val in GenerateHtValue(arg1))
        {
            combined[val["A"].AsString]=val;
        }
        Assert.That(_comparer.Compare(_allFields.Evaluate(combined), GenerateHtValue(new string[] { "left 1 right 2"}).First()),
            Is.EqualTo(0));
    }

    [Test]
    public void JoinTestInner1() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }
   [Test]
   public void JoinTestInner2() {
        var arg1 = new string[] { "1", "2","3"};
        var arg2 = new string[] { "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }
   [Test]
   public void JoinTestInner3() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2", "2", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }
   [Test]
   public void JoinTestInner4() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "0", "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }

   [Test]
   public void JoinTestInner5() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "0", "2", "2", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }

   [Test]
   public void JoinTestInner6() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2", "3", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }
   [Test]
   public void JoinTestInner7() {
        var arg1 = new string[] { "1", "2", "2"};
        var arg2 = new string[] { "0", "2", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }
   [Test]
   public void JoinTestInner8() {
        var arg1 = new string[] { "1", "2", "2"};
        var arg2 = new string[] { "0", "2", "2", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2 2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.JoinSymmetric, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(4));
   }  [Test]
    public void JoinTestIn1() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }
   [Test]
   public void JoinTestIn2() {
        var arg1 = new string[] { "1", "2","3"};
        var arg2 = new string[] { "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }
   [Test]
   public void JoinTestIn3() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2", "2", "3"};
        var expectedResult = GenerateHtValue(new string[] { "2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }
   [Test]
   public void JoinTestIn4() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "0", "2", "2"};
        var expectedResult = GenerateHtValue(new string[] { "2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }

   [Test]
   public void JoinTestIn5() {
        var arg1 = new string[] { "1", "4"};
        var arg2 = new string[] { "0", "1", "1", "3"};
        var expectedResult = GenerateHtValue(new string[] { "1"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }

   [Test]
   public void JoinTestIn6() {
        var arg1 = new string[] { "-1", "1", "1", "4"};
        var arg2 = new string[] { "0", "1", "1", "3"};
        var expectedResult = GenerateHtValue(new string[] { "1"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(2));
   }

   [Test]
   public void JoinTestIn7() {
        var arg1 = new string[] { "1", "2"};
        var arg2 = new string[] { "2"};
        var expectedResult = GenerateHtValue(new string[] { "2"}).First();
        int count = 0;
        foreach (var val in JoinTransform.GetJoin(JoinTransform.JoinTypes.In, JoinTransform.ResultTypes.ProjectionExpression, _comparer, _allFields, GenerateHtValue(arg1), _keyExpression, GenerateHtValue(arg2), _keyExpression))
        {
            count++;
            Assert.That(_comparer.Compare(val, expectedResult), Is.EqualTo(0));
        }
        Assert.That(count, Is.EqualTo(1));
   }
}