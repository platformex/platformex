﻿using Platformex;
using System;

namespace Siam.MemoContext
{
    [Serializable]
    public class MemoStatusHistory : ValueObject
    {
        public MemoStatusHistory() { }
        public MemoStatusHistory(DateTime changeDate, string userId, MemoStatus status)
        {
            ChangeDate = changeDate;
            UserId = userId;
            Status = status;
        }

        public DateTime ChangeDate { get; set; }
        public string UserId { get; set; }
        public MemoStatus Status { get; set; }
    }
}