using System.Collections.Generic;

namespace SmartStore.Core.Search
{
    /// <summary>
    /// Abstraction of document to be stored in the underlying index providers
    /// </summary>
    public interface IIndexDocument : IEnumerable<IndexField>
    {
        /// <summary>
        /// The primary key of the indexed entity
        /// </summary>
        /// <remarks>Implementors: the id must be persisted internally as an <see cref="IndexField"/></remarks>
        int Id { get; }

        /// <summary>
        /// Identifies the type of a document, can be <c>null</c>
        /// </summary>
        SearchDocumentType? DocumentType { get; }

        /// <summary>
        /// The number of fields in this document
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a field to this document
        /// </summary>
        /// <param name="field">The field to add</param>
        void Add(IndexField field);

        /// <summary>
        /// Removes a field from this document
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The number of removed fields</returns>
        /// <remarks>A document can contain fields with the same name. This method removes ALL fields with the passed name.</remarks>
        int Remove(string name);

        /// <summary>
        /// Whether at least one field with the passed name exists in the document
        /// </summary>
        /// <param name="name">The name of the field(s)</param>
        /// <returns><c>true</c> if field exists, <c>false</c> otherwise</returns>
        bool Contains(string name);

        /// <summary>
        /// Enumerates all fields with the passed name
        /// </summary>
        /// <param name="name">The name of the field(s)</param>
        /// <returns>The sequence</returns>
        IEnumerable<IndexField> this[string name] { get; }
    }
}
