using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAuthService
    {
        // Đăng ký
        Task<ResponseDTO> RegisterAsync(RegisterRequest request);

        // Xác thực OTP
        Task<ResponseDTO> VerifyOtpAsync(VerifyOtpRequest request);

        // Gửi lại OTP
        Task<ResponseDTO> ResendOtpAsync(ResendOtpRequest request);

        // Đăng nhập
        Task<ResponseDTO> LoginAsync(LoginRequest request);

        // Đăng nhập Google
        Task<ResponseDTO> LoginWithGoogleAsync(GoogleLoginRequest request);

        // Refresh token
        Task<ResponseDTO> RefreshTokenAsync(RefreshTokenRequest request);

        // Đăng xuất
        Task<ResponseDTO> LogoutAsync(LogoutRequest request);

        // Quên mật khẩu
        Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordRequest request);

        // Đặt lại mật khẩu
        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequest request);

        // Đổi mật khẩu
        Task<ResponseDTO> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}
