using System;

namespace Siam.MemoContext
{
    [Serializable]
    public enum MemoStatus
    {
        Undefined = 0,
        SigningStarted,
        Signed,
        RejectionStarted,
        Rejected
    }
}