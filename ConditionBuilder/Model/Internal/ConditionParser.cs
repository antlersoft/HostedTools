using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal class ConditionParser : ParserBase
    {
        internal static readonly Symbol NumberSym = new Symbol("NumberSym");
        internal static readonly Symbol NameSym = new Symbol("NameSym");
        internal static readonly Symbol StringSym = new Symbol("StringSym");
        internal static readonly Symbol SubscriptableExpressionSym = new Symbol("SubscriptableExpression");
        internal static readonly Symbol SubscriptExpressionSym = new Symbol("SubscriptExpressionSym");
        internal static readonly Symbol ProjectionItemSym = new Symbol("ProjectionItemSym");
        internal static readonly Symbol ProjectionTailSym = new Symbol("ProjectionTailSym");
        internal static readonly Symbol ArgumentListSym = new Symbol("ArgumentListSym");
        internal static readonly Symbol ExprSym = new Symbol("ExprSym");
        internal static readonly Symbol LeftParen = new Symbol("(");
        internal static readonly Symbol RightParen = new Symbol(")");
        internal static readonly Symbol LeftBracket = new Symbol("[");
        internal static readonly Symbol RightBracket = new Symbol("]");
        internal static readonly Symbol Colon = new Symbol(":");
        internal static readonly Symbol Asterisk = TimesOperator.I.Name;
        internal static readonly Symbol AtSign = new Symbol("@");

        internal static readonly Symbol Comma = new Symbol(",");

        internal static readonly RegexMatch NameToken = new RegexMatch("[_a-zA-Z][_a-zA-Z0-9]*",
            s => new Token(NameSym, s));

        internal static readonly RegexMatch NumberToken =
            new RegexMatch(@"(((-?[1-9][0-9]*)|0)\.[0-9]*)|(((-?[1-9][0-9]*)|0)|(-?\.[0-9][0-9]*))",
                s => new Token(NumberSym, s));

        internal static readonly RegexMatch StringToken = new RegexMatch("'(([^'\\\\])|(\\\\')|(\\\\\\\\))*'",
            s =>
                new Token(StringSym,
                    s.Substring(1, s.Length - 2).Replace("\\'","'").Replace("\\\\", "\\")));
        internal static readonly RegexMatch QuotedNameToken = new RegexMatch("\"(([^\"\\\\])|(\\\\\")|(\\\\\\\\))*\"",
            s =>
                new Token(NameSym,
                    s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\")));

        internal static readonly SingleChar CommaToken = new SingleChar(',', new Token(Comma));
        private static Symbol Begins = new Symbol("begins");
        private static Symbol Between = new Symbol("between");
        private static Symbol ContainsSym = new Symbol("contains");
        private static Symbol And = new Symbol("and");
        private static Symbol ValueSym = new Symbol("ValueSym");
        private static Symbol In = new Symbol("in");
        private static Symbol InList = new Symbol("InList");
        private static Symbol Is = new Symbol("is");
        private static Symbol Null = new Symbol("null");
        private static Symbol Not = new Symbol("not");
        private static Symbol With = new Symbol("with");

        private static NextToken[] _matchers =
        {
            StringToken,
            NumberToken,
            new StringMatch("<=", LessThanOrEqual.I),
            new SingleChar('<', LessThan.I),
            new StringMatch("==", EqualTo.I),
            new StringMatch("!=", NotEqualTo.I),
            new StringMatch(">=", GreaterThanOrEqual.I),
            new SingleChar('>', GreaterThan.I),
            new StringMatch("~=", RegExpMatch.I),
            new SingleChar('+', PlusOperator.I), 
            new StringMatch("&&", new LogicalAnd()),
            new StringMatch("||", new LogicalOr()),
            new SingleChar(',', new Token(Comma)),
            new StringMatch("begins", new Token(Begins)),
            new StringMatch("between", new Token(Between)),
            new StringMatch("contains", new Token(ContainsSym)),
            new StringMatch("and", new Token(And)),
            new StringMatch("in", new Token(In)),
            new StringMatch("is", new Token(Is)),
            new StringMatch("null", new Token(Null)),
            new SingleChar('!', new Token(Not)),
            new StringMatch("with", new Token(With)),
            new SingleChar('(', new Token(LeftParen)),
            new SingleChar(')', new Token(RightParen)),
            new SingleChar('[', new Token(LeftBracket)),
            new SingleChar(']', new Token(RightBracket)),
            new SingleChar(':', new Token(Colon)),
            new SingleChar('*', TimesOperator.I),
            new SingleChar('-', SubtractOperator.I),
            new SingleChar('/', DivideOperator.I), 
            new SingleChar('@', new Token(AtSign)), 
            NameToken,
            QuotedNameToken
        };

        private static IGoal[] GetGoals(IEnumerable<IFunctionNamespace> namespaces)
        {
            return new IGoal[] {
            new SG(ValueSym,
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (double),
                                d =>
                                    new ConstantExpression(
                                        new JsonHtValue(Double.Parse(((TokenNode) pr[0].Node).Token.Value)))),
                            pr[0].NextTokenOffset), NumberSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (string),
                                d =>
                                    new ConstantExpression(new JsonHtValue(((TokenNode) pr[0].Node).Token.Value))),
                            pr[0].NextTokenOffset), StringSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(
                                typeof (IHtValue),
                                d => new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value)
                                ),
                            pr[0].NextTokenOffset
                            ),
                    NameSym
                    )
                ),
            new SG(SimpleExprSym,
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d => new OperatorExpression("BETWEEN", f =>
                                    new JsonHtValue(
                                        LessThanOrEqual.I.If(new List<IHtValue> {f[1], f[0]}) &&
                                        GreaterThanOrEqual.I.If(new List<IHtValue>
                                        {
                                            f[2],
                                            f[0]
                                        })),
                                    new[]
                                    {
                                        new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value),
                                        (IHtExpression) pr[2].Node.GetFunctor()(d),
                                        (IHtExpression) pr[4].Node.GetFunctor()(d)
                                    }
                                    )
                                ), pr[4].NextTokenOffset),
                    NameSym, Between, ValueSym, And, ValueSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d => new OperatorExpression("IN", f =>
                                {
                                    var v = f[0];
                                    var result = false;

                                    for (int i = 1; i < f.Count && ! result; i++)
                                    {
                                        result = EqualTo.I.If(new List<IHtValue> {v, f[i]});
                                    }
                                    return new JsonHtValue(result);
                                },
                                    new[] {new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value)}.Concat(
                                        (List<IHtExpression>) pr[2].Node.GetFunctor()(d))))
                            , pr[2].NextTokenOffset),
                    NameSym, In, InList),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d => new OperatorExpression("NULL", f =>
                                    new JsonHtValue(f[0].IsEmpty),
                                    new[] {new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value)})),
                            pr[2].NextTokenOffset),
                    NameSym, Is, Null),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d => new OperatorExpression("NOT_NULL", f =>
                                    new JsonHtValue(! f[0].IsEmpty),
                                    new[] {new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value)})),
                            pr[3].NextTokenOffset),
                    NameSym, Is, Not, Null),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d =>
                                    new BeginsWithExpression(
                                        new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value),
                                        (IHtExpression) pr[3].Node.GetFunctor()(d))), pr[3].NextTokenOffset),
                    NameSym, Begins, With, ValueSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d =>
                                    new OperatorExpression("CONTAINS",
                                        f => new JsonHtValue(f[0].AsString.Contains(f[1].AsString))
                                        ,
                                        new[]
                                        {
                                            new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value),
                                            (IHtExpression) pr[2].Node.GetFunctor()(d)
                                        })), pr[2].NextTokenOffset),
                    NameSym, ContainsSym, ValueSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (bool),
                                d =>
                                    new OperatorExpression("NOT_CONTAINS",
                                        f => new JsonHtValue(! f[0].AsString.Contains(f[1].AsString))
                                        ,
                                        new[]
                                        {
                                            new DataReferenceExpression(((TokenNode) pr[0].Node).Token.Value),
                                            (IHtExpression) pr[3].Node.GetFunctor()(d)
                                        })), pr[3].NextTokenOffset),
                    NameSym, Not, ContainsSym, ValueSym),
                new FunctorParseRule(
                    (t, pr) => new GoalResult(pr[0].Node, pr[0].NextTokenOffset),
                    SubscriptExpressionSym
                    ),
                new FunctorParseRule(
                    (t, pr) => new GoalResult(
                        new ParseTreeNode(typeof (bool),
                            d =>
                                new OperatorExpression("!", f => new JsonHtValue(! f[0].AsBool),
                                    new[] {(IHtExpression) pr[1].Node.GetFunctor()(d)})), pr[1].NextTokenOffset), Not,
                    SimpleExprSym
                    ),
                new FunctorParseRule(
                    (t, pr) => new GoalResult(
                        new ParseTreeNode(typeof (IHtExpression),
                            d => new NullHtExpression()), pr[0].NextTokenOffset),
                    Null
                    )),
            new SG(SubscriptableExpressionSym,
                new FunctorParseRule(
                    (t, pr) => new GoalResult(pr[0].Node, pr[0].NextTokenOffset),
                    ValueSym
                    ),
                 new FunctorParseRule(
                    (t, pr) => new GoalResult(new ParenthesizedNode(pr[1].Node), pr[2].NextTokenOffset),
                    LeftParen, ExprSym, RightParen
                    ),
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(pr[2].Node, pr[2].NextTokenOffset),
                     AtSign, LeftParen, ProjectionTailSym
                     ),
                 new FunctorParseRule(
                     (t,pr) => new GoalResult(new FunctionCallNode(namespaces, pr[1].Node, pr[3].Node), pr[3].NextTokenOffset),
                     AtSign, NameSym, LeftParen, ArgumentListSym
                     )),
            new SG(ProjectionTailSym,
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionNode((ProjectionNode)pr[2].Node, (ProjectionItemNode)pr[0].Node), pr[2].NextTokenOffset),
                     ProjectionItemSym, Comma, ProjectionTailSym
                     ),
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionNode(new ProjectionNode(false), (ProjectionItemNode)pr[0].Node), pr[1].NextTokenOffset),
                     ProjectionItemSym, RightParen
                     ),
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionNode(true), pr[1].NextTokenOffset),
                     Asterisk, RightParen
                     )),
            new SG(ProjectionItemSym,
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionItemNode(pr[0].Node, pr[2].Node), pr[2].NextTokenOffset),
                     ExprSym, Colon, ExprSym
                     ),
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionItemNode(pr[0].Node, new WildCardNode()), pr[2].NextTokenOffset),
                     ExprSym, Colon, Asterisk
                     ),
                 new FunctorParseRule(
                     (t, pr) => new GoalResult(new ProjectionItemNode(pr[0].Node, null), pr[0].NextTokenOffset),
                     ExprSym
                     )), 
            new SG(InList,
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (List<IHtExpression>),
                                d =>
                                {
                                    var l = (List<IHtExpression>) pr[2].Node.GetFunctor()(d);
                                    l.Insert(0, (IHtExpression) pr[0].Node.GetFunctor()(d));
                                    return l;
                                }),
                            pr[2].NextTokenOffset), ValueSym, Comma, InList),
                new FunctorParseRule((t, pr) => new GoalResult(
                    new ParseTreeNode(
                        typeof (List<IHtExpression>),
                        d => new List<IHtExpression> {(IHtExpression) pr[0].Node.GetFunctor()(d)}),
                    pr[0].NextTokenOffset), ValueSym)
                ),
            new SG(ArgumentListSym,
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (List<IHtExpression>),
                                d => new List<IHtExpression>()),
                                pr[0].NextTokenOffset), RightParen),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (List<IHtExpression>),
                                d => 
                                {
                                    var l = (List<IHtExpression>) pr[2].Node.GetFunctor()(d);
                                    l.Insert(0, (IHtExpression) pr[0].Node.GetFunctor()(d));
                                    return l;
                                }),
                                pr[2].NextTokenOffset), ExprSym, Comma, ArgumentListSym),
                new FunctorParseRule(
                    (t, pr) =>
                        new GoalResult(
                            new ParseTreeNode(typeof (List<IHtExpression>),
                                d => new List<IHtExpression> {(IHtExpression) pr[0].Node.GetFunctor()(d)}),
                                pr[1].NextTokenOffset), ExprSym, RightParen)
                ), 
            new InFixGoal(),
            new SubscriptGoal(),
            new SG(ExprSym,
                new FunctorParseRule(
                    (t, pr) => ((InFixNode) pr[0].Node).ResolveTypes(pr[0].NextTokenOffset),
                    InFixExprSym),
                new FunctorParseRule(
                    (t, pr) => new GoalResult(pr[0].Node, pr[0].NextTokenOffset),
                    SimpleExprSym)
                )
        };
        }

            internal ConditionParser(IEnumerable<IFunctionNamespace> namespaces)
                : base(_matchers, GetGoals(namespaces))
            {

            }
    }


}
