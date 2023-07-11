using System;
using System.Collections.Generic;
using System.Text;
using TucanScript.Blocks;

namespace TucanScript
{
    public class Invokable : IInvokable
    {
        public static readonly Invokable None = null;
        
        protected readonly List<IInvokable> Invokables;
        protected readonly Dictionary<string, object> Locals;
        
        private readonly Invokable parent;

        public Invokable(Invokable parent, Engine engine, string source)
        {
            Invokables = new List<IInvokable>();
            Locals = new Dictionary<string, object>();
            Engine = engine;
            this.parent = parent;
            Trace(engine.Variables, source);
        }

        protected Engine Engine { get; }

        private void Trace(Dictionary<string, object> variables, string source)
        {
            var tokens = Lexer.Separate(source);
            var bracketsCount = 0;

            var evaluator = Engine.Evaluator;

            for (var tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {
                var token = tokens[tokenIndex];

                var previousIndex = tokenIndex - 1;
                if (Engine.ContainsInstance(token) && (previousIndex < 0 || tokens[previousIndex].IsClosing()))
                {
                    var executableBuilder = new StringBuilder();
                    executableBuilder.Append(token);
                    
                    while (!Engine.ContainsInternalExecutable(executableBuilder.ToString()))
                    {
                        tokenIndex++;
                        executableBuilder.Append(tokens[tokenIndex]);
                    }

                    tokenIndex++;

                    var cachedIndex = tokenIndex;
                    Invokables.Add(new Executable(variables, inputVariables =>
                    {
                        var internalIndex = cachedIndex;
                        Engine.InvokeInternalExecutable(executableBuilder.ToString(),
                            GetParameters(ref internalIndex, tokens, inputVariables));
                    }));
                }

                if (token == Shell.BeginStatement)
                {
                    var beginingIndex = tokenIndex - 1;
                    token = tokens[beginingIndex];

                    BlockType blockType;
                    var parametersTokens = new List<string>();
                    
                    while (!token.IsBlock(out blockType))
                    {
                        parametersTokens.Add(token);
                        token = tokens[--beginingIndex];
                    }

                    parametersTokens.Reverse();
                    
                    var parameters = parametersTokens.JoinTokens();

                    bracketsCount++;
                    tokenIndex++;

                    var internalData = new List<string>();
                    
                    while (bracketsCount > 0)
                    {
                        token = tokens[tokenIndex];
                        
                        switch (token)
                        {
                            case Shell.BeginStatement:
                                bracketsCount++;
                                break;
                            case Shell.EndStatement:
                                bracketsCount--; 
                                break;
                        }

                        if (bracketsCount > 0)
                        {
                            internalData.Add(token);
                        }
                        
                        tokenIndex++;
                    }

                    tokenIndex--;
                    token = tokens[tokenIndex];

                    var combinedInternalData = internalData.JoinTokens();

                    object ConditionCheck(Dictionary<string, object> inputVariables)
                    {
                        if (evaluator != null)
                        {
                            var inserted = InsertValues(inputVariables, parameters);
                            return (bool) evaluator.Evaluate(inserted);
                        }

                        return false;
                    }
                    
                    switch (blockType)
                    {
                        case BlockType.If:
                            var ifStatementInstance = new If(this, Engine, ConditionCheck, combinedInternalData);
                            Invokables.Add(ifStatementInstance);
                            break;
                        case BlockType.While:
                            var whileStatementInstance = new While(this, Engine, ConditionCheck, combinedInternalData);
                            Invokables.Add(whileStatementInstance);
                            break;
                    }
                }

                if (token.IsDeclarationToken(Engine, out var declarationType))
                {
                    var declarationToken = token;
                    var declarationTokens = new List<string>();
                    
                    while (token != Shell.DeclarationSeparator)
                    {
                        declarationTokens.Add(token);
                        token = tokens[++tokenIndex];
                    }

                    int localIndex;
                    
                    switch (declarationType)
                    {
                        case DeclarationType.Variable:
                            for (localIndex = 1; localIndex < declarationTokens.Count; localIndex++)
                            {
                                var localName = declarationTokens[localIndex];
                                variables.Add(localName, new object());
                            }
                            break;
                        case DeclarationType.Local:
                            for (localIndex = 1; localIndex < declarationTokens.Count; localIndex++)
                            {
                                var localName = declarationTokens[localIndex];
                                Locals.Add(localName, new object());
                            }
                            break;
                        case DeclarationType.Instance :
                            for (localIndex = 1; localIndex < declarationTokens.Count; localIndex++)
                            {
                                var localName = declarationTokens[localIndex];
                                Engine.CreateInstanceOfStruct(localName, declarationToken);
                            }
                            break;
                        case DeclarationType.Operation:
                            
                            OperationType operation;
                            
                            var variableNames = new List<string>();
                            var localPlaceholder = declarationTokens[localIndex = 1];

                            while (!localPlaceholder.IsOperator(out operation))
                            {
                                variableNames.Add(localPlaceholder);
                                localPlaceholder = declarationTokens[++localIndex];
                            }
                            
                            var expressionTokens = new List<string>();
                            
                            for (localIndex++; localIndex < declarationTokens.Count; localIndex++)
                            {
                                expressionTokens.Add(declarationTokens[localIndex]);
                            }

                            var expression = expressionTokens.JoinTokens();

                            foreach (var local in variableNames)
                            {
                                Invokables.Add(new Executable(variables, TraceOperation(local, expression, operation))); 
                            }
                            break;
                    }
                }
            }
        }

        private ExecutableTypeDef TraceOperation(string local, string expression, OperationType operation)
        {
            var evaluator = Engine.Evaluator;

            if (evaluator is null)
            {
                throw new Exception("Evaluator should be initialized before expression parsing");
            }

            switch (operation)
            {
                case OperationType.Equalize:
                    return inputVariables =>
                    {
                        expression = InsertValues(inputVariables, expression);
                        var evaluated = evaluator.Evaluate(expression);

                        if (ContainsLocal(local, out var holder))
                        {
                            holder.Locals[local] = evaluated;
                            return;
                        }
                        
                        inputVariables[local] = evaluated;
                    };
                case OperationType.Multiply:
                    return inputVariables =>
                    {
                        expression = InsertValues(inputVariables, expression);
                        var containsLocal = ContainsLocal(local, out var holder);
                        var evaluated = (containsLocal ? (float) holder.Locals[local] : (float) inputVariables[local]) * (float) evaluator.Evaluate(expression);
                        
                        if (containsLocal)
                        {
                            holder.Locals[local] = evaluated;
                            return;
                        }
                        
                        inputVariables[local] = evaluated;
                    };
                case OperationType.Divide:
                    return inputVariables =>
                    {
                        expression = InsertValues(inputVariables, expression);
                        var containsLocal = ContainsLocal(local, out var holder);
                        var evaluated = (containsLocal ? (float) holder.Locals[local] : (float) inputVariables[local]) / (float) evaluator.Evaluate(expression);
                        
                        if (containsLocal)
                        {
                            holder.Locals[local] = evaluated;
                            return;
                        }
                        
                        inputVariables[local] = evaluated;
                    };
                case OperationType.Add:
                    return inputVariables =>
                    {
                        expression = InsertValues(inputVariables, expression);
                        var containsLocal = ContainsLocal(local, out var holder);
                        var evaluated = (containsLocal ? (float) holder.Locals[local] : (float) inputVariables[local]) + (float) evaluator.Evaluate(expression);
                        
                        if (containsLocal)
                        {
                            holder.Locals[local] = evaluated;
                            return;
                        }
                        
                        inputVariables[local] = evaluated;
                    };
                case OperationType.Subtract:
                    return inputVariables =>
                    {
                        expression = InsertValues(inputVariables, expression);
                        var containsLocal = ContainsLocal(local, out var holder);
                        var evaluated = (containsLocal ? (float) holder.Locals[local] : (float) inputVariables[local]) - (float) evaluator.Evaluate(expression);
                        
                        if (containsLocal)
                        {
                            holder.Locals[local] = evaluated;
                            return;
                        }
                        
                        inputVariables[local] = evaluated;
                    };
            }

            return null;
        }

        private bool ContainsLocal(string localName, out Invokable holder)
        {
            holder = this;
            while (holder.parent != null)
            {
                if (holder.Locals.ContainsKey(localName))
                {
                    return true;
                }

                holder = holder.parent;
            }

            return holder.Locals.ContainsKey(localName);
        }

        protected string InsertValues(IReadOnlyDictionary<string, object> inputVariables, string source)
        {
            var inputTokens = Lexer.Separate(source);
            var resultTokens = new List<string>();

            for (var index = 0; index < inputTokens.Length; index++)
            {
                var token = inputTokens[index];
                if (ContainsLocal(token, out var holder))
                {
                    resultTokens.Add(holder.Locals[token].ToInvariantString());
                    continue;
                }

                if (inputVariables.ContainsKey(token))
                {
                    resultTokens.Add(inputVariables[token].ToInvariantString());
                    continue;
                }
                
                if (token == Shell.CharQuote)
                {
                    token = inputTokens[++index];
                    resultTokens.Add(((int) char.Parse(token)).ToString());
                    index++;
                    continue;
                }

                if (Engine.ContainsInstance(token))
                {
                    var functionBuilder = new StringBuilder();
                    functionBuilder.Append(token);
                    while (!Engine.ContainsInternalFunction(functionBuilder.ToString()))
                    {
                        functionBuilder.Append(inputTokens[++index]);
                    }

                    index++;
                    var parameters = GetParameters(ref index, inputTokens, inputVariables);
                    resultTokens.Add(Engine.InvokeInternalFunction(functionBuilder.ToString(), parameters).ToInvariantString());
                    continue;
                }

                resultTokens.Add(token);
            }

            return resultTokens.JoinTokens();
        }

        private object[] GetParameters(ref int index, IReadOnlyList<string> inputTokens, IReadOnlyDictionary<string, object> inputVariables)
        {
            var parameters = new List<object>();
            
            var token = inputTokens[index];
            
            if (token != Shell.BracketsOpening.ToString())
            {
                return parameters.ToArray();
            }
            
            var localTokens = new List<string>();
            var bracketsCount = 1;

            index++;
            while (bracketsCount > 0)
            {
                token = inputTokens[index];
                if (token == Shell.ParameterSeparator && bracketsCount is 1)
                {
                    PushParameter(parameters, inputVariables, localTokens);
                    localTokens.Clear();
                    index++;
                    continue;
                }
                            
                if (char.TryParse(token, out var sym))
                {
                    switch (sym)
                    {
                        case Shell.BracketsOpening:
                            bracketsCount++;
                            break;
                        case Shell.BracketsClosing:
                            bracketsCount--;
                            if (bracketsCount <= 0)
                            {
                                continue;
                            }
                            break;
                    }
                }
                
                localTokens.Add(token);
                index++;
            }
            PushParameter(parameters, inputVariables, localTokens);

            return parameters.ToArray();
        }

        private void PushParameter(ICollection<object> parameters,
            IReadOnlyDictionary<string, object> inputVariables,
            IEnumerable<string> localTokens)
        {
            var result = Engine.Evaluator.Evaluate(InsertValues(inputVariables, localTokens.JoinTokens()));
            if (result != null)
            {
                parameters.Add(result);
            }
        }

        public virtual void Invoke()
        {
            foreach (var invokable in Invokables)
            {
                invokable.Invoke();
            }
        }

        public void Dispose()
        {
            if (Invokables != null)
            {
                for (var index = 0; index < Invokables.Count; index++)
                {
                    Invokables[index].Dispose();
                    Invokables[index] = null;
                }
            }
        }
    }
}