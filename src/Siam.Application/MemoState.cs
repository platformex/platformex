using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;

namespace Siam.Application
{
    public interface IMemoModel
    {
        public string Id { get; set; }
        public MemoDocument Document { get; set; }
        public MemoStatus Status { get; set; }
        public ICollection<MemoStatusHistory> History { get; }
    }
    
    public class MemoState :  AggregateState<MemoId, MemoState>, IMemoState,
    ICanApply<MemoUpdated, MemoId>,
    ICanApply<SigningStarted, MemoId>,
    ICanApply<MemoSigned, MemoId>,
    ICanApply<RejectionStarted, MemoId>,
    ICanApply<MemoRejected, MemoId>
    {
        private readonly IDbProvider<IMemoModel> _provider;
        private IMemoModel _model;

        public MemoDocument Document => _model.Document;
        public MemoStatus Status => _model.Status;
        public IEnumerable<MemoStatusHistory> History => _model.History;


        public MemoState(IDbProvider<IMemoModel> provider)
        {
            _provider = provider;
        }

        protected override async Task LoadStateInternal(MemoId id)
        {
            _model = await _provider.FindAsync(id.Value) ?? _provider.Create(id.Value);
        }

        protected override async Task AfterApply(IAggregateEvent<MemoId> id)
        {
            await _provider.SaveChangesAsync(_model);
        }

        public void Apply(MemoUpdated @event) => _model.Document = @event.Document;

        public void Apply(SigningStarted @event)
        {
            _model.Status = MemoStatus.SigningStarted;
            _model.History.Add(new MemoStatusHistory(DateTime.Now, @event.UserId, _model.Status));
        }

        public void Apply(MemoSigned @event)
        {
            _model.History.Add(new MemoStatusHistory(DateTime.Now, string.Empty, _model.Status));
            _model.Status = MemoStatus.Signed;
        }

        public void Apply(RejectionStarted @event)
        {
            _model.History.Add(new MemoStatusHistory(DateTime.Now, @event.UserId, _model.Status));
            _model.Status = MemoStatus.RejectionStarted;
        }

        public void Apply(MemoRejected @event)
        {
            _model.History.Add(new MemoStatusHistory(DateTime.Now, string.Empty, _model.Status));
            _model.Status = MemoStatus.Rejected;
        }
    }
}