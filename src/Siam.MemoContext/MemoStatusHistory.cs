using System;
using Platformex;

namespace Siam.MemoContext
{
    public class MemoStatusHistory : ValueObject
    {
        public MemoStatusHistory(DateTime changeDate, string userId, MemoStatus status)
        {
            ChangeDate = changeDate;
            UserId = userId;
            Status = status;
        }

        public DateTime ChangeDate { get; }
        public string UserId { get; }
        public MemoStatus Status { get; }
    }
}