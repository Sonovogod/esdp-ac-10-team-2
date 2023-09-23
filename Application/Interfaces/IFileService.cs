using Application.DataObjects;

namespace Application.Interfaces;

public interface IFileService
{
    public Task<BaseResponse<byte[]>> ProjectWord(string oid);

}