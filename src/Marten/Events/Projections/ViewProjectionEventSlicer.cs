﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten.Events.Aggregation;
using Marten.Storage;

namespace Marten.Events.Projections
{
    public class ViewProjectionEventSlicer<TDoc, TId>: IViewProjectionEventSlicer<TDoc, TId>
    {
        public List<IGrouper<TId>> Groupers { get; } = new();
        public List<IFanOutRule> Fanouts { get; } = new();

        public virtual async ValueTask<IReadOnlyList<EventSlice<TDoc, TId>>> Slice(IQuerySession querySession,
            IEnumerable<StreamAction> streams, ITenancy tenancy)
        {
            var streamActions = streams.ToList();
            await SetupCustomGroupers(querySession, streamActions.SelectMany(stream => stream.Events).ToList());

            return Slice(streamActions, tenancy).ToList();
        }

        public virtual async ValueTask<IReadOnlyList<TenantSliceGroup<TDoc, TId>>> Slice(IQuerySession querySession,
            IReadOnlyList<IEvent> events, ITenancy tenancy)
        {
            await SetupCustomGroupers(querySession, events);
            var tenantGroups = events.GroupBy(x => x.TenantId);
            var slices = tenantGroups.Select(x => Slice(tenancy[x.Key], x.ToList())).ToList();
            return slices;
        }

        protected virtual IEnumerable<EventSlice<TDoc, TId>> Slice(IEnumerable<StreamAction> streams, ITenancy tenancy)
        {
            var events = streams.SelectMany(x => x.Events);
            var tenantGroups = events.GroupBy(x => x.TenantId);
            foreach (var group in tenantGroups)
            {
                var tenant = tenancy[group.Key];
                foreach (var slice in Slice(tenant, group.ToArray()).Slices)
                {
                    yield return slice;
                }
            }
        }

        protected virtual ValueTask SetupCustomGroupers(IQuerySession querySession, IReadOnlyList<IEvent> events)
        {
            return new ();

        }

        protected TenantSliceGroup<TDoc, TId> Slice(ITenant tenant, IList<IEvent> events)
        {
            var grouping = new EventGrouping<TId>();
            foreach (var grouper in Groupers)
            {
                grouper.Group(events, grouping);
            }

            return grouping.BuildSlices<TDoc>(tenant, Fanouts);
        }
    }
}
