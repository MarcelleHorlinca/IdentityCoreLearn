using IdentityManager.Core.Data;
using IdentityManager.Core.Data.ViewModel;
using IdentityManager.Core.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace IdentityManager.Web.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            RegisterViewModel registerViewModel = new RegisterViewModel();
            return View(registerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnurl=null)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackurl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your account- Identity Manager",
                        "Confirm your account by clicking here:<a href=\"" + callbackurl + "\"> link </a>");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index","Home");
                }

                AddErrors(result);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if(userId== null || code == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
      
        [HttpGet]
        public async Task<IActionResult> Login(string? returnurl=null)
        {
          
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);


        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
                await _emailSender.SendEmailAsync(model.Email, "Reset password - Identity Manager",
                    "please reset your password by clicking here:<a href=\""+ callbackurl + "\"> link </a>" );
#pragma warning restore CS8604 // Existence possible d'un argument de référence null.

                return RedirectToAction("ForgotPasswordConfirmation");
            }


          return View(model);

        }






        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }



        [HttpGet]
        public IActionResult ResetPassword(string? code=null)
        {

            return code == null? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPassworViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);    
                if(result.Succeeded)
                {
                    return RedirectToAction("Reset password confirmation");
                }

                AddErrors(result);
             }


            return View();

        }



        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /* [HttpGet]
         public async Task<IActionResult> EnableAuthenticator()
         {
             var user = await _userManager.GetUserAsync(User);
             await _userManager.ResetAuthenticatorKeyAsync(user);
             var token= await _userManager.GetAuthenticatorKeyAsync(user);
             var model= new TwoFactorAuthenticationViewModel() { Token=token };
             return View(model);
         }


         [HttpPost]
         public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticationViewModel model)
         {
             if (ModelState.IsValid)
             {
                 var user = await _userManager.GetUserAsync(User);
                 var succeeded= await _userManager.VerifyChangePhoneNumberTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Token);

                 if (succeeded)
                 {
                     await _userManager.SetTwoFactorEnabledAsync(user, true);
                     return View(model);
                 }
                 else
                 {
                     ModelState.AddModelError("verify", " your two factor authentication code could no be avalided");
                 }
             }
             return RedirectToAction(nameof(AuthenticatorConfirmation));

         }
        */
        public IActionResult AuthenticatorConfirmation()
        {
            return View();
        }

       

       /* [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback( string returnurl= null, string remoteError= null)
        {
            returnurl = returnurlurl ?? Url.Content("~/");
            if(remoteError!= null)
            {
                ModelState.AddModelError(string.Empty, err)
            }
        }
       */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");

        }


        public void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
    
}