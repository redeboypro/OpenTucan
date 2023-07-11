using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TucanScript
{
    public class Evaluator
    {
        private readonly Dictionary<int, int> multiplicationTokens;
        private readonly Dictionary<int, int> divisionTokens;
        private readonly Dictionary<int, int> divisionRemainderTokens;
        private readonly Dictionary<int, int> additionTokens;
        private readonly Dictionary<int, int> subtractionTokens;

        private readonly Dictionary<int, int> largerNumberTokens;
        private readonly Dictionary<int, int> lessNumberTokens;
        private readonly Dictionary<int, int> equalityTokens;
        
        private readonly Dictionary<int, int> andTokens;
        private readonly Dictionary<int, int> orTokens;
        private readonly Dictionary<int, int> xorTokens;

        public Evaluator()
        {
            multiplicationTokens = new Dictionary<int, int>();
            divisionTokens = new Dictionary<int, int>();
            divisionRemainderTokens = new Dictionary<int, int>();
            additionTokens = new Dictionary<int, int>();
            subtractionTokens = new Dictionary<int, int>(); 
            
            largerNumberTokens = new Dictionary<int, int>(); 
            lessNumberTokens = new Dictionary<int, int>(); 
            equalityTokens = new Dictionary<int, int>();
            
            andTokens = new Dictionary<int, int>(); 
            orTokens = new Dictionary<int, int>(); 
            xorTokens = new Dictionary<int, int>();

            CultureInfo = (CultureInfo) CultureInfo.CurrentCulture.Clone();
            CultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
        }
        
        public CultureInfo CultureInfo { get; }
        
        public object Evaluate(string expression)
        {
            while (expression.Contains(Shell.BracketsOpening.ToString()))
            {
                expression = Trace(expression).ToString();
            }

            return Trace(expression);
        }

        private object Trace(string expression)
        {
            var tokens = Lexer.Separate(expression);
            expression = tokens.JoinTokens();
            
            var expressionPosition = 0;
            var bracketsCount = 0;
            var bracketBuilder = new StringBuilder();

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (tokens[i] == Shell.BracketsOpening.ToString())
                {
                    i++; 
                    bracketsCount++;

                    var startPosition = expressionPosition;
                    var length = 3;

                    while (bracketsCount > 0)
                    {
                        token = tokens[i];

                        if (char.TryParse(token, out var sym))
                        {
                            switch (sym)
                            {
                                case Shell.BracketsOpening:
                                    bracketsCount++;
                                    break;
                                case Shell.BracketsClosing:
                                    bracketsCount--;
                                    break;
                            }
                        }

                        if (bracketsCount > 0)
                        {
                            bracketBuilder.Append(token + Shell.TokenSeparator);
                            length += token.Length + 1;
                        }

                        i++;
                    }

                    return expression.Remove(startPosition, length)
                        .Insert(startPosition, Evaluate(bracketBuilder.ToString()).ToString());
                }
                
                expressionPosition += token.Length + 1;
            }

            for (var i = 0; i < tokens.Length / 2; i++)
            {
                expression = UnitTrace(expression);
            }
            
            if (bool.TryParse(expression, out var resultAsBoolean))
            {
                return resultAsBoolean;
            }

            if (float.TryParse(expression, NumberStyles.Any, CultureInfo, out var resultAsSingle))
            {
                return resultAsSingle;
            }
            
            return null;
        }

        private string UnitTrace(string expression)
        {
            var whitespaces = Lexer.Separate(expression);

            multiplicationTokens.Clear();
            divisionTokens.Clear();
            additionTokens.Clear();
            subtractionTokens.Clear();
            divisionRemainderTokens.Clear();
            largerNumberTokens.Clear();
            lessNumberTokens.Clear();
            equalityTokens.Clear();
            andTokens.Clear();
            orTokens.Clear();
            xorTokens.Clear();

            var currentTokenId = 0;

            for (var i = 0; i < expression.Length; i++)
            {
                var sym = expression[i];
                if (sym == Shell.TokenSeparator)
                {
                    currentTokenId++;
                    continue;
                }

                switch (sym)
                {
                    case '*':
                        multiplicationTokens.Add(currentTokenId, i);
                        break;
                    case '/':
                        divisionTokens.Add(currentTokenId, i);
                        break;
                    case '%':
                        divisionRemainderTokens.Add(currentTokenId, i);
                        break;
                    case '+':
                        additionTokens.Add(currentTokenId, i);
                        break;
                    case '-':
                        subtractionTokens.Add(currentTokenId, i);
                        break;
                    case '>':
                        largerNumberTokens.Add(currentTokenId, i);
                        break;
                    case '<':
                        lessNumberTokens.Add(currentTokenId, i);
                        break;
                    case '=':
                        equalityTokens.Add(currentTokenId, i);
                        break;
                    case '&':
                        andTokens.Add(currentTokenId, i);
                        break;
                    case '|':
                        orTokens.Add(currentTokenId, i);
                        break;
                    case '^':
                        xorTokens.Add(currentTokenId, i);
                        break;
                }
            }

            var divisionRemainderCheck = EvaluateLocally(Operation.DivisionRemainder, divisionRemainderTokens, whitespaces, expression);
            if (divisionRemainderCheck != expression)
            {
                return divisionRemainderCheck;
            }

            var multiplicationCheck = EvaluateLocally(Operation.Multiplication, multiplicationTokens, whitespaces, expression);
            if (multiplicationCheck != expression)
            {
                return multiplicationCheck;
            }

            var divisionCheck = EvaluateLocally(Operation.Division, divisionTokens, whitespaces, expression);
            if (divisionCheck != expression)
            {
                return divisionCheck;
            }

            var additionCheck = EvaluateLocally(Operation.Addition, additionTokens, whitespaces, expression);
            if (additionCheck != expression)
            {
                return additionCheck;
            }

            var subtractionCheck = EvaluateLocally(Operation.Subtraction, subtractionTokens, whitespaces, expression);
            if (subtractionCheck != expression)
            {
                return subtractionCheck;
            }
            
            var largerNumberCheck = EvaluateLocally(Operation.LargerThan, largerNumberTokens, whitespaces, expression);
            if (largerNumberCheck != expression)
            {
                return largerNumberCheck;
            }
            
            var lessNumberCheck = EvaluateLocally(Operation.LessThan, lessNumberTokens, whitespaces, expression);
            if (lessNumberCheck != expression)
            {
                return lessNumberCheck;
            }
            
            var equalityCheck = EvaluateLocally(Operation.Equals, equalityTokens, whitespaces, expression);
            if (equalityCheck != expression)
            {
                return equalityCheck;
            }
            
            var andCheck = EvaluateLocally(Operation.And, andTokens, whitespaces, expression);
            if (andCheck != expression)
            {
                return andCheck;
            }
            
            var orCheck = EvaluateLocally(Operation.Or, orTokens, whitespaces, expression);
            if (orCheck != expression)
            {
                return orCheck;
            }
            
            var xorCheck = EvaluateLocally(Operation.Xor, xorTokens, whitespaces, expression);
            return xorCheck != expression ? xorCheck : expression;
        }

        private string EvaluateLocally(Operation operation, IReadOnlyDictionary<int, int> tokens, IReadOnlyList<string> whitespaces,
            string expression)
        {
            foreach (var token in tokens)
            {
                var a = whitespaces[token.Key - 1];
                var b = whitespaces[token.Key + 1];
                var length = a.Length + b.Length + 3;

                var tokenPosition = token.Value - a.Length - 1;

                var clearedExpression = expression.Remove(tokenPosition, length);

                if (float.TryParse(a, NumberStyles.Any, CultureInfo, out var aTestA) && float.TryParse(b, NumberStyles.Any, CultureInfo, out var bTestA))
                {
                    return GetLocalResult(operation, clearedExpression, tokenPosition, aTestA, bTestA);
                }
                
                if (bool.TryParse(a, out var aTestB) && bool.TryParse(b, out var bTestB))
                {
                    return GetLocalResult(operation, clearedExpression, tokenPosition, aTestB, bTestB);
                }
            }

            return expression;
        }

        private static string GetLocalResult(Operation operation, string expression, int tokenPosition, object a, object b)
        {
            return expression.Insert(tokenPosition, EvaluateOperation(operation, a, b).ToInvariantString());
        }
        
        private static object EvaluateOperation(Operation operation, object a, object b)
        {
            switch (operation)
            {
                /*
                 * Math operations
                 */
                case Operation.Multiplication:
                    return (float) a * (float) b;
                case Operation.Division:
                    return (float) a / (float) b;
                case Operation.DivisionRemainder:
                    return (float) a % (float) b;
                case Operation.Addition:
                    return (float) a + (float) b;
                case Operation.Subtraction:
                    return (float) a - (float) b;
                /*
                 * Comparison operations
                 */
                case Operation.LargerThan:
                    return (float) a > (float) b;
                case Operation.LessThan:
                    return (float) a < (float) b;
                case Operation.Equals:
                    return a.Equals(b);
                
                /*
                 * Logic operations
                 */
                case Operation.And:
                    return (bool) a && (bool) b;
                case Operation.Or:
                    return (bool) a || (bool) b;
                case Operation.Xor:
                    return (bool) a ^ (bool) b;

                default:
                    throw new Exception("Unknown operation");
            }
        }

        private enum Operation
        {
            Multiplication,
            Division,
            Addition,
            Subtraction,
            DivisionRemainder,
            And,
            Or,
            Xor,
            LargerThan,
            LessThan,
            Equals
        }
    }
}