
using System.Security.Claims;
using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClothingStoreDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ClothingStoreDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }
        [HttpGet]
        public IActionResult LoginRegister()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View("~/Views/User/Auth/LoginRegister.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Vui lòng nhập email và mật khẩu.";
                TempData["AuthTab"] = "login";
                TempData["LoginEmail"] = email;
                return RedirectToAction(nameof(LoginRegister));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == email);

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
                TempData["AuthTab"] = "login";
                TempData["LoginEmail"] = email;
                return RedirectToAction(nameof(LoginRegister));
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Email hoặc mật khẩu không chính xác.";
                TempData["AuthTab"] = "login";
                TempData["LoginEmail"] = email;
                return RedirectToAction(nameof(LoginRegister));
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.UserRole?.ToString() ?? "0")
    };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string fullName,
            string email,
            string password,
            string confirmPassword)
        {
            fullName = fullName?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin đăng ký.";
                TempData["AuthTab"] = "register";
                return RedirectToAction(nameof(LoginRegister));
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                TempData["AuthTab"] = "register";
                return RedirectToAction(nameof(LoginRegister));
            }

            if (password.Length < 8)
            {
                TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự.";
                TempData["AuthTab"] = "register";
                return RedirectToAction(nameof(LoginRegister));
            }

            if (fullName.Length > 100 || email.Length > 100)
            {
                TempData["Error"] = "Họ tên hoặc email vượt quá độ dài cho phép.";
                TempData["AuthTab"] = "register";
                return RedirectToAction(nameof(LoginRegister));
            }

            if (await _context.Users.AnyAsync(u => u.Username == email))
            {
                TempData["Error"] = "Email này đã được đăng ký.";
                TempData["AuthTab"] = "register";
                return RedirectToAction(nameof(LoginRegister));
            }

            var newUser = new User
            {
                Username = email,
                UserRole = 0
            };

            newUser.Password = _passwordHasher.HashPassword(newUser, password);

            var customer = new Customer
            {
                CustomerId = GenerateCustomerId(),
                Username = email,
                FullName = fullName,
                CustomerType = 0
            };

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(newUser);
                _context.Customers.Add(customer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập bằng tài khoản vừa tạo.";
                TempData["AuthTab"] = "login";
                TempData["LoginEmail"] = email;

                return RedirectToAction(nameof(LoginRegister));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                TempData["Error"] = "Đăng ký chưa thành công. Vui lòng kiểm tra thông tin và thử lại.";
                TempData["AuthTab"] = "register";

                return RedirectToAction(nameof(LoginRegister));
            }
        }

      

        private string GenerateCustomerId()
        {
            // Tạo mã CUS kèm 12 ký tự hex ngẫu nhiên.
            return "CUS" +
                   Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
