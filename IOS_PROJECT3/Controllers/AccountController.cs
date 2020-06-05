using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;


namespace IOS_PROJECT3.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<EUser> userManager;
        private readonly SignInManager<EUser> signInManager;

        public AccountController(UserManager<EUser> userManager, SignInManager<EUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await userManager.FindByEmailAsync(model.Email) != null)
                {
                    EUser user = await userManager.FindByEmailAsync(model.Email);
                    int failedLogins = await userManager.GetAccessFailedCountAsync(user);
                    if (failedLogins >= 3)
                    {
                        ModelState.AddModelError("", "Вход заблокирован, обратитесь к администратору для разблокировки");
                        return View(model);//---если ошибка есть, она тут
                    }

                }
                var result =
                    await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    EUser user = await userManager.FindByEmailAsync(model.Email);
                    await userManager.ResetAccessFailedCountAsync(user);
                    //!!!!!!!
                    // проверяем, принадлежит ли URL приложению (узнать точно, зачем он нужен)
                    //!!!!!!!
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    if(await userManager.FindByEmailAsync(model.Email)!=null)
                    {
                        EUser user = await userManager.FindByEmailAsync(model.Email);
                        await userManager.AccessFailedAsync(user);
                    }
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }

}