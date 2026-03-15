using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Common.DTOs.Helpers;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using System.Security.Cryptography;
using Google.Apis.Auth;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<Client> _clientRepo;
    private readonly IGenericRepository<RefreshToken> _refreshRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly JwtHelper _jwtHelper;

    private const string ADMIN_EMAIL = "hibana664@gmail.com";

    public AuthService(
        IGenericRepository<Client> clientRepo,
        IGenericRepository<RefreshToken> refreshRepo,
        IUnitOfWork unitOfWork,
        JwtHelper jwtHelper,
        IEmailService emailService)
    {
        _clientRepo = clientRepo;
        _refreshRepo = refreshRepo;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _jwtHelper = jwtHelper;
    }

    // REGISTER
    public async Task<ResponseDTO> RegisterAsync(RegisterRequest request)
    {
        var exist = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (exist != null)
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXISTED_USER,
                Message = "Email đã tồn tại."
            };
        }

        var otp = OtpGenerator.GenerateOtp();

        var client = new Client
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Email == ADMIN_EMAIL ? "Admin" : "User",
            EmailVerified = false,
            EmailOtp = otp,
            EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5)
        };

        await _clientRepo.Insert(client);
        await _unitOfWork.SaveChangeAsync();

        await _emailService.SendEmailAsync(
     client.Email,
     "Xác thực tài khoản TeaVault",
     $@"
Xin chào {client.FullName},

Mã OTP xác thực tài khoản của bạn là: {otp}

OTP có hiệu lực trong 5 phút.

Nếu bạn không yêu cầu đăng ký tài khoản,
vui lòng bỏ qua email này.

TeaVault System
"
 );

        return new ResponseDTO
        {
            BusinessCode = BusinessCode.SIGN_UP_SUCCESSFULLY,
            Message = "Đăng ký thành công. Kiểm tra email để lấy OTP."
        };
    }

    // VERIFY OTP
    public async Task<ResponseDTO> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Không tìm thấy user." };

        if (user.EmailOtp != request.Otp)
            return new ResponseDTO { IsSucess = false, Message = "OTP không đúng." };

        if (user.EmailOtpExpiry < DateTime.UtcNow)
            return new ResponseDTO { IsSucess = false, Message = "OTP đã hết hạn." };

        user.EmailVerified = true;

        await _clientRepo.Update(user);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseDTO
        {
            Message = "Xác thực email thành công."
        };
    }

    // RESEND OTP
    public async Task<ResponseDTO> ResendOtpAsync(ResendOtpRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Không tìm thấy user." };

        var otp = OtpGenerator.GenerateOtp();

        user.EmailOtp = otp;
        user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _clientRepo.Update(user);
        await _unitOfWork.SaveChangeAsync();

        await _emailService.SendEmailAsync(user.Email, "OTP mới", $"OTP: {otp}");

        return new ResponseDTO { Message = "OTP mới đã được gửi." };
    }

    // LOGIN
    public async Task<ResponseDTO> LoginAsync(LoginRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Email không tồn tại." };

        if (!user.EmailVerified)
            return new ResponseDTO { IsSucess = false, Message = "Chưa xác thực email." };

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new ResponseDTO { IsSucess = false, Message = "Sai mật khẩu." };
        if (user.Status == "Blocked")
        {
            return new ResponseDTO
            {
                IsSucess = false,
                Message = "Tài khoản của bạn đã bị khóa."
            };
        }

        var accessToken = _jwtHelper.GenerateToken(new JwtUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        });

        var refreshToken = GenerateRefreshToken();

        await _refreshRepo.Insert(new RefreshToken
        {
            Id = Guid.NewGuid(),
            ClientId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        await _unitOfWork.SaveChangeAsync();

        return new ResponseDTO
        {
            Data = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<ResponseDTO> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var token = await _refreshRepo.GetFirstByExpression(x => x.Token == request.RefreshToken);

        if (token == null || token.ExpiryDate < DateTime.UtcNow)
            return new ResponseDTO { IsSucess = false, Message = "Refresh token không hợp lệ." };

        var user = await _clientRepo.GetById(token.ClientId);

        var accessToken = _jwtHelper.GenerateToken(new JwtUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        });

        return new ResponseDTO
        {
            Data = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = token.Token,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    // LOGOUT
    public async Task<ResponseDTO> LogoutAsync(LogoutRequest request)
    {
        var token = await _refreshRepo.GetFirstByExpression(x => x.Token == request.RefreshToken);

        if (token == null)
            return new ResponseDTO { IsSucess = false, Message = "Token không tồn tại." };

        token.IsRevoked = true;

        await _refreshRepo.Update(token);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseDTO { Message = "Đăng xuất thành công." };
    }

    private string GenerateRefreshToken()
    {
        var random = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return Convert.ToBase64String(random);
    }

    public async Task<ResponseDTO> LoginWithGoogleAsync(GoogleLoginRequest request)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);

        var email = payload.Email;
        var name = payload.Name;

        var user = await _clientRepo.GetFirstByExpression(x => x.Email == email);

        if (user == null)
        {
            user = new Client
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = name,
                Role = "User",
                EmailVerified = true
            };

            await _clientRepo.Insert(user);
            await _unitOfWork.SaveChangeAsync();
        }
        var refreshToken = GenerateRefreshToken();

        await _refreshRepo.Insert(new RefreshToken
        {
            Id = Guid.NewGuid(),
            ClientId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        await _unitOfWork.SaveChangeAsync();

        var accessToken = _jwtHelper.GenerateToken(new JwtUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        });

        return new ResponseDTO
        {
            Data = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,

                Email = user.Email,
                Role = user.Role
            }
        };
    }
    public async Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO
            {
                IsSucess = false,
                Message = "Email không tồn tại."
            };

        var otp = OtpGenerator.GenerateOtp();

        user.EmailOtp = otp;
        user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _clientRepo.Update(user);
        await _unitOfWork.SaveChangeAsync();

        await _emailService.SendEmailAsync(
            user.Email,
            "Đặt lại mật khẩu",
            $"Mã OTP đặt lại mật khẩu của bạn là: {otp}"
        );

        return new ResponseDTO
        {
            Message = "OTP đặt lại mật khẩu đã được gửi."
        };
    }

    public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Không tìm thấy user." };

        if (user.EmailOtp != request.Otp)
            return new ResponseDTO { IsSucess = false, Message = "OTP không đúng." };

        if (user.EmailOtpExpiry < DateTime.UtcNow)
            return new ResponseDTO { IsSucess = false, Message = "OTP đã hết hạn." };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _clientRepo.Update(user);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseDTO
        {
            Message = "Đặt lại mật khẩu thành công."
        };
    }

    public async Task<ResponseDTO> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _clientRepo.GetById(userId);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Không tìm thấy user." };

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            return new ResponseDTO { IsSucess = false, Message = "Mật khẩu cũ không đúng." };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _clientRepo.Update(user);
        await _unitOfWork.SaveChangeAsync();

        return new ResponseDTO
        {
            Message = "Đổi mật khẩu thành công."
        };
    }
}