using System.Threading.Tasks;

namespace TodoListApi.Interfaces {
    public interface INoteGrain : Orleans.IGrainWithIntegerKey {
        
        Task<string> getNote();
        Task<string> getDate();
        Task<bool> getFinished();
        Task setFinished(bool finished);
        Task setNote(string note);
        Task setDate(string date);

        Task removeData();

        Task setData(bool finished, string note, string date);

    }
}