using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ContainerProd.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContainerProd.Controllers
{
    [Route("/api/[controller]/[action]")]
    public class DefaultController : Controller
    {
        private ApplicationDbContext db;

        public DefaultController(ApplicationDbContext option)
        {
            db = option;
        }
        public IActionResult Index()
        {
            return Json("Index runs");
        }


        [HttpGet]
        public ActionResult GetBookByTitle(long studentNumber, string session, string bookTitle)
        {
            if (IsAuthenticated(studentNumber, session))
            {
                string _result = "‌Book ‌Not Found";
                if (db.Books != null)
                {
                    var book = db.Books.Where(e => e.title.Contains(bookTitle.Trim())).FirstOrDefault();
                    if (book != null)
                    {
                        return Json(book);
                    }
                    return Json(_result);
                }
                else
                    return Json(_result);
            }
            return Json("Not Authenticate");
        }

        [HttpGet]
        public ActionResult GetAvailableBooks(long studentNumber, string session)
        {
            List<Book> lstbooks = new List<Book>();
            if (IsAuthenticated(studentNumber, session))
            {
                List<Library> lstReserved = new List<Library>();
                lstReserved = db.Libraries.Where(e => e.status == 1).ToList();
                if (lstReserved.Count > 0)
                    lstbooks = db.Books.Where(e => !lstReserved.Select(d => d.book_id).Contains(e.id)).ToList();
                else
                    lstbooks = db.Books.ToList();
                return Json(lstbooks);
            }
            else
            {
                return Json("Not Authenticated");
            }
        }

        [HttpGet]
        public ActionResult GetReservedBooks(long studentNumber, string session)
        {
            List<ReservedBookViewModel> lstbooks = new List<ReservedBookViewModel>();
            if (IsAuthenticated(studentNumber, session))
            {
                List<Library> lstReserved = new List<Library>();
                lstReserved = db.Libraries.Where(e => e.status == 1).ToList();
                foreach (var reserved in lstReserved)
                {
                    var book = db.Books.Where(e => e.id == reserved.book_id).FirstOrDefault();
                    if (book != null)
                    {

                        lstbooks.Add(new ReservedBookViewModel
                        {
                            BeginDate = reserved.date,
                            EndDate = DateTime.Now.ToString(),
                            Number = book.number,
                            Title = book.title
                        });
                    }
                }
                return Json(lstbooks);
            }
            else
            {
                return Json("Not Authenticated");
            }
        }


        [HttpGet]
        public ActionResult myBook(long studentNumber, string session, int stu_id)
        {
            if (IsAuthenticated(studentNumber, session))
            {
                string _result = "‌Empty List Book ";

                List<Book> books = new List<Book>();
                if (db.Libraries != null)
                {

                    return Json(books);
                }
                return Json(_result);
            }
            return Json("Not Authenticate");
        }

        [HttpGet]
        public ActionResult ReserveBook(long studentNumber, string session, int bookId)
        {
            string resultstatus = "Unsuccess";
            if (IsAuthenticated(studentNumber, session))
            {
                try
                {
                    if (db.Libraries.Where(e => e.book_id == bookId).Any())
                        resultstatus = "Duplicate";
                    else
                    {
                        Library lib = new Library { stu_id = studentNumber, book_id = bookId, date = DateTime.Now.ToString(), status = 1 };
                        db.Libraries.Add(lib);
                        db.SaveChanges();
                        resultstatus = "Success";
                    }
                }
                catch (Exception ex)
                {
                    resultstatus = "Unsuccess";
                }
                return Json(resultstatus);
            }
            return Json("Not Authenticate");

        }

        public bool IsAuthenticated(long studentNumber, string session)
        {
            bool result;
            string urlreq = "http://auth_service:8080/authenticate?studentNumber=" + studentNumber + "&session=" + session + "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlreq);
                request.Method = "GET";
                request.KeepAlive = true;
                request.ContentType = "appication/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string myResponse = string.Empty;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    myResponse = sr.ReadToEnd();
                }
                Auth_Response auth_ = null;
                if (myResponse.Length > 0)
                    auth_ = myResponse.FromJson<Auth_Response>();
                if (auth_ != null && auth_.status == "200")
                    result = true;
                else result = false;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }
}