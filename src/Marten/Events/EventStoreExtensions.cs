using System;
using System.Collections.Generic;
using System.Linq;

namespace Marten.Events
{
    public static class EventStoreExtensions
    {
        /// <summary>
        /// Append one or more events in order to an existing stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="events"></param>
        public static EventStream Append(this IEventStore eventStore, Guid stream, IEnumerable<object> events)
        {
            return eventStore.Append(stream, events.ToArray());
        }

        /// <summary>
        /// Append one or more events in order to an existing stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="events"></param>
        public static EventStream Append(this IEventStore eventStore, string stream, IEnumerable<object> events)
        {
            return eventStore.Append(stream, events.ToArray());
        }

        /// <summary>
        /// Append one or more events in order to an existing stream and verify that maximum event id for the stream
        /// matches supplied expected version or transaction is aborted.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="expectedVersion">Expected maximum event version after append</param>
        /// <param name="events"></param>
        public static EventStream Append(this IEventStore eventStore, Guid stream, int expectedVersion, IEnumerable<object> events)
        {
            return eventStore.Append(stream, expectedVersion, events.ToArray());
        }

        /// <summary>
        /// Append one or more events in order to an existing stream and verify that maximum event id for the stream
        /// matches supplied expected version or transaction is aborted.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="expectedVersion">Expected maximum event version after append</param>
        /// <param name="events"></param>
        public static EventStream Append(this IEventStore eventStore, string stream, int expectedVersion, IEnumerable<object> events)
        {
            return eventStore.Append(stream, expectedVersion, events.ToArray());
        }

        /// <summary>
        /// Creates a new event stream based on a user-supplied Guid and appends the events in order to the new stream
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream<TAggregate>(this IEventStore eventStore, Guid id, IEnumerable<object> events) where TAggregate : class
        {
            return eventStore.StartStream<TAggregate>(id, events.ToArray());
        }

        /// <summary>
        /// Creates a new event stream based on a user-supplied Guid and appends the events in order to the new stream
        ///  - WILL THROW AN EXCEPTION IF THE STREAM ALREADY EXISTS
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="streamKey">String identifier of this stream</param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream<TAggregate>(this IEventStore eventStore, string streamKey, IEnumerable<object> events) where TAggregate : class
        {
            return eventStore.StartStream<TAggregate>(streamKey, events.ToArray());
        }

        /// <summary>
        /// Creates a new event stream based on a user-supplied Guid and appends the events in order to the new stream - WILL THROW AN EXCEPTION IF THE STREAM ALREADY EXISTS
        /// </summary>
        /// <param name="id"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream(this IEventStore eventStore, Guid id, IEnumerable<object> events)
        {
            return eventStore.StartStream(id, events.ToArray());
        }

        /// <summary>
        /// Creates a new event stream based on a user-supplied Guid and appends the events in order to the new stream
        ///  - WILL THROW AN EXCEPTION IF THE STREAM ALREADY EXISTS
        /// </summary>
        /// <param name="streamKey"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream(this IEventStore eventStore, string streamKey, IEnumerable<object> events)
        {
            return eventStore.StartStream(streamKey, events.ToArray());
        }


        /// <summary>
        /// Creates a new event stream, assigns a new Guid id, and appends the events in order to the new stream
        ///  - WILL THROW AN EXCEPTION IF THE STREAM ALREADY EXISTS
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream<TAggregate>(this IEventStore eventStore, IEnumerable<object> events) where TAggregate : class
        {
            return eventStore.StartStream<TAggregate>(events.ToArray());
        }

        /// <summary>
        /// Creates a new event stream, assigns a new Guid id, and appends the events in order to the new stream
        ///  - WILL THROW AN EXCEPTION IF THE STREAM ALREADY EXISTS
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="events"></param>
        /// <returns></returns>
        public static EventStream StartStream(this IEventStore eventStore, IEnumerable<object> events)
        {
            return eventStore.StartStream(events.ToArray());
        }

    }
}
