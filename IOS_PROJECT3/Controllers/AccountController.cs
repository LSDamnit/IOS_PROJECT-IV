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
        private DBMergedContext DBContext;
        public AccountController(UserManager<EUser> userManager, SignInManager<EUser> signInManager,DBMergedContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            DBContext = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeSecuritySettings(UserPageViewModel model)
        {
            
            List<string> errors = new List<string>();
            var user = await userManager.FindByIdAsync(model.UserId);
                      

            if (!(String.IsNullOrWhiteSpace(model.OldPassword) &&
                String.IsNullOrWhiteSpace(model.NewPassword) &&
                String.IsNullOrWhiteSpace(model.NewPasswordConfirm)))
            {
                if (String.IsNullOrWhiteSpace(model.OldPassword) ||
                    String.IsNullOrWhiteSpace(model.NewPassword) ||
                    String.IsNullOrWhiteSpace(model.NewPasswordConfirm))
                {
                    errors.Add("Не все поля формы заполнены");
                    return RedirectToAction("Index", "PersonalPage", new { SecuritySettingsErrors = errors });
                }
                
                var oldPassValid = await userManager.CheckPasswordAsync(user, model.OldPassword);
                if (!oldPassValid)
                {
                    errors.Add("Текущий пароль не верный!");
                    return RedirectToAction("Index", "PersonalPage", new { SecuritySettingsErrors = errors });
                }
                if (model.NewPassword != model.NewPasswordConfirm)
                {
                    errors.Add("Пароли не совпадают!");
                    return RedirectToAction("Index", "PersonalPage", new { SecuritySettingsErrors = errors });
                }
                var validator = HttpContext.RequestServices.GetService(typeof(IPasswordValidator<EUser>))
                    as IPasswordValidator<EUser>;
                var validationResult = await validator.ValidateAsync(userManager, user, model.NewPassword);
                if (!validationResult.Succeeded)
                {
                    errors.Add("Пароль не соответствует<abbr" +
                        " style=\"border:none; text-decoration:none;\"" +
                        " title=\"Минимальная длина - 6 символов; " +
                        "Минимум 1 символ латинского алфавита в верхнем регистре; " +
                        "Минимум 1 символ латинского алфавита в нижнем регистре; " +
                        "Минимум 1 цифра; " +
                        "Минимум 1 небуквенно-цифровой символ." +
                        "\"> <b>требованиям безопасности</b></abbr>");

                    return RedirectToAction("Index", "PersonalPage", new { SecuritySettingsErrors = errors });
                }
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "PersonalPage", new { SettingsChanged = true });
                }
                else
                {
                    foreach (var err in result.Errors)
                        errors.Add(err.Description);
                    return RedirectToAction("Index", "PersonalPage", new { SecuritySettingsErrors = errors });
                }
            }
            if (model.BlockOnFails == true)
                user.BlockOnFailedLogins = 1;
            else
                user.BlockOnFailedLogins = 0;

            if (model.NotifyOnLogin == true)
                user.NotifyOnLogins = 1;
            else
                user.NotifyOnLogins = 0;
            await DBContext.SaveChangesAsync();
            return RedirectToAction("Index", "PersonalPage", new { SettingsChanged = true });
        }

        public async Task SendEmailNotification(string UserMail,int About)//1=Login 2=BlockLogin
        {
            
            var alert = new EmailService();
            string[] receiver = new string[1] { UserMail };
            string subj="";
            string message = "";
            if(About==1)
            {
                 subj = "Оповещение о входе в ИОС СГТУ";
                message = "В ваш аккаунт осуществлен вход<br>" +
                   "Дата/Время: " + System.DateTime.Now.ToString("dd'.'MM'.'yyyy HH:mm:ss") +
                   "<br>IP адрес: "+HttpContext.Connection.RemoteIpAddress.ToString()+
                   "<br>Данные о браузере: "+
                   HttpContext.Request.Headers["User-Agent"]+
                "<br><b><Отправлено из системы ИОС СГТУ. Не отвечайте на это сообщение></b>";

            }
            else if(About==2)
            {
                subj = "Вход в ваш аккаунт ИОС СГТУ заблокирован";
                message="Были совершены 3 неудачные попытки входа. Так как вы включили опцию" +
                    "\"Блокировать вход в аккаунт после 3 неудачных попыток входа\" в настройках безопасности," +
                    "вход в ваш аккаунт был заблокирован.<br>" +
                    "<b style=\"font-size:120%\">Для разблокировки аккаунта и сброса пароля обратитесь к администратору.<b>"+
                    "<br><b><Отправлено из системы ИОС СГТУ. Не отвечайте на это сообщение></b>";
            }

            await alert.SendEmailAsync(receiver, subj, message);
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
                    if (failedLogins >= 3&&user.BlockOnFailedLogins==1)
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
                        if (user.NotifyOnLogins == 1)
                           await SendEmailNotification(user.Email, 1);
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        if (user.NotifyOnLogins == 1)
                            await SendEmailNotification(user.Email, 1);
                        return RedirectToAction("Index", "PersonalPage");
                    }
                }
                else
                {
                    if(await userManager.FindByEmailAsync(model.Email)!=null)
                    {
                        EUser user = await userManager.FindByEmailAsync(model.Email);
                        await userManager.AccessFailedAsync(user);
                        if(user.AccessFailedCount==3&&user.BlockOnFailedLogins==1)
                            await SendEmailNotification(user.Email, 2);
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