using System.IO;
using System.Threading.Tasks;

namespace InstaFaceFam.Vision
{
    public interface IComputerVisionService
    {
        Task<ComputerVisionResults> GetComputerVisionResultsAsync(Stream stream);
    }
}
