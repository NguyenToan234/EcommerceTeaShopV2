using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class AddressService : IAddressService
{
    private readonly IGenericRepository<Addresses> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(
        IGenericRepository<Addresses> addressRepository,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDTO> AddAddressAsync(Guid clientId, CreateAddressDTO dto)
    {
        ResponseDTO response = new();

        try
        {
            var db = _addressRepository.GetDbContext();

            if (dto.IsDefault)
            {
                var oldDefault = await db.Set<Addresses>()
                    .Where(x => x.ClientId == clientId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in oldDefault)
                {
                    item.IsDefault = false;
                }
            }

            var address = new Addresses
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                AddressLine = dto.AddressLine,
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward,
                IsDefault = dto.IsDefault
            };

            await _addressRepository.Insert(address);

            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.Message = "Thêm địa chỉ thành công";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> GetMyAddressesAsync(Guid clientId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _addressRepository.GetDbContext();

            var addresses = await db.Set<Addresses>()
                .Where(x => x.ClientId == clientId)
                .ToListAsync();

            response.IsSucess = true;
            response.Data = addresses.Select(x => new ReadAddressDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                Phone = x.Phone,
                AddressLine = x.AddressLine,
                City = x.City,
                District = x.District,
                Ward = x.Ward,
                IsDefault = x.IsDefault
            });
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> DeleteAddressAsync(Guid clientId, Guid addressId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _addressRepository.GetDbContext();

            var address = await db.Set<Addresses>()
                .FirstOrDefaultAsync(x => x.Id == addressId && x.ClientId == clientId);

            if (address == null)
            {
                response.IsSucess = false;
                response.Message = "Không tìm thấy địa chỉ.";
                return response;
            }

            await _addressRepository.Delete(address);

            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.Message = "Xóa địa chỉ thành công";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.Message = ex.Message;
        }

        return response;
    }
}