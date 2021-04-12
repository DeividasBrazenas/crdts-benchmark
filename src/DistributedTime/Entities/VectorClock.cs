using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CRDT.Cluster.Entities;
using CRDT.DistributedTime.Enums;
using CRDT.DistributedTime.Extensions;

namespace CRDT.DistributedTime.Entities
{
    public sealed class VectorClock
    {
        public ImmutableSortedDictionary<Node, long> Values { get; }

        public VectorClock()
        {
            Values = ImmutableSortedDictionary<Node, long>.Empty;
        }

        public VectorClock(ImmutableSortedDictionary<Node, long> values)
        {
            Values = values;
        }

        public bool Equals(VectorClock otherClock)
        {
            return Compare(otherClock) == Order.Same;
        }

        public Order Compare(VectorClock otherClock)
        {
            if (ReferenceEquals(this, otherClock) || Values.SequenceEqual(otherClock.Values))
                return Order.Same;

            return Compare(Values, otherClock.Values);
        }

        public bool IsSameAs(VectorClock otherClock) => Compare(Values, otherClock.Values) == Order.Same;

        public bool IsBefore(VectorClock otherClock) => Compare(Values, otherClock.Values) == Order.Before;

        public bool IsAfter(VectorClock otherClock) => Compare(Values, otherClock.Values) == Order.After;

        public bool IsConcurrentWith(VectorClock otherClock) => Compare(Values, otherClock.Values) == Order.Concurrent;

        public VectorClock Increment(Node node)
        {
            if (!Values.TryGetValue(node, out var currentTimestamp))
            {
                return this;
            }

            return new VectorClock(Values.SetItem(node, currentTimestamp + 1));
        }

        public VectorClock Prune(Node removedNode)
        {
            var newValues = Values.Remove(removedNode);

            return !ReferenceEquals(newValues, Values) ? new VectorClock(newValues) : this;
        }

        public VectorClock Merge(VectorClock otherClock)
        {
            var commonValues = GetCommonValues(Values, otherClock.Values);

            var additionalThisClockValues = Values.Where(v => commonValues.All(c => !Equals(c.Key, v.Key))).ToImmutableList();
            var additionalOtherClockValues = otherClock.Values.Where(v => commonValues.All(c => !Equals(c.Key, v.Key))).ToImmutableList();

            var mergedValues = commonValues
                .ToDictionary(commonValue => commonValue.Key,
                    commonValue => Math.Max(commonValue.Value.Item1, commonValue.Value.Item2));

            foreach (var additionalValue in additionalThisClockValues)
            {
                mergedValues.Add(additionalValue.Key, additionalValue.Value);
            }

            foreach (var additionalValue in additionalOtherClockValues)
            {
                mergedValues.Add(additionalValue.Key, additionalValue.Value);
            }

            return new VectorClock(mergedValues.ToImmutableSortedDictionary());
        }

        private static IEnumerable<KeyValuePair<Node, (long, long)>> GetCommonValues(IEnumerable<KeyValuePair<Node, long>> leftClock,
            IEnumerable<KeyValuePair<Node, long>> rightClock)
        =>
            leftClock
            .Concat(rightClock)
            .GroupBy(d => d.Key)
            .ToDictionary(k => k.Key, v =>
            {
                return v.Select(_ => _).Count() == 2 ? (v.FirstOrDefault().Value, v.Skip(1).FirstOrDefault().Value) : (-1, -1);
            })
            .Where(x => x.Value.Item1 != -1 && x.Value.Item2 != -1);

        /// <summary>
        /// Returns the order of leftClock in comparison with rightClock
        /// </summary>
        private static Order Compare(IEnumerable<KeyValuePair<Node, long>> leftClock,
            IEnumerable<KeyValuePair<Node, long>> rightClock)
        {
            var commonValues = GetCommonValues(leftClock, rightClock);

            var additionalLeftClockValues = leftClock.Where(v => commonValues.All(c => !Equals(c.Key, v.Key))).ToImmutableList();
            var additionalRightClockValues = rightClock.Where(v => commonValues.All(c => !Equals(c.Key, v.Key))).ToImmutableList();

            // If there are additional values on both clocks, they are concurrent
            if (additionalLeftClockValues.Count > 0 && additionalRightClockValues.Count > 0)
            {
                return Order.Concurrent;
            }

            using var commonValuesEnumerator = commonValues.GetEnumerator();

            Order CompareNext(KeyValuePair<Node, (long, long)>? commonValue, Order currentOrder)
            {
                if (commonValue is null)
                {
                    // If there are no additional values, return the actual value from common nodes
                    if (additionalLeftClockValues.Count == 0 && additionalRightClockValues.Count == 0)
                    {
                        return currentOrder;
                    }

                    // If there are additional values on left clock 
                    // and the order of common values is Before, the clocks are concurrent.
                    // Otherwise, the order is After
                    if (additionalLeftClockValues.Count > 0)
                    {
                        return currentOrder == Order.Before ? Order.Concurrent : Order.After;
                    }

                    // If there are additional values on right clock 
                    // and the order of common values is After, the clocks are concurrent.
                    // Otherwise, the order is Before
                    if (additionalRightClockValues.Count > 0)
                    {
                        return currentOrder == Order.After ? Order.Concurrent : Order.Before;
                    }
                }

                var (leftValue, rightValue) = commonValue.Value.Value;

                if (leftValue == rightValue)
                {
                    return CompareNext(commonValuesEnumerator.NextOrNull(), currentOrder);
                }

                if (leftValue < rightValue)
                {
                    return currentOrder == Order.After ? Order.Concurrent : CompareNext(commonValuesEnumerator.NextOrNull(), Order.Before);
                }

                return currentOrder == Order.Before ? Order.Concurrent : CompareNext(commonValuesEnumerator.NextOrNull(), Order.After);
            }

            return CompareNext(commonValuesEnumerator.NextOrNull(), Order.Same);
        }
        // TODO
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // TODO
        public override string ToString()
        {
            return base.ToString();
        }
    }
}