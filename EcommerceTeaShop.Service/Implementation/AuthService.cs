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


    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }


    private bool IsStrongPassword(string password)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"
        );

        return regex.IsMatch(password);
    }
    // REGISTER
    public async Task<ResponseDTO> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
        {
            return new ResponseDTO
            {
                IsSucess = false,
                Message = "Dữ liệu gửi lên không hợp lệ."
            };
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,

                Message = "Họ và tên không được để trống."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,

                Message = "Email không được để trống."
            };
        }

        if (!IsValidEmail(request.Email))
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,
                Message = "Email không đúng định dạng."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,

                Message = "Mật khẩu không được để trống."
            };
        }

        if (!IsStrongPassword(request.Password))
        {
            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,
                Message = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt."
            };
        }

        request.Email = request.Email.Trim().ToLower();

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
            Role = "User",
            EmailVerified = false,
            EmailOtp = otp,
            EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5)
        };

        await _clientRepo.Insert(client);
        await _unitOfWork.SaveChangeAsync();

        try
        {
            await _emailService.SendEmailAsync(
                client.Email,
                "Xác thực tài khoản TeaVault",
        $@"
Xin chào {client.FullName},

Mã OTP xác thực tài khoản của bạn là: {otp}

OTP có hiệu lực trong 5 phút.

TeaVault System
");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return new ResponseDTO
            {
                IsSucess = false,
                BusinessCode = BusinessCode.EXCEPTION,
                Message = "Không thể gửi email OTP. Vui lòng thử lại."
            };
        }

        return new ResponseDTO
        {
            IsSucess = true,
            BusinessCode = BusinessCode.SIGN_UP_SUCCESSFULLY,
            Message = "Đăng ký thành công. Vui lòng kiểm tra email để lấy mã OTP."
        };
    }    // VERIFY OTP
    public async Task<ResponseDTO> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _clientRepo.GetFirstByExpression(x => x.Email == request.Email);

        if (user == null)
            return new ResponseDTO { IsSucess = false, Message = "Không tìm thấy user." };

        if (user.EmailOtp != request.Otp)
            return new ResponseDTO { IsSucess = false,
                BusinessCode = BusinessCode.VALIDATION_FAILED,
                Message = "OTP không đúng." };

        if (user.EmailOtpExpiry < DateTime.UtcNow)
            return new ResponseDTO { IsSucess = false, Message = "OTP đã hết hạn." };

        user.EmailVerified = true;
        user.EmailOtp = null;
        user.EmailOtpExpiry = null;

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
        request.Email = request.Email.Trim().ToLower();

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

        var oldTokens = await _refreshRepo.GetListByExpression(x => x.ClientId == user.Id && !x.IsRevoked);

        foreach (var t in oldTokens)
        {
            t.IsRevoked = true;
        }

        await _refreshRepo.Insert(new RefreshToken
        {
            Id = Guid.NewGuid(),
            ClientId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
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
    public async Task<ResponseDTO> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var token = await _refreshRepo.GetFirstByExpression(x => x.Token == request.RefreshToken);

        if (token == null || token.IsRevoked || token.ExpiryDate < DateTime.UtcNow)
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

        if (token == null || token.IsRevoked)
            return new ResponseDTO { IsSucess = false, Message = "Token không hợp lệ." };

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

        if (user.EmailOtpExpiry != null && user.EmailOtpExpiry > DateTime.UtcNow)
        {
            return new ResponseDTO
            {
                IsSucess = false,
                Message = "OTP vừa được gửi. Vui lòng thử lại sau."
            };
        }

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
        user.EmailOtp = null;
        user.EmailOtpExpiry = null;

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