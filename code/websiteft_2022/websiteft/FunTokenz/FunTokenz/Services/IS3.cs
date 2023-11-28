using System.Threading.Tasks;

namespace FunTokenz.Services
{
    public interface IS3
    {
        Task<bool> sendToS3(string data, string path, string ext);
    }
}
