using System;
using Microsoft.CodeAnalysis;

namespace Analyzers
{
    public class OrthogonalEvaluator
    {
        private readonly Func<ISymbol, bool> firstCondition;
        private readonly Func<ISymbol, bool> secondCondition;

        private bool firstConditionResult = false;
        private bool secondConditionResult = false;

        public ISymbol FirstConditionMatch { get; private set; }
        public ISymbol SecondConditionMatch { get; private set; }

        public OrthogonalEvaluator(Func<ISymbol, bool> firstCondition, Func<ISymbol, bool> secondCondition)
        {
            if (firstCondition == null)
            {
                throw new ArgumentNullException(nameof(firstCondition));
            }
            if (secondCondition == null)
            {
                throw new ArgumentNullException(nameof(secondCondition));
            }
            this.firstCondition = firstCondition;
            this.secondCondition = secondCondition;
        }

        public bool Evaluate(ISymbol symbol)
        {
            firstConditionResult = firstConditionResult ? firstConditionResult : FirstCondition(symbol);
            secondConditionResult = secondConditionResult ? secondConditionResult : SecondCondition(symbol);

            return firstConditionResult && secondConditionResult;
        }

        private bool FirstCondition(ISymbol symbol)
        {
            var result = firstCondition(symbol);
            if (result)
            {
                FirstConditionMatch = symbol;
            }
            return result;
        }

        private bool SecondCondition(ISymbol symbol)
        {
            var result = secondCondition(symbol);
            if (result)
            {
                SecondConditionMatch = symbol;
            }
            return result;
        }
    }
}