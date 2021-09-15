using Platformex;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Siam.MemoContext
{

    public interface IMemoService : IService
    {
        [Description("Cозданиe заданного количества Памяток")]
        Task<int> CreateMemos(int memoCount);
    }
}