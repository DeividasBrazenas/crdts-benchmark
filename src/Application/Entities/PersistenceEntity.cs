using System;
using CRDT.Core.Abstractions;
using CRDT.Core.Cluster;
using CRDT.Core.DistributedTime;

namespace CRDT.Application.Entities
{
    public class PersistenceEntity<T> where T : DistributedEntity
    {
        public Guid Id { get; }

        public T Value { get; }

        public Node UpdatedBy { get; }
  
        public Timestamp Timestamp { get; }

        public PersistenceEntity(T value)
        {
            Id = value.Id;
            Value = value;
        }

        public PersistenceEntity(T value, Node updatedBy, long timestamp)
        {
            Id = value.Id;
            Value = value;
            UpdatedBy = updatedBy;
            Timestamp = new Timestamp(timestamp);
        }

        public PersistenceEntity(T value, Node updatedBy, Timestamp timestamp)
        {
            Id = value.Id;
            Value = value;
            UpdatedBy = updatedBy;
            Timestamp = timestamp;
        }
    }
}