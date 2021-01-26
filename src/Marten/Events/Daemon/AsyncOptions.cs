using System;
using System.Collections.Generic;
using Marten.Internal;
using Marten.Internal.Operations;

namespace Marten.Events.Daemon
{
    public class AsyncOptions
    {
        public int BatchSize { get; set; } = 500;
        public int MaximumHopperSize { get; set; } = 2500;

        // TODO -- add an option to just use SQL

        public void DeleteViewTypeOnTeardown<T>()
        {

        }

        public void DeleteViewTypeOnTeardown(Type type)
        {

        }

        private readonly IList<Action<IDocumentOperations>> _actions = new List<Action<IDocumentOperations>>();


        internal void Teardown(IDocumentOperations operations)
        {
            foreach (var action in _actions)
            {
                action(operations);
            }
        }
    }


}
