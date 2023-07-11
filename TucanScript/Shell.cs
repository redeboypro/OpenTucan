using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TucanScript
{
    public enum BlockType
    {
        Object,
        If,
        While,
        None
    }

    public enum DeclarationType
    {
        Variable,
        Operation,
        Local,
        Instance,
        None
    }
    
    public enum OperationType 
    { 
        Equalize,
        Multiply,
        Divide,
        Add,
        Subtract,
        None
    }
    
    public delegate object FunctionTypeDef(Dictionary<string, object> variables, object[] parameters);
    
    public delegate void VoidTypeDef(Dictionary<string, object> variables, object[] parameters);

    public static class Shell
    {
        public const char TokenSeparator = ' ';
        public const string DeclarationSeparator = ";";
        public const string ParameterSeparator = ",";

        public const string CharQuote = "'";

        public const char BracketsOpening = '(';
        public const char BracketsClosing = ')';

        public const string BeginStatement = "{";
        public const string EndStatement = "}";
        
        private const string Execute = "exec";
        private const string Print = "print";
        
        private const string VariableDeclaration = "var";
        private const string LocalDeclaration = "local";
        private const string OperationDeclaration = "let";

        private const string If = "if";
        private const string While = "while";

        private const string Equalize = "=";
        private const string Multiply = "*";
        private const string Divide = "/";
        private const string Add = "+";
        private const string Subtract = "-";

        public static bool IsOperator(this string source, out OperationType operation)
        {
            operation = OperationType.None;
            
            switch (source)
            {
               case Equalize:
                   operation = OperationType.Equalize;
                   return true;
               case Multiply:
                   operation = OperationType.Multiply;
                   return true;
               case Divide:
                   operation = OperationType.Divide;
                   return true;
               case Add:
                   operation = OperationType.Add;
                   return true;
               case Subtract:
                   operation = OperationType.Subtract;
                   return true;
            }

            return false;
        }
        
        public static bool IsBlock(this string source, out BlockType statementType)
        {
            statementType = BlockType.None;
            
            switch (source)
            {
                case If :
                    statementType = BlockType.If;
                    return true;
                case While :
                    statementType = BlockType.While;
                    return true;
            }

            return false;
        }
        
        public static bool IsDeclarationToken(this string source, Engine engine, out DeclarationType declarationType)
        {
            declarationType = DeclarationType.None;
            
            if (engine.ContainsInstanceType(source))
            {
                declarationType = DeclarationType.Instance;
                return true;
            }

            switch (source)
            {
                case VariableDeclaration :
                    declarationType = DeclarationType.Variable;
                    return true;
                case OperationDeclaration :
                    declarationType = DeclarationType.Operation;
                    return true;
                case LocalDeclaration :
                    declarationType = DeclarationType.Local;
                    return true;
            }

            return false;
        }

        public static string ParametersToString(this IEnumerable<object> parameters)
        {
            var pathBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                pathBuilder.Append((char)Convert.ToInt32(parameter));
            }

            return pathBuilder.ToString();
        }
        
        public static string ToInvariantString(this object token)
        {
            if (token is float number)
            {
                return number.ToString(CultureInfo.InvariantCulture);
            }

            return token.ToString();
        }
        
        public static bool IsClosing(this string token)
        {
            return token is EndStatement || token is DeclarationSeparator;
        }
        
        public static string JoinTokens(this IEnumerable<string> tokens)
        {
            return string.Join(TokenSeparator.ToString(), tokens);
        }
    }
}