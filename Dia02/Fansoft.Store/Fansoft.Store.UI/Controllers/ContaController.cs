﻿using Fansoft.Store.UI.Data;
using Fansoft.Store.UI.Models;
using Fansoft.Store.UI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Fansoft.Store.UI.Controllers
{
    public class ContaController : Controller
    {
        private readonly FanSoftStoreDataContext _ctx;
        public ContaController(FanSoftStoreDataContext ctx)
        {
            _ctx = ctx;
        }

        public IActionResult Login(string returnUrl) => View(new LoginVM { ReturnUrl=returnUrl});

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {

            var usuario = _ctx.Usuarios.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

            if (usuario == null)
                ModelState.AddModelError("Email", "Email não localizado");
            else
            {
                if (usuario.Senha != model.Senha.Encrypt())
                    ModelState.AddModelError("Senha", "Senha incorreta");
            }



            if (ModelState.IsValid)
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.Email));
                identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nome));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    principal, 
                    new AuthenticationProperties
                {
                    IsPersistent = model.Lembrar
                });

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Produtos");
            }

            return View(model);

        }


        public async  Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


    }
}
