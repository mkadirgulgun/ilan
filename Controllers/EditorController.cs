using Dapper;
using IlanSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace IlanSitesi.Controllers
{
    public class EditorController : Controller
    {
        string connectionString = "TrustServerCertificate=True";
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var ilanlar = connection.Query<Ilan>("SELECT * FROM ilanlar ORDER BY UpdatedDate DESC").ToList();

            return View(ilanlar);
        }

        public IActionResult IlanEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IlanEkle(Ilan model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;

            using var connection = new SqlConnection(connectionString);
            var ilanlar = "INSERT INTO ilanlar (Name, Price, CreatedDate, UpdatedDate, ImgUrl, Detail, Email, UserName) VALUES (@Name, @Price, @CreatedDate, @UpdatedDate, @ImgUrl, @Detail, @Email, @UserName)";
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);
            model.ImgUrl = imageName;
            var data = new
            {
                model.Name,
                model.Price,
                model.Detail,
                model.CreatedDate,
                model.UpdatedDate,
                model.ImgUrl,
                model.Email,
                model.UserName
            };

            var rowsAffected = connection.Execute(ilanlar, data);
            ViewBag.Subject = "Rakibinden.com";
            ViewBag.Body = $"Sayın {model.UserName} {model.Name} adlı ilanınız başarıyla eklenmiştir. İlanınız editörlerimiz tarafından onaylandıktan sonra tekrar mail alacaksınız. İyi günler dileriz";
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "İlanınız başarıyla eklendi. İlanınız onaylandıktan sonra yayınlanacaktır.";
            ViewBag.Return = "IlanEkle";
            SendMail(model);
            return View("Message");
        }

        public IActionResult IlanGuncelle()
        {
            using var connection = new SqlConnection(connectionString);
            var ilanlar = connection.Query<Ilan>("SELECT * FROM ilanlar ORDER BY UpdatedDate DESC").ToList();

            return View(ilanlar);
        }

        [HttpPost]
        public IActionResult IlanGuncelle(Ilan model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Geçersiz veri.";
                ViewBag.MessageCssClass = "alert-danger";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var ilanlar = "UPDATE ilanlar SET Name = @Name, Price = @Price, Detail = @Detail, UpdatedDate = @UpdatedDate WHERE Id = @Id";

            var data = new
            {
                model.Name,
                model.Price,
                model.Detail,
                UpdatedDate = DateTime.Now,
                model.Id
            };
            var affectedRows = connection.Execute(ilanlar, data);
            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        public IActionResult IlanSil()
        {
            using var connection = new SqlConnection(connectionString);
            var ilanlar = connection.Query<Ilan>("SELECT * FROM ilanlar ORDER BY UpdatedDate DESC").ToList();

            return View(ilanlar);
        }

        [HttpPost]
        public IActionResult IlanSil(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var ilanlar = "DELETE FROM ilanlar WHERE Id = @Id";
            var rowsAffected = connection.Execute(ilanlar, new { Id = id });
            
            ViewBag.Message = "Silindi.";
            ViewBag.MessageCssClass = "alert-success";
            
            return View("Message");
        }

        public IActionResult SendMail(Ilan model)
        {

            var mailMessage = new MailMessage
            {
                From = new MailAddress("bildirim@rakibinden.com.tr", "Rakibinden.com"),
                //ReplyTo = new MailAddress("info@mkadirgulgun.com.tr", "Mehmet Kadir Gülgün"),
                Subject = ViewBag.Subject,
                Body = ViewBag.Body,
                IsBodyHtml = true,
            };

            mailMessage.ReplyToList.Add(model.Email);
            //mailMessage.To.Add("mkadirgulgun@gmail.com");
            mailMessage.To.Add(new MailAddress($"{model.Email}", $"{model.UserName}"));

            client.Send(mailMessage);
            return RedirectToAction(ViewBag.Return);
        }

        public IActionResult IlanGoster()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM ilanlar WHERE IsApproved = 0";
                var comments = connection.Query<Ilan>(sql).ToList();

                return View(comments);
            }
        }

        public IActionResult IlanOnayla(int? id)
        {
            using var connection = new SqlConnection(connectionString);
            var ilan = connection.QueryFirstOrDefault<Ilan>("SELECT * FROM ilanlar WHERE Id = @Id", new { Id = id });

            var sql = "UPDATE ilanlar SET IsApproved = 1 WHERE Id = @Id";

            var affectedRows = connection.Execute(sql, new { Id = id });
            ViewBag.Subject = "Rakibinden.com İlan Onay";
            ViewBag.Body = $"Sayın {ilan.UserName} {ilan.Name} adlı ilanınız onaylanmıştır. İlanınızı websitemiz üzerinde görüntüleyebilirsiniz. İyi günler dileriz";
            ViewBag.Message = "İlan Onaylandı.";
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Return = "IlanGoster";
            SendMail(ilan);
            return View("Message");
        }
       
        public IActionResult IlanReddet(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var ilan = connection.QueryFirstOrDefault<Ilan>("SELECT * FROM ilanlar WHERE Id = @Id", new { Id = id });
            var sql = "DELETE FROM ilanlar WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });
            ViewBag.Subject = "Rakibinden.com İlan Onay Durumu";
            ViewBag.Body = $"Sayın {ilan.UserName} {ilan.Name} adlı ilanınız maalesef onaylanmamıştır. İlanınızı düzenleyip tekrardan yükleyebilirsiniz. İyi günler dileriz";
            ViewBag.Message = "İlan Reddedildi";
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Return = "IlanGoster";
            SendMail(ilan);
            return View("Message");
        }
    }
}

