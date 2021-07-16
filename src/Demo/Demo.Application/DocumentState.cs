using System;
using System.Threading.Tasks;
using Demo.Documents;
using Demo.Documents.Domain;
using Platformex;
using Platformex.Application;
// ReSharper disable ClassNeverInstantiated.Global

namespace Demo.Application
{
    public interface IDocumentModel 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
    }
    public class DocumentState : AggregateState<DocumentId, DocumentState>, IDocumentState,
        ICanApply<DocumentCreated, DocumentId>,
        ICanApply<DocumentRenamed, DocumentId>
    {
        private readonly IDbProvider<IDocumentModel> _provider;
        private IDocumentModel _model;

        public DocumentState(IDbProvider<IDocumentModel> provider)
        {
            _provider = provider;
        }

        public string Name => _model.Name;
        public DateTime CreateDate => _model.CreateDate;

        public void Apply(DocumentCreated e)
        {
            _model.Name = e.Name;
            _model.CreateDate = DateTime.Now;
        }

        public void Apply(DocumentRenamed e)
            => _model.Name = e.NewName;

        protected override async Task LoadStateInternal(DocumentId id)
        {
            _model = await _provider.FindAsync(id.Value) ?? _provider.Create(id.Value);
        }

        protected override async Task AfterApply(IAggregateEvent<DocumentId> id)
        {
            await _provider.SaveChangesAsync(_model);
        }
    }


}