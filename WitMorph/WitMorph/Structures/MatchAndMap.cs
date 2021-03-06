using System;
using System.Collections.Generic;
using System.Linq;

namespace WitMorph.Structures
{
    public class MatchAndMap<TItem, TKey> where TItem:class where TKey:class
    {
        private readonly Func<TItem, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _keyEqualityComparer;
        private readonly ICurrentToGoalMap<TKey> _keyMap;

        public MatchAndMap(Func<TItem, TKey> keySelector, IEqualityComparer<TKey> keyEqualityComparer, ICurrentToGoalMap<TKey> keyMap)
        {
            _keySelector = keySelector;
            _keyEqualityComparer = keyEqualityComparer;
            _keyMap = keyMap;
        }

        private bool MatchFunction(TItem currentItem, TItem goalItem)
        {
            var goalKey = _keySelector(goalItem);
            var currentKey = _keySelector(currentItem);
            var match = _keyEqualityComparer.Equals(goalKey, currentKey);
            if (!match)
            {
                var mappedCurrentKey = _keyMap.GetGoalByCurrent(currentKey);
                if (mappedCurrentKey != default(TKey))
                {
                    match = _keyEqualityComparer.Equals(goalKey, mappedCurrentKey);
                }
            }
            return match;
        }

        public MatchResult<TItem> Match(IEnumerable<TItem> currentItems, IEnumerable<TItem> goalItems)
        {
            var output = new MatchResult<TItem>();

            goalItems = goalItems as TItem[] ?? goalItems.ToArray();
            currentItems = currentItems as TItem[] ?? currentItems.ToArray();

            foreach (var currentItem in currentItems)
            {
                var goalItem = goalItems.SingleOrDefault(s => MatchFunction(currentItem, s));
                if (goalItem == null)
                {
                    output.CurrentOnly.Add(currentItem);
                } 
                else
                {
                    output.Pairs.Add(new CurrentAndGoalPair<TItem>(currentItem, goalItem));
                }
            }

            foreach (var goalItem in goalItems)
            {
                if (!currentItems.Any(t => MatchFunction(t, goalItem)))
                {
                    output.GoalOnly.Add(goalItem);
                }
            }

            return output;
        }
        
    }
}