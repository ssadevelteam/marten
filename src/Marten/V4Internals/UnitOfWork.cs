using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Marten.Events;
using Marten.Patching;
using Marten.Schema;
using Marten.Services;
using Marten.Util;

namespace Marten.V4Internals
{
    public class UnitOfWork : IUnitOfWork, IChangeSet
    {
        // TODO -- watch _store.Options.UpdateBatchSize

        private readonly IList<IStorageOperation> _operations = new List<IStorageOperation>();
        private IEnumerable<object> _updated;
        private IEnumerable<object> _inserted;
        private IEnumerable<IDeletion> _deleted;
        private IEnumerable<PatchOperation> _patches;

        /*
            using (var reader = await cmd.ExecuteReaderAsync(tkn).ConfigureAwait(false))
            {
                if (batch.Callbacks.Any())
                {
                    if (batch.Callbacks[0] != null)
                        await batch.Callbacks[0].PostprocessAsync(reader, list, tkn).ConfigureAwait(false);

                    for (var i = 1; i < batch.Callbacks.Count; i++)
                    {
                        if (!(batch.Calls[i - 1] is NoDataReturnedCall))
                        {
                            await reader.NextResultAsync(tkn).ConfigureAwait(false);
                        }

                        if (batch.Callbacks[i] != null)
                        {
                            await batch.Callbacks[i].PostprocessAsync(reader, list, tkn).ConfigureAwait(false);
                        }
                    }
                }
            }
         */


        private bool shouldSort(List<IStorageOperation> operations, out IComparer<IStorageOperation> comparer)
        {
            comparer = null;
            if (operations.Count <= 1)
                return false;

            if (operations.Select(x => x.DocumentType).Distinct().Count() == 1)
                return false;

            var types = _operations
                .Select(x => x.DocumentType)
                .Distinct()
                .TopologicalSort(GetTypeDependencies).ToArray();

            if (operations.OfType<IDeletion>().Any())
            {
                comparer = new StorageOperationWithDeletionsComparer(types);
            }
            else
            {
                comparer = new StorageOperationByTypeComparer(types);
            }

            return true;
        }

        private IEnumerable<Type> GetTypeDependencies(Type type)
        {
            throw new NotImplementedException();
            // var mappingFor = _tenant.MappingFor(type);
            // var documentMapping = mappingFor as DocumentMapping ?? (mappingFor as SubClassMapping)?.Parent;
            // if (documentMapping == null)
            //     return Enumerable.Empty<Type>();
            //
            // return documentMapping.ForeignKeys.Where(x => x.ReferenceDocumentType != type && x.ReferenceDocumentType != null)
            //     .SelectMany(keyDefinition =>
            //     {
            //         var results = new List<Type>();
            //         var referenceMappingType =
            //             _tenant.MappingFor(keyDefinition.ReferenceDocumentType) as DocumentMapping;
            //         // If the reference type has sub-classes, also need to insert/update them first too
            //         if (referenceMappingType != null && referenceMappingType.SubClasses.Any())
            //         {
            //             results.AddRange(referenceMappingType.SubClasses.Select(s => s.DocumentType));
            //         }
            //         results.Add(keyDefinition.ReferenceDocumentType);
            //         return results;
            //     });
        }




        IEnumerable<IDeletion> IUnitOfWork.Deletions()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IDeletion> IUnitOfWork.DeletionsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IDeletion> IUnitOfWork.DeletionsFor(Type documentType)
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IUnitOfWork.Updates()
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IUnitOfWork.Inserts()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.UpdatesFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.InsertsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.AllChangedFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<EventStream> IUnitOfWork.Streams()
        {
            throw new NotImplementedException();
        }

        IEnumerable<PatchOperation> IUnitOfWork.Patches()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IStorageOperation> IUnitOfWork.Operations()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IStorageOperation> IUnitOfWork.OperationsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IStorageOperation> IUnitOfWork.OperationsFor(Type documentType)
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IChangeSet.Updated => _updated;

        IEnumerable<object> IChangeSet.Inserted => _inserted;

        IEnumerable<IDeletion> IChangeSet.Deleted => _deleted;

        IEnumerable<IEvent> IChangeSet.GetEvents()
        {
            throw new NotImplementedException();
        }

        IEnumerable<PatchOperation> IChangeSet.Patches => _patches;

        IEnumerable<EventStream> IChangeSet.GetStreams()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IStorageOperation> operationsFor(Type documentType)
        {
            return _operations.Where(x => x.DocumentType == documentType);
        }

        public void Eject<T>(T document)
        {
            var operations = operationsFor(typeof(T));
            var matching = operations.OfType<IDocumentStorageOperation>().Where(x => object.ReferenceEquals(document, x.Document)).ToArray();

            foreach (var operation in matching)
            {
                _operations.Remove(operation);
            }
        }

        private class StorageOperationWithDeletionsComparer: IComparer<IStorageOperation>
        {
            private readonly Type[] _topologicallyOrderedTypes;

            public StorageOperationWithDeletionsComparer(Type[] topologicallyOrderedTypes)
            {
                _topologicallyOrderedTypes = topologicallyOrderedTypes;
            }

            public int Compare(IStorageOperation x, IStorageOperation y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x?.DocumentType == null || y?.DocumentType == null)
                {
                    return 0;
                }

                // Maintain order if same document type and same operation
                if (x.DocumentType == y.DocumentType && x.GetType() == y.GetType())
                {
                    return 0;
                }

                var xIndex = FindIndex(x);
                var yIndex = FindIndex(y);

                var xIsDelete = x is IDeletion;
                var yIsDelete = y is IDeletion;

                if (xIsDelete != yIsDelete)
                {
                    // Arbitrary order if one is a delete but the other is not, because this will force the sorting
                    // to try and compare these documents against others and fall in to the below checks.
                    return -1;
                }

                if (xIsDelete)
                {
                    // Both are deletes, so we need reverse topological order to inserts, updates and upserts
                    return yIndex.CompareTo(xIndex);
                }

                // Both are inserts, updates or upserts so topological
                return xIndex.CompareTo(yIndex);
            }

            private int FindIndex(IStorageOperation x)
            {
                // Will loop through up the inheritance chain until reaches the end or the index is found, used
                // to handle inheritance as topologically sorted array may not have the subclasses listed
                var documentType = x.DocumentType;
                var index = 0;

                do
                {
                    index = _topologicallyOrderedTypes.IndexOf(documentType);
                    documentType = documentType.BaseType;
                } while (index == -1 && documentType != null);

                return index;
            }
        }

        private class StorageOperationByTypeComparer: IComparer<IStorageOperation>
        {
            private readonly Type[] _topologicallyOrderedTypes;

            public StorageOperationByTypeComparer(Type[] topologicallyOrderedTypes)
            {
                _topologicallyOrderedTypes = topologicallyOrderedTypes;
            }

            public int Compare(IStorageOperation x, IStorageOperation y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x?.DocumentType == null || y?.DocumentType == null)
                {
                    return 0;
                }

                if (x.DocumentType == y.DocumentType)
                {
                    return 0;
                }

                var xIndex = FindIndex(x);
                var yIndex = FindIndex(y);

                return xIndex.CompareTo(yIndex);
            }

            private int FindIndex(IStorageOperation x)
            {
                // Will loop through up the inheritance chain until reaches the end or the index is found, used
                // to handle inheritance as topologically sorted array may not have the subclasses listed
                var documentType = x.DocumentType;
                var index = 0;

                do
                {
                    index = _topologicallyOrderedTypes.IndexOf(documentType);
                    documentType = documentType.BaseType;
                } while (index == -1 && documentType != null);

                return index;
            }
        }

    }
}
